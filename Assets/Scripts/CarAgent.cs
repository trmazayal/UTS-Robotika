using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    private const string HORIZONTAL_MOTION = "Horizontal";
    private const string VERTICAL_MOTION = "Vertical";
    /// <summary>
    /// private variables
    /// </summary>

    [SerializeField] private bool m_allGrounded = false;
    [SerializeField] private int m_obstacleHit;
    [SerializeField] public float m_currentReward;
    [SerializeField] private int m_nextCheckpointNumber;

    private SpawnPointManager m_spawnPointManager;
    public CheckpointManager m_checkpointManager;

    private WheelVehicle m_carController;
    private int m_steps;
    private int m_deadCounter;
    private Vector2 m_move;
    private Rigidbody m_carRigidbody;
    private WheelCollider[] m_wheelColliders;
    private WheelHit m_out;
    // private Checkpoint lastCheckpoint;
    // public Checkpoint nextCheckPointToReach;

    public override void Initialize()
    {
        m_spawnPointManager = FindObjectOfType<SpawnPointManager>();
        m_carController = GetComponent<WheelVehicle>();
        m_carRigidbody = GetComponentInChildren<Rigidbody>();
        m_wheelColliders = GetComponentsInChildren<WheelCollider>();
    }

    public void Respawn()
    {
        Vector3 spawnPosition = m_spawnPointManager.SelectRandomSpawnpoint();
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
        m_carRigidbody.velocity = Vector3.zero;
        m_carRigidbody.angularVelocity = Vector3.zero;
    }

    public override void OnEpisodeBegin()
    {
        m_checkpointManager.ResetCheckpoints();
        Respawn();
        PrivateVariableReset();
        m_obstacleHit = 0;
        m_steps = 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        m_move.x = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        m_move.y = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        m_carController.AgentMove(m_move);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis(HORIZONTAL_MOTION);
        continuousActionsOut[1] = Input.GetAxis(VERTICAL_MOTION);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = m_checkpointManager.GetNextCheckpoint().transform.forward;
        float directionDot = Vector3.Dot(checkpointForward, transform.forward);
        sensor.AddObservation(directionDot);
        // Distance to incoming checkpoint
        // sensor.AddObservation(m_distanceToTarget / 30f); // float

        // Agent's normalized local position
        sensor.AddObservation(new Vector2(transform.localPosition.x / 500f, transform.localPosition.z / 500f)); // vec2

        // Agent's normalized torque and steering angle
        sensor.AddObservation(m_carController.GetTorque()); //float
        sensor.AddObservation(m_carController.GetSteeringAngle()); //float

        // Calculate the direction to incoming checkpoint
        // m_dirToTarget = (m_checkpointPos - transform.localPosition).normalized;

        // Dot product of agent forward and direction to incoming checkpoint/target
        // sensor.AddObservation(Vector3.Dot(transform.forward, m_dirToTarget)); //float

        // sensor.AddObservation(m_carRigidbody.velocity);
        // sensor.AddObservation(m_carRigidbody.angularVelocity);
        // sensor.AddObservation(m_carRigidbody.position);
        // sensor.AddObservation(m_carRigidbody.rotation);
        // sensor.AddObservation(m_obstacleHit);
        // sensor.AddObservation(m_steps);

        // sensor.AddObservation(m_checkpointManager.TimeLeft);
        // sensor.AddObservation(m_checkpointManager.MaxTimeToReachNextCheckpoint);

        // sensor.AddObservation(m_allGrounded);
    }

    private void PrivateVariableReset()
    {        
        // Step count of episode
        m_steps = 0;

        // Movement vector of car
        m_carController.AgentMove(Vector2Int.zero);

        // Variable to store the accumulated reward for debugging purposes
        m_currentReward = 0;

        m_obstacleHit = 0;

        // Variable to count how long the agent moves with speed less than minimum threshold (1 used here)
        m_deadCounter = 0;

        m_nextCheckpointNumber = m_checkpointManager.GetCurrentCheckpointIndex()+1;
    }


    private void OnCollisionStay(Collision collision)
    {
        // Adds negative reward if agent tries to move by pushing the obstacle
        if (collision.collider.CompareTag("Terrain"))
        {
            // m_currentReward += -0.01f;
            AddReward(-0.1f);
            m_currentReward += -0.1f;
        }
        else if (collision.collider.CompareTag("Finish"))
        {
            if (m_checkpointManager.GetCurrentCheckpointIndex() >= m_checkpointManager.GetCheckpointsCount())
            {
                Debug.Log("Reached the final target");
                // A set reward for reaching the final target + extra reward based on how quickly the agent reached the target
                m_currentReward += 1f + ((30f * m_nextCheckpointNumber) / Mathf.Clamp(m_steps, 1, Mathf.Infinity));
                AddReward(1f + ((30f * m_nextCheckpointNumber) / Mathf.Clamp(m_steps, 1, Mathf.Infinity)));
                EndEpisode();
            }
            // If agent reaches the final target without clearing the previous checkpoints
            else
            {
                Debug.Log("Reached the final target without clearing all checkpoints");
                AddReward(-0.1f);
                m_currentReward += -0.1f;
            }
        }

        if (collision.collider.CompareTag("Wall"))
        {
            // m_currentReward += -0.01f;
            AddReward(-0.1f);
            m_currentReward += -0.1f;
        }
    }

    /// <summary>
    /// Checks for collision with obstacles or deadzone i.e. agent fell off the track
    /// </summary>
    private void OnCollisionEnter(Collision other)
    {

        if (other.collider.CompareTag("Wall"))
        {
            AddReward(-0.5f);
            m_currentReward += -0.5f;
        }

        if (other.collider.CompareTag("Terrain"))
        {
            //Debug.Log("Collided with obstacle, negative reward = " + (-1f * (m_obstacleHit + 1) * (m_currentReward / 5f)));
            AddReward(-0.5f);
            m_currentReward += -0.5f;
            // Reset reward to -1 if agent hits obstacle
            // m_currentReward += -1f * (m_obstacleHit + 1) * (m_currentReward / 10f);
            // AddReward(-1f * (m_obstacleHit + 1) * (m_currentReward / 10f));
            // AddReward(-0.5f);
            // m_obstacleHit++;
        }
        else if (other.collider.CompareTag("Finish"))
        {
            if (m_checkpointManager.GetCurrentCheckpointIndex() >= m_checkpointManager.GetCheckpointsCount())
            {
                Debug.Log("Reached the final target");
                // A set reward for reaching the final target + extra reward based on how quickly the agent reached the target
                m_currentReward += 1f + ((30f * m_nextCheckpointNumber) / Mathf.Clamp(m_steps, 1, Mathf.Infinity));
                AddReward(1f + ((30f * m_nextCheckpointNumber) / Mathf.Clamp(m_steps, 1, Mathf.Infinity)));
                EndEpisode();
            }
            // If agent reaches the final target without clearing the previous checkpoints
            else
            {
                Debug.Log("Reached the final target without clearing all checkpoints");
                AddReward(-1f);
                m_currentReward += -1f;
            }
        }
    }

    /// <summary>
    /// Checks for collision with final target or checkpoints
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            // If agent collided with the right checkpoint
            if (other.GetComponent<Checkpoint>().Equals(m_checkpointManager.GetNextCheckpoint()))
            {
                //Debug.Log("Good Checkpoint " + m_nextcheckpointnumber);

                // A set reward for reaching the checkpoint + extra reward based on how quickly the agent reached the checkpoint 
                // and how many obstacles it was able to avoid completely
                // m_currentReward += (0.5f + ((20f * m_nextCheckpointNumber) / Mathf.Clamp(m_steps, 1, Mathf.Infinity)));
                // AddReward(0.5f + ((20f * m_nextCheckpointNumber) / Mathf.Clamp(m_steps, 1, Mathf.Infinity)));

                m_nextCheckpointNumber++;

            }
        }

        // Colliding with outermost boundary
        // else if (other.CompareTag("Respawn"))
        // {
        //     NextEpisode(-1f);
        // }
    }

    private void CheckMovement()
    {
        // CheckDistanceFromCheckpoint();

        if (m_carRigidbody.velocity.magnitude > 1f)
        {
            // Reward based on how far the agent is from the middle of road
            //m_currentReward += (1f / (3000f * m_laneOffset * m_laneOffset * m_laneOffset));
            //AddReward(1f / (3000f * m_laneOffset * m_laneOffset * m_laneOffset));

            // Reward based on the agent moving in the forward direction 
            m_currentReward += (0.0005f * ((Vector3.Dot(m_carRigidbody.velocity.normalized, transform.forward) / 2f) + 0.5f));
            AddReward((0.0005f * ((Vector3.Dot(m_carRigidbody.velocity.normalized, transform.forward) / 2f) + 0.5f)));
        }

        // If agent's speed is too low or its off the track for a certain amount of time then start giving negative rewards
        if (m_carRigidbody.velocity.magnitude < 0.5f || !m_allGrounded)
        {
            m_deadCounter++;
        }

        if (m_carRigidbody.velocity.magnitude > 0.5f && m_allGrounded)
        {
            m_deadCounter = 0;
        }

        // if (m_deadCounter >= 500)
        // {
        //     m_currentReward += -0.001f;
        //     AddReward(-0.001f);
        // }
    }
    /// <summary>
    /// Checks if all wheel colliders are grounded and if all wheels are touching the road or not
    /// </summary>
    private void CheckGrounded()
    {
        foreach (WheelCollider tempcol in m_wheelColliders)
        {
            if (transform.position.y >= 1f || transform.position.y <= 0f)
            {
                tempcol.GetGroundHit(out m_out);
                if (m_out.collider.CompareTag("Road"))
                {
                    m_allGrounded = true;
                    // AddReward(0.001f);
                }
                else
                {
                    NextEpisode(-1f);
                    break;
                }
            }

            if (!tempcol.isGrounded)
            {
                m_allGrounded = false;
                break;
            }

            else
            {
                tempcol.GetGroundHit(out m_out);

                if (m_out.collider.CompareTag("Finish"))
                {
                    NextEpisode(-1f);

                    //m_currentReward += -0.5f;
                    //AddReward(-0.5f);
                }
            }
        }
    }
    /// <summary>
    /// Sets the reward to the given value and ends the episode
    /// </summary>
    public void NextEpisode(float _reward)
    {
        SetReward(_reward);
        m_currentReward = _reward;
        EndEpisode();
    }


    private void FixedUpdate()
    {
        m_steps++;
        m_allGrounded = true;
        CheckGrounded();

        // GetMovementInput();
        // InfiniteRewardCheck();
        // CheckMovement();
    }

    public void GetMovementInput()
    {
        m_move.x = Input.GetAxis(HORIZONTAL_MOTION);
        m_move.y = Input.GetAxis(VERTICAL_MOTION);
        m_carController.AgentMove(m_move);
    }

    public void setCheckpointIndex(int index)
    {
        m_nextCheckpointNumber = index + 1;
    }
}
