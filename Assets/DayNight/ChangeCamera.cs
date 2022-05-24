using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public GameObject FreeCamera;
    public GameObject PersonCamera;
    public Transform PersonCamera1;
    //public Transform FreeCamera1;
    //public Transform PersonCamera1;
   //public Transform PersonCamera2;

    public bool change;

    // Start is called before the first frame update
    void Start()
    {
        change = true;
        FreeCamera.SetActive(true);
        PersonCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (change == false)
            {
                change = true;
                FreeCameraChange();
            }
            else
            {
                change = false;
                PersonCameraChange();
            }
        }
      
    }

    void FreeCameraChange()
    {
        
        FreeCamera.transform.position = PersonCamera.transform.position + new Vector3(0,5,0);
        FreeCamera.transform.localRotation = PersonCamera1.rotation;

        FreeCamera.SetActive(true);
        PersonCamera.SetActive(false);
    }

    void PersonCameraChange()
    {
        PersonCamera1.rotation = FreeCamera.transform.localRotation;
        PersonCamera.transform.position = FreeCamera.transform.position + new Vector3(0,20,0);
        
        FreeCamera.SetActive(false);
        PersonCamera.SetActive(true);
    }
}
