using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    private float engineInput, steeringInput, brakeInput;
    private float speed;
    private float speedClamp;
    [SerializeField] private float maxSpeed;
    private Rigidbody rb;
    [SerializeField] private float brakeForce;
    private float slipAngle;
    [SerializeField] private float motorForce;
    [SerializeField] private float maxSteeringAngle;
    private int isEngineRunning;

    public int IsEngineRunning { get => isEngineRunning; set => isEngineRunning = value; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
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
        if(Mathf.Abs(engineInput) > 0 && isEngineRunning == 0)
        {
            StartCoroutine(GetComponent<EngineAudio>().StartEngine());
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
        if (isEngineRunning > 1)
        {
            if (Mathf.Abs(speed) < maxSpeed)
            {
                BRWheel.motorTorque = motorForce * engineInput;
                BLWheel.motorTorque = motorForce * engineInput;
            }
            else
            {
                BRWheel.motorTorque = 0;
                BLWheel.motorTorque = 0;
            }
        }
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
        return speedClamp * gas / maxSpeed;
    }
}
