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

    private WheelVehicle m_carController;
    private int m_steps;
    private Vector2 m_move;
    private Rigidbody m_carRigidbody;
    private WheelCollider[] m_wheelColliders;
    private WheelHit m_out;

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
        Respawn();
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
        var _actionsOut = actionsOut.ContinuousActions;
        _actionsOut[0] = m_move.x;
        _actionsOut[1] = m_move.y;
        m_carController.AgentMove(m_move);
    }

    public override void CollectObservations(VectorSensor sensor)
    {

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

        else if (other.CompareTag("Waypoint"))
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
                NextEpisode(-1f);
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
    private void NextEpisode(float _reward)
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
