using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    public List<Checkpoint> checkpointList;
    // private List<Checkpoint> checkpointList;
    
    private void Awake()
    {
        // Transform checkpointsTransform = transform.Find("Checkpoints");

        // foreach (Transform checkpointTransform in checkpointsTransform)
        // {
        //     Checkpoint checkpoint = checkpointsTransform.GetComponent<Checkpoint>();

        //     checkpoint.SetCheckpoints(this);

        //     checkpointList.Add(checkpoint);
        // }
        checkpointList = new List<Checkpoint>(GetComponentsInChildren<Checkpoint>());


    }

    public void PlayerThroughCheckpoint(Checkpoint checkpoint)
    {
        Debug.Log(checkpointList.IndexOf(checkpoint));
    }
}
