using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartScreenManager : MonoBehaviour
{
    private VisualElement root;
    private Label callToAction;
    
    private UIDocument pauseMenu;
    private MixerManager mixerManager;


    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        callToAction = root.Q<Label>("CallToAction");
        Time.timeScale = 1;
        
        GameObject pauseMenuObject = GameObject.Find("PauseMenu");

        pauseMenu = pauseMenuObject.GetComponent<UIDocument>();
        pauseMenu.rootVisualElement.visible = false;

        // get the MixerManager
        mixerManager = pauseMenuObject.GetComponent<MixerManager>();
        
        // Callbacks for the three sliders
        Slider masterVolSlider = pauseMenu.rootVisualElement.Q<Slider>("MasterVolSlider");
        masterVolSlider.RegisterValueChangedCallback(v =>
        {
            mixerManager.setVolume("MasterVol", v.newValue);
        });
        
        Slider musicVolSlider = pauseMenu.rootVisualElement.Q<Slider>("MusicVolSlider");
        musicVolSlider.RegisterValueChangedCallback(v =>
        {
            mixerManager.setVolume("MusicVol", v.newValue);
        });
        
        Slider SFXVolSlider = pauseMenu.rootVisualElement.Q<Slider>("SFXVolSlider");
        SFXVolSlider.RegisterValueChangedCallback(v =>
        {
            mixerManager.setVolume("SFXVol", v.newValue);
        });

        InvokeRepeating("blinkCallToAction", 0.25f, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey && !Input.GetKey(KeyCode.Escape) && pauseMenu.rootVisualElement.visible == false)
        {
            SceneManager.LoadScene("SampleScene");
            
            MusicManager musicManager = MusicManager.instance;

            if (musicManager.currentlyCrossfading)
            {
                musicManager.source0Active = !musicManager.source0Active;
                StopCoroutine(musicManager.previousCrossfade);
            }
            
            StartCoroutine(musicManager.SwitchTracks());
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.rootVisualElement.visible = !pauseMenu.rootVisualElement.visible;
            root.visible = !root.visible;

            if (pauseMenu.rootVisualElement.visible)
            {
                CancelInvoke("blinkCallToAction");
                callToAction.visible = false;
                mixerManager.transitionHPF(true);
            }
            else
            {
                InvokeRepeating("blinkCallToAction", 0.25f, 0.25f);
                mixerManager.transitionHPF(false);
            }
        }
    }

    void blinkCallToAction()
    {
        callToAction.visible = !callToAction.visible;
    }
}
