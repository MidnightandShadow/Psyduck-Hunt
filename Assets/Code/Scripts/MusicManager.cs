using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TextCore.Text;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    private AudioSource musicSource0;

    private AudioSource musicSource1;

    private bool source0Active;
    
    [SerializeField] private AudioClip[] tracks;

    [SerializeField] private float numCrossfadeSteps;

    [SerializeField] private float musicVolume;
    [SerializeField] private float fadeDuration;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        musicSource0 = gameObject.AddComponent<AudioSource>();
        musicSource1 = gameObject.AddComponent<AudioSource>();

        musicSource0.volume = musicVolume;
        musicSource0.clip = tracks[0];
        musicSource0.loop = true;

        musicSource1.volume = 0.0f;
        musicSource1.clip = tracks[1];
        musicSource1.loop = true;

        musicSource0.Play();
        source0Active = true;
    }

    public IEnumerator SwitchTracks()
    {
        AudioSource fadeFrom = source0Active ? musicSource0 : musicSource1;
        AudioSource fadeTo = source0Active ? musicSource1 : musicSource0;

        yield return StartCoroutine(CrossFade(fadeFrom, fadeTo, fadeDuration));
    }

    private IEnumerator CrossFade(AudioSource fadeFrom, AudioSource fadeTo, float fadeDuration)
    {
        float stepInterval = fadeDuration / this.numCrossfadeSteps;
        float volInterval = this.musicVolume / this.numCrossfadeSteps;
        
        fadeTo.Play();

        for (int i = 0; i < (int) this.numCrossfadeSteps; i++)
        {
            fadeFrom.volume -= volInterval;
            fadeTo.volume += volInterval;
            yield return new WaitForSeconds(stepInterval);
        }
        
        fadeFrom.Stop();
        source0Active = !source0Active;
    }
}
