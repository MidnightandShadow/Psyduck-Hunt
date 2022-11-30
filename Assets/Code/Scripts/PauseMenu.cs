using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;
    
    [SerializeField] private UniversalRendererData universalRendererData;
    private ScriptableRendererFeature scriptableRendererFeature;
    
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
        scriptableRendererFeature = universalRendererData.rendererFeatures
            .Find(x=>x.name.Equals("RetroBlit"));

        // Callbacks for the Retro Mode toggle
        Toggle retroModeToggle = instance.GetComponent<UIDocument>().rootVisualElement.Q<Toggle>("RetroModeToggle");
        
        retroModeToggle.RegisterValueChangedCallback(v =>
        {
            instance.ToggleRetroMode(v.newValue);
        });
    }

    private void ToggleRetroMode(bool shouldToggle)
    {
        scriptableRendererFeature.SetActive(shouldToggle);
    }
}
