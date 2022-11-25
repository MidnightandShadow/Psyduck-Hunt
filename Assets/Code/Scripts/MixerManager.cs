using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    private Coroutine previousTransitionCoroutine;
    private bool alreadyTransitioning;

    // Called by UI Builder slider's RegisterValueChangedCallback(value => fun)
    public void setVolume(string mixerGroup, float sliderVal)
    {
        // Conversion used because dBs are a logarithmic scale
        audioMixer.SetFloat(mixerGroup, Mathf.Log10(sliderVal) * 20f);
    }

    public void transitionHPF(bool toPauseMenu)
    {
        previousTransitionCoroutine = StartCoroutine(transitionHPFCoroutine(toPauseMenu));
    }

    private IEnumerator transitionHPFCoroutine(bool toPauseMenu)
    {
        if (alreadyTransitioning)
        {
            StopCoroutine(previousTransitionCoroutine);
        }
        
        alreadyTransitioning = true;

        for (int i = 0; i < 100; i++)
        {
            float currValue;
            audioMixer.GetFloat("MasterHPF", out currValue);
            if (toPauseMenu)
            {
                audioMixer.SetFloat("MasterHPF", Mathf.Min(currValue += 20f, 2020f));
            }
            else
            {
                audioMixer.SetFloat("MasterHPF", Mathf.Max(currValue -= 20f, 10f));
            }

            yield return new WaitForSecondsRealtime(0.0025f);
        }

        alreadyTransitioning = false;
    }
}
