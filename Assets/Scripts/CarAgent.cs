using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    /// <summary>
    /// private variables
    /// </summary>

    // [SerializeField] private bool m_allGrounded = false;
    [SerializeField] private int m_obstacleHit;

    private WheelVehicle m_carController;
    private int m_steps;
    private Vector2 m_move;
    private Rigidbody m_carRigidbody;
    private WheelCollider[] m_wheelColliders;


    public override void Initialize()
    {
        m_carController = GetComponent<WheelVehicle>();
        m_carRigidbody = GetComponentInChildren<Rigidbody>();
        m_wheelColliders = GetComponentsInChildren<WheelCollider>();
    }

    public override void OnEpisodeBegin()
    {
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

        // m_allGrounded = true;

        // InfiniteRewardCheck();
        // CheckGrounded();
        // CheckMovement();
    }
}
