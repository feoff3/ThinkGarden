using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.RainMaker
{
    public class DemoScriptStartRainOnSpaceBar : MonoBehaviour
    {
        public BaseRainScript RainScript;

        private void Start()
        {
            if (RainScript == null)
            {
                Debug.Log("Cannot find rain script");
                return;
            }
            RainScript.EnableWind = false;
        }

        private void Update()
        {
            if (RainScript == null)
            {
                Start();
                return;
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Starting rain script");
                RainScript.RainIntensity = 1.0f;
                RainScript.EnableWind = !RainScript.EnableWind;
            }
        }
    }
}