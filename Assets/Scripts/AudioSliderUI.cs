using UnityEngine;
using UnityEngine.UI;

public class AudioSliderUI : MonoBehaviour
{
    public enum SliderType { Master, Music, SFX }
    public SliderType type;
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        LoadSavedValue();
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        if (AudioManager.Instance == null) return;

        switch (type)
        {
            case SliderType.Master:
                AudioManager.Instance.SetMasterVolume(value);
                break;
            case SliderType.Music:
                AudioManager.Instance.SetMusicVolume(value);
                break;
            case SliderType.SFX:
                AudioManager.Instance.SetSFXVolume(value);
                break;
        }
    }

    void LoadSavedValue()
    {
        string key = type switch
        {
            SliderType.Master => "MasterVolume",
            SliderType.Music => "MusicVolume",
            SliderType.SFX => "SFXVolume",
            _ => "MasterVolume"
        };
        slider.value = PlayerPrefs.GetFloat(key, 1f);
    }
}
