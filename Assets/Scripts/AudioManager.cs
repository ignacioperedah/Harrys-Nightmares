using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class ClipEntry
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicMixerParam = "MusicVolume";
    [SerializeField] private string sfxMixerParam = "SFXVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private List<ClipEntry> clips = new List<ClipEntry>();

    private Dictionary<string, ClipEntry> _clipMap;

    private const string VolumePrefKey = "volumenAudio";
    private float _volume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Desanclar del padre antes de DontDestroyOnLoad:
            // DontDestroyOnLoad solo funciona en GameObjects raíz.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        _clipMap = new Dictionary<string, ClipEntry>();
        foreach (var ce in clips)
        {
            if (ce == null || string.IsNullOrEmpty(ce.name) || ce.clip == null) continue;
            if (!_clipMap.ContainsKey(ce.name)) _clipMap.Add(ce.name, ce);
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        _volume = PlayerPrefs.GetFloat(VolumePrefKey, 0.5f);
        ApplyVolumeToSystem(_volume);
    }

    private ClipEntry GetClip(string name)
    {
        if (string.IsNullOrEmpty(name) || _clipMap == null) return null;
        _clipMap.TryGetValue(name, out var ce);
        return ce;
    }

    public void PlaySFX(string name)
    {
        var ce = GetClip(name);
        if (ce == null || sfxSource == null) return;
        sfxSource.PlayOneShot(ce.clip, ce.volume);
    }

    public void PlayMusic(string name, bool loop = true)
    {
        var ce = GetClip(name);
        if (ce == null || musicSource == null) return;
        musicSource.clip = ce.clip;
        musicSource.loop = loop;
        musicSource.volume = ce.volume;
        musicSource.Play();
    }

    public void StopMusic(string name = null)
    {
        if (musicSource == null) return;
        if (!string.IsNullOrEmpty(name) && musicSource.clip != null)
        {
            if (musicSource.clip.name == name) musicSource.Stop();
        }
        else if (string.IsNullOrEmpty(name))
        {
            musicSource.Stop();
        }
    }

    public void ToggleMusic(bool pause)
    {
        if (musicSource == null) return;
        if (pause) musicSource.Pause();
        else musicSource.UnPause();
    }

    public void SetVolume(float val)
    {
        _volume = Mathf.Clamp01(val);
        PlayerPrefs.SetFloat(VolumePrefKey, _volume);
        PlayerPrefs.Save();
        ApplyVolumeToSystem(_volume);
    }

    public float GetVolume() => _volume;

    private void ApplyVolumeToSystem(float val)
    {
        AudioListener.volume = val;

        if (audioMixer != null)
        {
            float dB = Mathf.Lerp(-80f, 0f, val);
            if (!string.IsNullOrEmpty(musicMixerParam)) audioMixer.SetFloat(musicMixerParam, dB);
            if (!string.IsNullOrEmpty(sfxMixerParam))   audioMixer.SetFloat(sfxMixerParam, dB);
        }

        if (musicSource != null) musicSource.volume = val;
        if (sfxSource   != null) sfxSource.volume   = val;
    }
}