using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Groups (Drag manually)")]
    public List<AudioSource> musicSources = new List<AudioSource>();
    public List<AudioSource> sfxSources = new List<AudioSource>();

    [Header("Default Volumes")]
    [Range(0f, 1f)] public float defaultMaster = 1f;
    [Range(0f, 1f)] public float defaultMusic = 0.8f;
    [Range(0f, 1f)] public float defaultSFX = 0.8f;

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumes();
        ApplyVolumes();
    }

    // ============================
    // 🎚️ Volume Control
    // ============================
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = masterVolume; // Global volume
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        foreach (var src in musicSources)
        {
            if (src != null)
                src.volume = musicVolume * masterVolume;
        }
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        foreach (var src in sfxSources)
        {
            if (src != null)
                src.volume = sfxVolume * masterVolume;
        }
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    // ============================
    // 💾 Load + Apply
    // ============================
    public void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMaster);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusic);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFX);
    }

    public void ApplyVolumes()
    {
        AudioListener.volume = masterVolume;

        foreach (var src in musicSources)
            if (src != null)
                src.volume = musicVolume * masterVolume;

        foreach (var src in sfxSources)
            if (src != null)
                src.volume = sfxVolume * masterVolume;
    }

    // Optional helper
    public void RegisterMusic(AudioSource src)
    {
        if (!musicSources.Contains(src))
            musicSources.Add(src);
    }

    public void RegisterSFX(AudioSource src)
    {
        if (!sfxSources.Contains(src))
            sfxSources.Add(src);
    }
}
