using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class GUIManager : MonoBehaviour
{
    private GroupBox timerGB;
    private Label time;
    
    private GroupBox reportGB;
    private Label titleLabel;
    private Label line1Label;
    private Label line2Label;
    private Button returnButton;

    private LevelManager levelManager;
    
    private UIDocument pauseMenu;
    private MixerManager mixerManager;

    private VisualElement root;

    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
        levelManager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        
        timerGB = root.Q<GroupBox>("timer");
        time = root.Q<Label>("time");
        
        reportGB = root.Q<GroupBox>("report");
        titleLabel = root.Q<Label>("title");
        line1Label = root.Q<Label>("line1");
        line2Label = root.Q<Label>("line2");
        returnButton = root.Q<Button>("return");

        returnButton.clicked += returnButtonPressed;

        HideReport();
        
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
    }

    // Update is called once per frame
    void Update()
    {
        if (isDisplayed(timerGB))
        {
            time.text = (levelManager.endTime - levelManager.currentTime).ToString("0");
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.rootVisualElement.visible = !pauseMenu.rootVisualElement.visible;
            root.visible = !root.visible;

            if (pauseMenu.rootVisualElement.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0;
                mixerManager.transitionHPF(true);
            }
            else
            {
                mixerManager.transitionHPF(false);
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
    
    public void reportToPlayer(string title, string line1, float timeVisible)
    {
        if (isDisplayed(reportGB))
        {
            CancelInvoke("HideReport");
        }
        
        setDisplay(timerGB, false);
        setDisplay(reportGB, true);

        titleLabel.text = title;
        line1Label.text = line1;

        titleLabel.visible = true;
        line1Label.visible = true;
        line2Label.visible = false;
        returnButton.visible = false;
        
        Invoke("HideReport", timeVisible);
    }
    
    public void reportToPlayer(string title, string line1, string line2)
    {
        if (isDisplayed(reportGB))
        {
            CancelInvoke("HideReport");
        }
        
        setDisplay(timerGB, false);
        setDisplay(reportGB, true);

        titleLabel.text = title;
        line1Label.text = line1;
        line2Label.text = line2;
        returnButton.text = "Return to Start";

        titleLabel.visible = true;
        line1Label.visible = true;
        line2Label.visible = true;
        returnButton.visible = true;
    }

    private void HideReport()
    {
        setDisplay(timerGB, true);
        setDisplay(reportGB, false);
    }

    private void returnButtonPressed()
    {
        SceneManager.LoadScene("Level/Scenes/Start");
        MusicManager musicManager = MusicManager.instance;
        
        if (musicManager.currentlyCrossfading)
        {
            musicManager.source0Active = !musicManager.source0Active;
            StopCoroutine(musicManager.previousCrossfade);
        }
        
        StartCoroutine(musicManager.SwitchTracks());
    }

    private bool isDisplayed(GroupBox gb)
    {
        return gb.style.display.value.Equals(DisplayStyle.Flex);
    }

    private void setDisplay(GroupBox gb, bool isDisplayed)
    {
        gb.style.display = isDisplayed ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
