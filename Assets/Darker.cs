using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.RainMaker;

public class Darker : NodHandler
{
    bool happening = false;
    bool ended = false;
    public Light sun = null;
    float initial_sky_brightness = 1;
    float initial_sun_intensity = 1.0f;
    public float darker_speed = 2.5f;
    public BaseRainScript RainScript;
    public float rain_delay = 5.0f;
    public float dark_intensity = 0.6f;
    public float rain_intensity = 0.6f;
    public float brighter_delay = 40.0f;
    Brighter brighter_script = null;

    Light[] FindLightsWithName(string name)
    {
        var lights = GameObject.FindObjectsOfType<Light>();
        int a = lights.Length;
        Light[] arr = new Light[a];
        int FluentNumber = 0;
        for (int i = 0; i < a; i++)
        {
            if (lights[i].name == name)
            {
                arr[FluentNumber] = lights[i];
                FluentNumber++;
            }
        }
        System.Array.Resize(ref arr, FluentNumber);
        return arr;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.registered)
        {
            base.Register();
            if (RenderSettings.skybox.HasProperty("_Exposure"))
            {
                initial_sky_brightness = RenderSettings.skybox.GetFloat("_Exposure");
            }
        }

        if (happening && !ended)
        {
            if (sun == null)
                sun = GameObject.Find("Sun").GetComponent<Light>();
            if (sun == null)
                return;
            if (sun)
            {
                sun.intensity -= 0.002f * darker_speed;
            }
            RenderSettings.ambientIntensity -= 0.003f * darker_speed;
            RenderSettings.reflectionIntensity -= 0.003f * darker_speed;
            if (RenderSettings.skybox.HasProperty("_Exposure"))
            {
                float sky_brightness = RenderSettings.skybox.GetFloat("_Exposure");
                sky_brightness -= 0.002f * darker_speed;
                RenderSettings.skybox.SetFloat("_Exposure", sky_brightness);
            }
            RenderSettings.fog = true;
            RenderSettings.fogDensity += 0.00002f * darker_speed;
            if (RenderSettings.fogDensity >= 0.02f)
            {
                RenderSettings.fogDensity = 0.02f;
            }

            if (sun.intensity <= dark_intensity)
            {
                ended = true;
                StartNightLights();
                Invoke("StartRain", rain_delay);
                Invoke("MakeBrighter", brighter_delay);
            }
        }
    }

    private void StartNightLights()
    {
        Light[] lights = FindLightsWithName("NightLight");
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = 5;
        }
    }

    private void StartWind()
    {
        if (RainScript == null)
            return;
        RainScript.EnableWind = true;
    }

    private void StartRain()
    {
        if (RainScript == null)
            return;
        RainScript.RainIntensity = rain_intensity;
    }

    private void StopWind()
    {
        if (RainScript == null)
            return;
        RainScript.EnableWind = false;
    }

    private void StopRain()
    {
        if (RainScript == null)
            return;
        RainScript.RainIntensity = 0.0f;
    }

    private void MakeBrighter()
    {
        brighter_script = GetComponent<Brighter>();
        if (brighter_script != null)
        {
            brighter_script.final_intensity = initial_sun_intensity;
            StopWind();
            StopRain();
            brighter_script.MakeBrighter();
        }
    }


    protected override void OnNod()
    {
        if (happening || ended)
            return;
        happening = true;
        initial_sun_intensity = sun.intensity;
        StartWind();
    }

    void OnApplicationQuit()
    {
        if (RenderSettings.skybox.HasProperty("_Exposure"))
        {
            float sky_brightness = RenderSettings.skybox.GetFloat("_Exposure");
            sky_brightness = initial_sky_brightness;
            RenderSettings.skybox.SetFloat("_Exposure", sky_brightness);
        }
    }
}
