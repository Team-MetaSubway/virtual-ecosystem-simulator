using UnityEngine;
using System.Collections;

namespace DigitalRuby.RainMaker
{
    public class AA : MonoBehaviour
    {
        public RainScript RainScript;
        public UnityEngine.UI.Toggle FlashlightToggle;
        public Light Flashlight;
        
        
        public UnityEngine.UI.Toggle RainToggle;
  
        public void FlashlightChanged(bool val)
        {
            FlashlightToggle.isOn = val;
            Flashlight.enabled = val;
        }
   
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

            if (!RainToggle.isOn)
            {
                RainScript.RainIntensity = 0.0f;
                return;
            }

            else
                RainScript.RainIntensity = 0.5f;
        }

        private void FlashlightStart()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                FlashlightToggle.isOn = !FlashlightToggle.isOn;
            }
        }

        // Use this for initialization
        private void Start()
        {
        
            RainScript.EnableWind = true;

        }

        // Update is called once per frame
        private void Update()
        {
            RainChanged();
            FlashlightStart();
        }

        public void rainInit()
        {
            RainToggle.isOn = true;
            RainChanged();
        }

    }
}