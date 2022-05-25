using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public GameObject FreeCamera;
    public GameObject PersonCamera;
    public Transform PersonCamera1;

    private UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController personCameraController;
    private DigitalRuby.RainMaker.CameraSetting freeCameraController;
    //public Transform FreeCamera1;
    //public Transform PersonCamera1;
    //public Transform PersonCamera2;

    public bool change;

    // Start is called before the first frame update
    void Start()
    {
        personCameraController = PersonCamera.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>();
        freeCameraController = FreeCamera.GetComponent<DigitalRuby.RainMaker.CameraSetting>();
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
        FreeCamera.transform.position = PersonCamera1.transform.position + new Vector3(0,5,0);

        var angleX = PersonCamera1.transform.localRotation.eulerAngles.x;
        if (angleX >= 300f) angleX -= 360f; //360 대신 0, 350 대신 -10, .... 300 대신 -60. Clamp 때문에.
        var angleY = PersonCamera.transform.localRotation.eulerAngles.y;

        freeCameraController.Init(angleX,angleY);

        FreeCamera.SetActive(true);
        PersonCamera.SetActive(false);
    }

    void PersonCameraChange()
    {
        PersonCamera.transform.position = FreeCamera.transform.position + new Vector3(0,20,0);

        PersonCamera.transform.localRotation = Quaternion.Euler(0, FreeCamera.transform.localRotation.eulerAngles.y, 0);
        PersonCamera1.transform.localRotation = Quaternion.Euler(FreeCamera.transform.localRotation.eulerAngles.x, 0, 0); 

        personCameraController.mouseLook.Init(PersonCamera.transform, PersonCamera1.transform);
        
        FreeCamera.SetActive(false);
        PersonCamera.SetActive(true);
    }
}
