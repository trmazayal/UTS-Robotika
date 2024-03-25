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

    private SpawnPointManager m_spawnPointManager;
    private CheckpointManager m_checkpointManager;

    private WheelVehicle m_carController;
    private int m_steps;
    private Vector2 m_move;
    private Rigidbody m_carRigidbody;
    private WheelCollider[] m_wheelColliders;
    private WheelHit m_out;

    public override void Initialize()
    {
        m_spawnPointManager = FindObjectOfType<SpawnPointManager>();
        m_checkpointManager = FindObjectOfType<CheckpointManager>();
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
        m_obstacleHit = 0;
        m_steps = 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        m_move.x = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        m_move.y = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        // Debug.Log("Move: " + m_move.x + " " + m_move.y);
        m_carController.AgentMove(m_move);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var _actionsOut = actionsOut.ContinuousActions;
        _actionsOut[0] = m_move.x;
        _actionsOut[1] = m_move.y;
        m_carController.AgentMove(m_move);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(m_carRigidbody.velocity);
        sensor.AddObservation(m_carRigidbody.angularVelocity);
        sensor.AddObservation(m_carRigidbody.position);
        sensor.AddObservation(m_carRigidbody.rotation);
        sensor.AddObservation(m_obstacleHit);
        sensor.AddObservation(m_steps);

        // sensor.AddObservation(m_checkpointManager.TimeLeft);
        // sensor.AddObservation(m_checkpointManager.MaxTimeToReachNextCheckpoint);

        sensor.AddObservation(m_allGrounded);
    }




    private void OnCollisionStay(Collision collision)
    {
        // Adds negative reward if agent tries to move by pushing the obstacle
        if (collision.collider.CompareTag("Terrain"))
        {
        }
    }

    /// <summary>
    /// Checks for collision with obstacles or deadzone i.e. agent fell off the track
    /// </summary>
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Terrain"))
        {
            m_obstacleHit++;
        }
    }

    /// <summary>
    /// Checks for collision with final target or checkpoints
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
        }

        else if (other.CompareTag("Checkpoint"))
        {
        }

        // Colliding with outermost boundary
        else if (other.CompareTag("Respawn"))
        {
        }
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
}
