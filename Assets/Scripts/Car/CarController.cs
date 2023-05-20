using System.Collections;
using System.Collections.Generic;
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
    private Rigidbody rb;
    [SerializeField] private float brakeForce;
    private float slipAngle;
    [SerializeField] private float motorForce;
    [SerializeField] private float maxSteeringAngle;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
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
        BRWheel.motorTorque = motorForce * engineInput;
        BLWheel.motorTorque = motorForce * engineInput;
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
}
