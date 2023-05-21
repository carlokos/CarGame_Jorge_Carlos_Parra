using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GearState
{
    Neutral,
    Running,
    CheckingChange,
    Changing
};
public class CarController : MonoBehaviour
{
    [Header ("WheelColliders")]
    [SerializeField] private WheelCollider FRWheel;
    [SerializeField] private WheelCollider FLWheel;
    [SerializeField] private WheelCollider BRWheel;
    [SerializeField] private WheelCollider BLWheel;

    [Header ("WheelMeshs")]
    [SerializeField] private MeshRenderer FRMesh;
    [SerializeField] private MeshRenderer FLMesh;
    [SerializeField] private MeshRenderer BRMesh;
    [SerializeField] private MeshRenderer BLMesh;

    [Header("Settings")]
    [SerializeField] private float maxSpeed;
    private float engineInput, steeringInput, brakeInput;
    private float speed;
    private float speedClamp;
    private Rigidbody rb;
    [SerializeField] private float brakeForce;
    private float slipAngle;
    private int isEngineRunning;
    [SerializeField] private float motorForce;
    [SerializeField] private float maxSteeringAngle;
    
    [Header("Engine Properties")]
    [SerializeField] private float redLine;
    [SerializeField] private float idleRPM;
    [SerializeField] private float[] gearRatios;
    [SerializeField] private float differentialRatio;
    [SerializeField] private AnimationCurve horsePower;
    [SerializeField] private float increaseGearRPM;
    [SerializeField] private float decreaseGearRPM;
    [SerializeField] private float changeGearTime = 0.5f;
    private GearState gearState;
    private float currentTorque;
    private float clutch;
    private float RPM;
    private float wheelRPM;
    private int currentGear;
    
    [Header("Canvas")]
    [SerializeField] private TextMeshProUGUI txtRPM;
    [SerializeField] private TextMeshProUGUI txtGear;
    [SerializeField] private Transform needle;
    [SerializeField] private float minNeedleRotation;
    [SerializeField] private float maxNeedleRotation;
    public int IsEngineRunning { get => isEngineRunning; set => isEngineRunning = value; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        needle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, RPM / redLine * 1.1f));
        txtRPM.text = "RPM: " + RPM.ToString("0,000");
        txtGear.text = (gearState==GearState.Neutral)?"N":(currentGear + 1).ToString();
        speed = BRWheel.rpm * BRWheel.radius * 2f * Mathf.PI / 10f;
        speedClamp = Mathf.Lerp(speedClamp, speed, Time.deltaTime);
        rotateWheels();
    }

    private void FixedUpdate()
    {
        getInputs();
        handleMotor();
        handleSteering();
        handleBrake();
    }

    private void getInputs()
    {
        engineInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");

        slipAngle = Vector3.Angle(transform.forward, rb.velocity - transform.forward);

        float movingDirection = Vector3.Dot(transform.forward, rb.velocity);
        if(gearState != GearState.Changing)
        {
            if(gearState == GearState.Neutral)
            {
                clutch = 0;
                if (engineInput > 0) gearState = GearState.Running;
            }
            else
            {
                clutch = Input.GetKey(KeyCode.LeftShift) ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            }
        }
        else
        {
            clutch = 0f;
        }

        if(Mathf.Abs(engineInput) > 0 && isEngineRunning == 0)
        {
            StartCoroutine(GetComponent<EngineAudio>().StartEngine());
            gearState = GearState.Running;
        }

        if (movingDirection < -0.5f && engineInput > 0)
        {
            brakeInput = Mathf.Abs(engineInput);
        }
        else if (movingDirection > 0.5f && engineInput < 0)
        {
            brakeInput = Mathf.Abs(engineInput);
        }
        else
        {
            brakeInput = 0;
        }
    }

    private void handleBrake()
    {
        FRWheel.brakeTorque = brakeInput * brakeForce * 0.7f;
        FLWheel.brakeTorque = brakeInput * brakeForce * 0.7f;

        BRWheel.brakeTorque = brakeInput * brakeForce * 0.3f;
        BLWheel.brakeTorque = brakeInput * brakeForce * 0.3f;
    }

    private void handleMotor()
    {
        currentTorque = CalculateTorque();
        BRWheel.motorTorque = currentTorque * engineInput;
        BLWheel.motorTorque = currentTorque * engineInput;
    }

    private float CalculateTorque()
    {
        float torque = 0;
        if(RPM < idleRPM + 200 && engineInput == 0 && currentGear == 0)
        {
            gearState = GearState.Neutral;
        }
        if(gearState == GearState.Running && clutch > 0)
        {
            if(RPM > increaseGearRPM)
            {
                StartCoroutine(ChangeGear(1));
            } else if(RPM < decreaseGearRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }
        if(isEngineRunning > 0)
        {
            if(clutch < 0.1f)
            {
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * engineInput) + Random.Range(-50, 50), Time.deltaTime);
            }
            else
            {
                wheelRPM = Mathf.Abs((BRWheel.rpm + BLWheel.rpm) / 2f) * gearRatios[currentGear] * differentialRatio;
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3f);
                torque = (horsePower.Evaluate(RPM / redLine) * motorForce / RPM) * gearRatios[currentGear]
                    * differentialRatio * 5252f * clutch;
            }
        }
        return torque;
    }

    private void handleSteering()
    {
        float steeringAngle = steeringInput * maxSteeringAngle;
        FRWheel.steerAngle = steeringAngle;
        FLWheel.steerAngle = steeringAngle;
    }

    private void rotateWheels()
    {
        updateWheel(FRWheel, FRMesh);
        updateWheel(FLWheel, FLMesh);
        updateWheel(BRWheel, BRMesh);
        updateWheel(BLWheel, BLMesh);
    }
    private void updateWheel(WheelCollider col, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        col.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }

    public float GetSpeedRatio()
    {
        var gas = Mathf.Clamp(Mathf.Abs(engineInput), 0.5f, 1f);
        return RPM * gas / redLine;
    }

    private IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (currentGear + gearChange >= 0)
        {
            if (gearChange > 0)
            {
                //increase the gear
                yield return new WaitForSeconds(0.7f);
                if (RPM < increaseGearRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if (gearChange < 0)
            {
                //decrease the gear
                yield return new WaitForSeconds(0.1f);

                if (RPM > decreaseGearRPM || currentGear <= 0)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(changeGearTime);
            currentGear += gearChange;
        }

        if (gearState != GearState.Neutral)
            gearState = GearState.Running;
    }
}
