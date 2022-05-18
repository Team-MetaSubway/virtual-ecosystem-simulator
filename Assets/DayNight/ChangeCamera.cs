using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public Camera FreeCamera;
    public Camera PersonCamera;

 
    // Start is called before the first frame update
    void Start()
    {
        FreeCamera.enabled = false;
        PersonCamera.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            PersonCamera.enabled = false;
            FreeCamera.enabled = true;
        }
    }
}
