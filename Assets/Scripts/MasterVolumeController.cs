using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeController : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;

    [Header("Settings")]
    public string prefsKey = "MasterVolume";
    public float defaultVolume = 1f;

    private void Start()
    {
        // Ambil nilai terakhir dari PlayerPrefs (kalau ada)
        float savedVolume = PlayerPrefs.GetFloat(prefsKey, defaultVolume);
        AudioListener.volume = savedVolume;

        // Sinkron ke UI slider
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    public void OnVolumeChanged(float value)
    {
        // Ubah volume global
        AudioListener.volume = value;

        // Simpan biar persist antar sesi
        PlayerPrefs.SetFloat(prefsKey, value);
        PlayerPrefs.Save();
    }
}
