using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeMaster : MonoBehaviour
{
    public Slider slideroptions;
    public Slider sliderpause;
    public Image imagenMuteoptions;
    public Image imagenMutepause;

    void Start()
    {
        // Now delegate storage and AudioListener.volume to AudioManager
        if (AudioManager.Instance != null)
        {
            float val = AudioManager.Instance.GetVolume();
            slideroptions.value = val;
            sliderpause.value = val;
        }
        else
        {
            // Fallback
            float stored = PlayerPrefs.GetFloat("volumenAudio", 0.5f);
            slideroptions.value = stored;
            sliderpause.value = stored;
            AudioListener.volume = stored;
        }

        RevisarSiestoyMute();
    }

    public void ChangeSlider1()
    {
        float v = slideroptions.value;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(v);
        }
        else
        {
            PlayerPrefs.SetFloat("volumenAudio", v);
            AudioListener.volume = v;
        }
        sliderpause.value = v;
        RevisarSiestoyMute();
    }

    public void ChangeSlider2()
    {
        float v = sliderpause.value;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(v);
        }
        else
        {
            PlayerPrefs.SetFloat("volumenAudio", v);
            AudioListener.volume = v;
        }
        slideroptions.value = v;
        RevisarSiestoyMute();
    }

    public void RevisarSiestoyMute()
    {
        float val = slideroptions != null ? slideroptions.value : 0f;
        bool isMute = Mathf.Approximately(val, 0f);
        if (imagenMuteoptions != null) imagenMuteoptions.enabled = isMute;
        if (imagenMutepause != null) imagenMutepause.enabled = isMute;
    }
}
