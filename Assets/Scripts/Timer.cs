using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Timer : MonoBehaviour
{
    bool stopwatchActive = false;
    float currTime;
    public Text currTimeText;

    // Start is called before the first frame update
    void Start()
    {
        currTime = 0;
        StartStopwatch();
    }

    // Update is called once per frame
    void Update()
    {
        if(stopwatchActive == true){
            currTime = currTime + Time.deltaTime;

        }
        TimeSpan time = TimeSpan.FromSeconds(currTime);
        currTimeText.text = time.Minutes.ToString() + ":" + time.Seconds.ToString();
    }

    public void StartStopwatch() {
        stopwatchActive = true;
    }
    public void StopStopwatch() {
        stopwatchActive = false;
    }
}
