/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class WheelVehicle : MonoBehaviour
{
    /* 
     *  Turn input curve: x real input, y value used
     *  My advice (-1, -1) tangent x, (0, 0) tangent 0 and (1, 1) tangent x
     */
    [SerializeField] AnimationCurve turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

    [Header("Wheels")]
    [SerializeField] WheelCollider[] driveWheel;
    public WheelCollider[] DriveWheel { get { return driveWheel; } }
    [SerializeField] WheelCollider[] turnWheel;

    public WheelCollider[] TurnWheel { get { return turnWheel; } }

    // This code checks if the car is grounded only when needed and the data is old enough
    bool isGrounded = false;
    int lastGroundCheck = 0;
    public bool IsGrounded
    {
        get
        {
            if (lastGroundCheck == Time.frameCount)
                return isGrounded;

            lastGroundCheck = Time.frameCount;
            isGrounded = true;
            foreach (WheelCollider wheel in wheels)
            {
                if (!wheel.gameObject.activeSelf || !wheel.isGrounded)
                    isGrounded = false;
            }
            return isGrounded;
        }
    }

    [Header("Behaviour")]
    /*
     *  Motor torque represent the torque sent to the wheels by the motor with x: speed in km/h and y: torque
     *  The curve should start at x=0 and y>0 and should end with x>topspeed and y<0
     *  The higher the torque the faster it accelerate
     *  the longer the curve the faster it gets
     */
    [SerializeField] AnimationCurve motorTorque = new AnimationCurve(new Keyframe(0, 200), new Keyframe(50, 300), new Keyframe(200, 0));

    // Differential gearing ratio
    [Range(2, 16)]
    [SerializeField] float diffGearing = 4.0f;
    public float DiffGearing { get { return diffGearing; } set { diffGearing = value; } }

    // Basicaly how hard it brakes
    [SerializeField] float brakeForce = 1500.0f;
    public float BrakeForce { get { return brakeForce; } set { brakeForce = value; } }

    // Max steering hangle, usualy higher for drift car
    [Range(0f, 50.0f)]
    [SerializeField] float steerAngle = 45.0f;
    public float SteerAngle { get { return steerAngle; } set { steerAngle = Mathf.Clamp(value, 0.0f, 50.0f); } }

    // The value used in the steering Lerp, 1 is instant (Strong power steering), and 0 is not turning at all
    [Range(0.001f, 1.0f)]
    [SerializeField] float steerSpeed = 0.2f;
    public float SteerSpeed { get { return steerSpeed; } set { steerSpeed = Mathf.Clamp(value, 0.001f, 1.0f); } }


    // How hard do you want to drift?
    // [Range(0.0f, 2f)]
    // [SerializeField] float driftIntensity = 1f;
    // public float DriftIntensity { get { return driftIntensity; } set { driftIntensity = Mathf.Clamp(value, 0.0f, 2.0f); } }

    // Reset Values
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    /*
     *  The center of mass is set at the start and changes the car behavior A LOT
     *  I recomment having it between the center of the wheels and the bottom of the car's body
     *  Move it a bit to the from or bottom according to where the engine is
     */
    [SerializeField] Transform centerOfMass;

    // Force aplied downwards on the car, proportional to the car speed
    [Range(0.5f, 10f)]
    [SerializeField] float downforce = 1.0f;
    public float Downforce { get { return downforce; } set { downforce = Mathf.Clamp(value, 0, 5); } }

    // When IsPlayer is false you can use this to control the steering
    float steering;
    public float Steering { get { return steering; } set { steering = Mathf.Clamp(value, -1f, 1f); } }

    // When IsPlayer is false you can use this to control the throttle
    float throttle;
    public float Throttle { get { return throttle; } set { throttle = Mathf.Clamp(value, -1f, 1f); } }

    // Like your own car handbrake, if it's true the car will not move
    [SerializeField] bool handbrake;
    public bool Handbrake { get { return handbrake; } set { handbrake = value; } }

    // Use this to disable drifting
    // [SerializeField] bool allowDrift = true;
    // bool drift;
    // public bool Drift { get { return drift; } set { drift = value; } }

    // Use this to read the current car speed (you'll need this to make a speedometer)
    [SerializeField] float speed = 0.0f;
    public float Speed { get { return speed; } }


    // Private variables set at the start
    Rigidbody _rb;
    WheelCollider[] wheels;
    [SerializeField]  private Vector2 m_movement;

    // Init rigidbody, center of mass, wheels and more
    void Start()
    {

        _rb = GetComponent<Rigidbody>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        if (_rb != null && centerOfMass != null)
        {
            _rb.centerOfMass = centerOfMass.localPosition;
        }

        wheels = GetComponentsInChildren<WheelCollider>();

        // Set the motor torque to a non null value because 0 means the wheels won't turn no matter what
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 0.0001f;
        }
    }


    public void Move(InputAction.CallbackContext context)
    {
        // Input from new input system
        m_movement = context.ReadValue<Vector2>();

    }

    public float GetSteeringAngle()
    {
        return (turnWheel[0].steerAngle / steerAngle);
    }

    public void AgentMove(Vector2 _move)
    {
        // Input from agent
        m_movement = _move;
    }

    public float GetTorque()
    {
        return (driveWheel[0].motorTorque / 602f);
    }

    [Header("Sensors")]
    public float sensorLength = 15f;
    public Vector3 frontCenterSensorPos = new Vector3(0f, 0, 1.5f);
    public float frontSideSensorPos = 1f;
    public float frontSideAngle = 45f;
    public float frontSideAngle2 = 90f;
    public Vector3 rearLeftSensorLocation = new Vector3(-1f, 0, -1.8f);
    public Vector3 rearRightSensorLocation = new Vector3(1f, 0, -1.8f);
    public float rearSideAngle = 20f;

    private bool avoiding = false;
    private bool turnLeft = false;
    private bool turnRight = false;
    private int randomTurn = 0;

    // Update everything
    void FixedUpdate()
    {
        // Debug.Log("torque = " + driveWheel[0].motorTorque / 602f);
        // Debug.Log("Steering angle = " + turnWheel[0].steerAngle / steerAngle);
        // Mesure current speed
        speed = transform.InverseTransformDirection(_rb.velocity).z * 3.6f;


        throttle = m_movement.y;
        // Debug.Log("Throttle = " + throttle);

        // Implementing the steering with sensors
        if (IsGrounded){
            RaycastHit hit;
            Vector3 sensorStartPos = transform.position + transform.forward * frontCenterSensorPos.z + transform.up * frontCenterSensorPos.y;
            turnLeft = false;
            turnRight = false;
            avoiding = false;

            // Front center sensor
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
                    avoiding = true;
                    randomTurn = Random.Range(0, 1);
                    if (randomTurn == 0)
                    {
                        turnLeft = true;
                    }
                    else
                    {
                        turnRight = true;
                    }
                    Debug.Log("Front center sensor");
                }
            }

            // Front right sensor
            sensorStartPos = transform.position + transform.forward * frontCenterSensorPos.z + transform.right * frontSideSensorPos + transform.up * frontCenterSensorPos.y;
            if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSideAngle, transform.up) * transform.forward, out hit, sensorLength))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
                    turnLeft = true;
                    avoiding = true;
                    Debug.Log("Front right sensor");
                }
            }

            // Front left sensor
            sensorStartPos = transform.position + transform.forward * frontCenterSensorPos.z - transform.right * frontSideSensorPos + transform.up * frontCenterSensorPos.y;
            if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSideAngle, transform.up) * transform.forward, out hit, sensorLength))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
                    turnRight = true;
                    avoiding = true;
                    Debug.Log("Front left sensor");
                }
            }
        }
        // if (avoiding)
        // {
        //     if (turnLeft)
        //     {
        //         steering = turnInputCurve.Evaluate(-1) * steerAngle;
        //     }
        //     else if (turnRight)
        //     {
        //         steering = turnInputCurve.Evaluate(1) * steerAngle;
        //     }
        // }
        // else
            steering = turnInputCurve.Evaluate(m_movement.x) * steerAngle;

        throttle = m_movement.y;
        

        // steering = turnInputCurve.Evaluate(m_movement.x) * steerAngle;
        // Debug.Log("Steering = " + steering);

        // Direction
        foreach (WheelCollider wheel in turnWheel)
        {
            wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, steering, steerSpeed);
        }

        foreach (WheelCollider wheel in wheels)
        {
            wheel.brakeTorque = 0;
        }

        // Handbrake
        if (handbrake)
        {
            foreach (WheelCollider wheel in wheels)
            {
                // Don't zero out this value or the wheel completly lock up
                wheel.motorTorque = 0.0001f;
                wheel.brakeTorque = brakeForce;
            }
        }
        else if (Mathf.Abs(speed) < 65 && Mathf.Sign(speed) == Mathf.Sign(throttle))
        {
            foreach (WheelCollider wheel in driveWheel)
            {
                wheel.motorTorque = throttle * motorTorque.Evaluate(speed) * diffGearing / driveWheel.Length * 2f;
            }
        }
        else
        {
            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = Mathf.Abs(throttle) * brakeForce;
            }
        }

        // Drift
        // if (drift && allowDrift)
        // {
        //     Debug.Log("Drifting");
        //     Vector3 driftForce = -transform.right;
        //     driftForce.y = 0.0f;
        //     driftForce.Normalize();

        //     if (steering != 0)
        //         driftForce *= _rb.mass * speed / 7f * throttle * steering / steerAngle;
        //     Vector3 driftTorque = transform.up * 0.1f * steering / steerAngle;


        //     _rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
        //     _rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);
        // }

        // Downforce
        _rb.AddForce(-transform.up * speed * downforce);
    }

    // // Reposition the car to the start position
    // public void ResetPos()
    // {
    //     transform.position = spawnPosition;
    //     transform.rotation = spawnRotation;

    //     _rb.velocity = Vector3.zero;
    //     _rb.angularVelocity = Vector3.zero;
    // }

    public void toogleHandbrake(bool h)
    {
        handbrake = h;
    }

}
