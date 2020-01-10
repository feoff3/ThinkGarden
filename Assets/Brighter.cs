using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brighter : MonoBehaviour
{
    bool happening = false;
    bool ended = false;
    public Light sun = null;
    public float final_intensity = 1.0f;
    public float brighter_speed = 4.0f;
    public GameObject enable_shadows_for = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (happening && !ended)
        {
            if (sun == null)
                sun = GameObject.Find("Sun").GetComponent<Light>();
            if (sun == null)
                return;
            if (sun)
            {
                sun.intensity += 0.002f * brighter_speed;
            }
            RenderSettings.ambientIntensity += 0.003f * brighter_speed;
            RenderSettings.reflectionIntensity += 0.003f * brighter_speed;
            if (RenderSettings.skybox.HasProperty("_Exposure"))
            {
                float sky_brightness = RenderSettings.skybox.GetFloat("_Exposure");
                sky_brightness += 0.002f * brighter_speed;
                RenderSettings.skybox.SetFloat("_Exposure", sky_brightness);
            }
            RenderSettings.fog = false;
            if (sun.intensity >= final_intensity)
            {
                ended = true;
                EnableShadows();
            }
        }
    }

    public void EnableShadows()
    {
        var components = enable_shadows_for.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < components.Length; i++)
        {
            components[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }

    public void MakeBrighter()
    {
        happening = true;
    }

}
