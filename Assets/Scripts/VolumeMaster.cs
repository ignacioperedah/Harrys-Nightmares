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


    // Start is called before the first frame update
    void Start()
    {
        slideroptions.value = PlayerPrefs.GetFloat("volumenAudio", 0.5f);
        AudioListener.volume = slideroptions.value;
        sliderpause.value = slideroptions.value;

        RevisarSiestoyMute();
    }

    public void ChangeSlider1()
    { 
        PlayerPrefs.SetFloat("volumenAudio", slideroptions.value);
        AudioListener.volume = slideroptions.value;
        sliderpause.value = slideroptions.value;
        RevisarSiestoyMute();
    }

    public void ChangeSlider2()
    {
        PlayerPrefs.SetFloat("volumenAudio", sliderpause.value);
        AudioListener.volume = sliderpause.value;
        slideroptions.value = sliderpause.value;
        RevisarSiestoyMute();
    }

    public void RevisarSiestoyMute()
    {
        if (slideroptions.value == 0)
        {
            imagenMuteoptions.enabled = true;
            imagenMutepause.enabled = true;
        }
        else
        {
            imagenMuteoptions.enabled = false;
            imagenMutepause.enabled = false;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
