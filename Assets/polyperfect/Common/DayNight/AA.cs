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
        //
        public GameObject DayNight;
        [Header("비오는 날짜")]
        public float startDay;
        public float finishDay;
        public bool changeCheck;

        private DayNightSystem DayNightTime;
        //
        public void FlashlightChanged(bool val)
        {
            FlashlightToggle.isOn = val;
            Flashlight.enabled = val;
        }
   
        public void Rain(bool val)
        {
            RainToggle.isOn = val;
            /*
            if (RainToggle.isOn)
                RandomObjectGenerator.instance.RainEffect();
            else
                RandomObjectGenerator.instance.RainEffect2();
            */
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
                RainScript.RainIntensity = 1.0f;
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
            DayNightTime = DayNight.GetComponent<DayNightSystem>();
            changeCheck = false;
        }

        // Update is called once per frame
        private void Update()
        {
            if (startDay <= DayNightTime.Day && DayNightTime.Day <= finishDay)
            {
                if (!changeCheck)
                {
                    Rain(true);
                    RainChanged();
                }
                else
                {
                    changeCheck = false;
                    Rain(false);
                    Invoke("rainInit", 0.2f);
                }
            }
            else if (DayNightTime.Day == (finishDay + 1) && DayNightTime.time == 0.0f)
            {
                Rain(false);
                RainChanged();
            }

            RainChanged();
            FlashlightStart();
        }

        public void rainDelay()
        {
            Rain(false);
            Invoke("rainInit", 0.2f);
        }

        public void rainInit()
        {
            Rain(true);           
            RainChanged();
        }

        public void rainStop()
        {
            Rain(false);
        }

    }
}