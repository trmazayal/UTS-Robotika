using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

    private Checkpoints checkpoints;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CheckpointManager>() != null)
        {
            other.GetComponent<CheckpointManager>().CheckPointReached(this);
            Debug.Log("Through Checkpoint");
        }
    }

    public void SetCheckpoints(Checkpoints checkpoints)
    {
        this.checkpoints = checkpoints;
    }
}
