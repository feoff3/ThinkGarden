using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceLoudnessTester : MonoBehaviour
{
    
    public AudioSource audioSource;
    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;

    private float currentUpdateTime = 0f;

    private float clipLoudness;
    private float[] clipSampleData;


    // Start is called before the first frame update
    void Start()
    {
        clipSampleData = new float[sampleDataLength];
        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (audioSource == null)
            Start();
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {
            currentUpdateTime = 0f;
            if (audioSource.clip)
            {
                audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                clipLoudness = 0f;
                foreach (var sample in clipSampleData)
                {
                    clipLoudness += Mathf.Abs(sample);
                }
                clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for
            }
            else
                clipLoudness = 0f;
        }
    }

    public float GetCurrentLoudness()
    {
        return clipLoudness;
    }
}
