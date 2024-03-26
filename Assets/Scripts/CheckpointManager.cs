using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class CheckpointManager : MonoBehaviour
{
    public float MaxTimeToReachNextCheckpoint = 30f;
    public float TimeLeft = 30f;
    
    public CarAgent carAgent;
    public Checkpoint nextCheckPointToReach;
    
    private int CurrentCheckpointIndex;
    private List<Checkpoint> Checkpoints;
    private Checkpoint lastCheckpoint;

    public event Action<Checkpoint> reachedCheckpoint; 

    void Start()
    {
        Checkpoints = FindObjectOfType<Checkpoints>().checkpointList;
        ResetCheckpoints();
    }

    public void ResetCheckpoints()
    {
        // Debug.Log("Before Reset: " + CurrentCheckpointIndex);
        CurrentCheckpointIndex = 0;
        // Debug.Log("After Reset: " + CurrentCheckpointIndex);
        TimeLeft = MaxTimeToReachNextCheckpoint;
        
        SetNextCheckpoint();
    }

    private void Update()
    {
        TimeLeft -= Time.deltaTime;

        // if (TimeLeft < 0f)
        // {
        //     Debug.Log("Time Ran Out!");
        //     carAgent.NextEpisode(-1f);
        // }
    }

    public void CheckPointReached(Checkpoint checkpoint)
    {
        lastCheckpoint = Checkpoints[CurrentCheckpointIndex];

        if (Checkpoints.IndexOf(checkpoint) < Checkpoints.IndexOf(lastCheckpoint)) {
            return;
        }
        else if (nextCheckPointToReach != checkpoint) {
            Debug.Log("Wrong Checkpoint!"+ checkpoint.name + " Expected: " + nextCheckPointToReach.name);
            carAgent.AddReward(-1f);
            carAgent.m_currentReward += -1f;
            // carAgent.EndEpisode();
            return;
        }

        
        reachedCheckpoint?.Invoke(checkpoint);
        CurrentCheckpointIndex++;
        carAgent.setCheckpointIndex(CurrentCheckpointIndex);

        if (CurrentCheckpointIndex >= Checkpoints.Count)
        {
            // carAgent.AddReward(1f);
            // carAgent.EndEpisode();
        }
        else
        {
            Debug.Log("Correct Checkpoint! "+ checkpoint.name);
            // Debug.Log("Before Checkpoint! "+ carAgent.m_currentReward);
            carAgent.AddReward(10f);
            carAgent.m_currentReward += 10f;
            // Debug.Log("After Checkpoint! "+ carAgent.m_currentReward);
            SetNextCheckpoint();

        }
    }

    private void SetNextCheckpoint()
    {
        if (Checkpoints.Count > 0)
        {
            TimeLeft = MaxTimeToReachNextCheckpoint;
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
            // Debug.Log("After Set Checkpoint: " + CurrentCheckpointIndex);
        }
    }

    private void ResetNextCheckpoint()
    {
        if (Checkpoints.Count > 0)
        {
            TimeLeft = MaxTimeToReachNextCheckpoint;
            nextCheckPointToReach = Checkpoints[0];
            Debug.Log("Reset Checkpoint: " + Checkpoints.IndexOf(nextCheckPointToReach));
        }
    }


    public Checkpoint GetLastCheckpoint()
    {
        return lastCheckpoint;
    }

    public Checkpoint GetNextCheckpoint()
    {
        return nextCheckPointToReach;
    }

    public int GetCurrentCheckpointIndex()
    {
        return CurrentCheckpointIndex;
    }

    public int GetCheckpointsCount()
    {
        return Checkpoints.Count;
    }
}