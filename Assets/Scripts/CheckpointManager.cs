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
        CurrentCheckpointIndex = 0;
        TimeLeft = MaxTimeToReachNextCheckpoint;
        
        SetNextCheckpoint();
    }

    private void Update()
    {
        TimeLeft -= Time.deltaTime;

        if (TimeLeft < 0f)
        {
            carAgent.AddReward(-1f);
            carAgent.EndEpisode();
        }
    }

    public void CheckPointReached(Checkpoint checkpoint)
    {
        if (nextCheckPointToReach != checkpoint) {
            // Debug.Log("Wrong Checkpoint!"+ checkpoint.name + " Expected: " + nextCheckPointToReach.name);
            carAgent.AddReward(-1f);
            carAgent.EndEpisode();
            return;
        }
        lastCheckpoint = Checkpoints[CurrentCheckpointIndex];
        reachedCheckpoint?.Invoke(checkpoint);
        CurrentCheckpointIndex++;
        carAgent.setCheckpointIndex(CurrentCheckpointIndex);

        if (CurrentCheckpointIndex >= Checkpoints.Count)
        {
            carAgent.AddReward(1f);
            carAgent.EndEpisode();
        }
        else
        {
            // Debug.Log("Correct Checkpoint! "+ checkpoint.name);
            carAgent.AddReward(1f);
            SetNextCheckpoint();

        }
    }

    private void SetNextCheckpoint()
    {
        if (Checkpoints.Count > 0)
        {
            TimeLeft = MaxTimeToReachNextCheckpoint;
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
            
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