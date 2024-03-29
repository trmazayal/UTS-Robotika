using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapTime : MonoBehaviour
{

    [SerializeField] private Stopwatch stopwatch;


    List<float> lapTimes = new List<float>();

    public void AddLapTime()
    {
        lapTimes.Add(stopwatch.elapsedTime);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            AddLapTime();
        }
    }
}
