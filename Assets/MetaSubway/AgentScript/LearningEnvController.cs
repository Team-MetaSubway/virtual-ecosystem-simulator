using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LearningEnvController : MonoBehaviour
{
    // Start is called before the first frame update

    [Tooltip("size x of the map, x value")]
    public float mapWidth = 50f;
    [Tooltip("size z of the map, z value")]
    public float mapLength = 50f;
    [Tooltip("maximum height of the map, y value")]
    public float mapMaxHeight = 100f;

    public static LearningEnvController instance = null;
    private void Awake()
    {
        mapWidth *= 0.95f;
        mapLength *= 0.95f;
        instance = this;
    }
}
