using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearningEnvController : MonoBehaviour
{
    // Start is called before the first frame update

    bool isTimeout = false;
    public bool IsTimeout
    {
        get { return isTimeout; }
    }

    int timer = 0;

    [Tooltip("Environment Steps")] 
    public int learningStep = 500;

    void Start()
    {
        timer = 0;
    }

    void FixedUpdate()
    {
        ++timer;
        if (timer >= learningStep) isTimeout = true;
    }

    public void resetTimer()
    {
        timer = 0;
    }
}
