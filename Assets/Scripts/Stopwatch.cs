using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Stopwatch : MonoBehaviour
{
    public float elapsedTime = 0.0f;
    public float prevLabTime = 0.0f;
    List<float> lapTimes = new List<float>();
    public Text stopwatchText;
    public Text prevLabTimeText;
    public Text bestLabTimeText;


    public bool TimerOn = true;


    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (TimerOn)
        {
            stopwatchText.text = "Timer: " + FormatTime(elapsedTime);
        }

    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }


    private void OnCollisionEnter(Collision collision)
    {

        if (collision.transform.CompareTag("Player"))
        {
            TimerOn = false;
            lapTimes.Add(elapsedTime);
            prevLabTime = elapsedTime;
            prevLabTimeText.text = "Previous: " + FormatTime(prevLabTime);
            lapTimes.Sort();
            bestLabTimeText.text = "Best: " + FormatTime(lapTimes[0]);
            elapsedTime = 0.0f;
            TimerOn = true;
        }
    }


}