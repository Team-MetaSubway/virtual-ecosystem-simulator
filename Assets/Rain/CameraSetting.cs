using UnityEngine;
using System.Collections;

namespace DigitalRuby.RainMaker
{
    public class CameraSetting: MonoBehaviour
    {
        public RainScript RainScript;
        public UnityEngine.UI.Toggle MouseLookToggle;
        public UnityEngine.UI.Toggle FlashlightToggle;
        public UnityEngine.UI.Toggle RainToggle;
        public Light Flashlight;
       
        public GameObject DayNight;
        private DayNightSystem DayNightTime;

        private enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
        private RotationAxes axes = RotationAxes.MouseXAndY;
        private float sensitivityX = 6F;
        private float sensitivityY = 6F;
        private float minimumX = -360F;
        private float maximumX = 360F;
        private float minimumY = -60F;
        private float maximumY = 60F;
        private float rotationX = 0F;
        private float rotationY = 0F;
        private Quaternion originalRotation;
        
        private void UpdateMovement()
        {
            
            float speed = 15.0f * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift)){
                speed *= 2;
            }
            else
            {
                speed = 15.0f * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.W))
            {
                Camera.main.transform.Translate(0.0f, 0.0f, speed);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Camera.main.transform.Translate(0.0f, 0.0f, -speed);
            }
            if (Input.GetKey(KeyCode.A))
            {
                Camera.main.transform.Translate(-speed, 0.0f, 0.0f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Camera.main.transform.Translate(speed, 0.0f, 0.0f);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                FlashlightToggle.isOn = !FlashlightToggle.isOn;
            }

            if (Input.GetKey(KeyCode.E))
            {
                Camera.main.transform.Translate(0.0f, speed, 0.0f);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                Camera.main.transform.Translate(0.0f, -speed, 0.0f);
            }

        }

        private void UpdateMouseLook()
        {
            /*
            if (Input.GetKeyDown(KeyCode.M))
            {
                MouseLookToggle.isOn = !MouseLookToggle.isOn;
            }
            */
            /*
            if (!MouseLookToggle.isOn)
            {
                return;
            }
            */
            if (axes == RotationAxes.MouseXAndY)
            {
                // Read the mouse input axis
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationY -= Input.GetAxis("Mouse Y") * sensitivityY;

                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.right);

                transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            }
            else if (axes == RotationAxes.MouseX)
            {
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationX = ClampAngle(rotationX, minimumX, maximumX);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                transform.localRotation = originalRotation * xQuaternion;
            }
            else if (axes == RotationAxes.MouseY)
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
                transform.localRotation = originalRotation * yQuaternion;
            }
            transform.localRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        }
        /*
        public void RainSliderChanged(float val)
        {
            RainScript.RainIntensity = val;
        }
        */
        /*
        public void MouseLookChanged(bool val)
        {
            MouseLookToggle.isOn = val;
        }
        */
        public void FlashlightChanged(bool val)
        {
            FlashlightToggle.isOn = val;
            Flashlight.enabled = val;
        }
        //
        
        public void Rain(bool val)
        {
            
            RainToggle.isOn = val;
            
        }
        
        private void RainChanged()
        {
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                RainToggle.isOn = !RainToggle.isOn;
            }
           

            if(!RainToggle.isOn)
            {
                RainScript.RainIntensity = 0.0f;
                    return;
            }
                 
            else
                RainScript.RainIntensity = 1.0f;
            
        }
        //
      
        // Use this for initialization
        private void Start()
        {
            originalRotation = transform.localRotation;
           
            RainScript.EnableWind = true;

            MouseLookToggle.isOn = true;

            DayNightTime = DayNight.GetComponent<DayNightSystem>();

            Rain(false);
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Break();
            }
            /*
            if ( -1 < DayNightTime.Day && DayNightTime.Day < 3)
            {
                Rain(true);
                RainChanged();
            }
            else
            {
                Rain(false);
                RainChanged();
            }
            */
            RainChanged();
            UpdateMovement();
            UpdateMouseLook();
            
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }

            return Mathf.Clamp(angle, min, max);
        }

        public void Init(float x, float y)
        {
            rotationX = y;
            rotationY = x;
            UpdateMouseLook();
        }
        
        public void rainInit(bool check)
        {
            Rain(check);
            RainChanged();
        }
        
    }
}