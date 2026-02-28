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
    [SerializeField] private string musicMixerParam = "MusicVolume"; // optional exposed param
    [SerializeField] private string sfxMixerParam = "SFXVolume";     // optional exposed param

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private List<ClipEntry> clips = new List<ClipEntry>();

    // Internal lookup for fast access
    private Dictionary<string, ClipEntry> _clipMap;

    private const string VolumePrefKey = "volumenAudio";
    private float _volume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        // Build lookup
        _clipMap = new Dictionary<string, ClipEntry>();
        foreach (var ce in clips)
        {
            if (ce == null || string.IsNullOrEmpty(ce.name) || ce.clip == null) continue;
            if (!_clipMap.ContainsKey(ce.name)) _clipMap.Add(ce.name, ce);
        }

        // Ensure AudioSources exist
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

        // Load saved volume (default 0.5)
        _volume = PlayerPrefs.GetFloat(VolumePrefKey, 0.5f);
        ApplyVolumeToSystem(_volume);
    }

    private ClipEntry GetClip(string name)
    {
        if (string.IsNullOrEmpty(name) || _clipMap == null) return null;
        _clipMap.TryGetValue(name, out var ce);
        return ce;
    }

    // Play short SFX; supports overlapping using PlayOneShot
    public void PlaySFX(string name)
    {
        var ce = GetClip(name);
        if (ce == null || sfxSource == null) return;
        sfxSource.PlayOneShot(ce.clip, ce.volume);
    }

    // Play music (single channel). Calling again replaces current music.
    public void PlayMusic(string name, bool loop = true)
    {
        var ce = GetClip(name);
        if (ce == null || musicSource == null) return;
        musicSource.clip = ce.clip;
        musicSource.loop = loop;
        musicSource.volume = ce.volume;
        musicSource.Play();
    }

    // Stop currently playing music (optionally only if matches name)
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

    // Pause/resume music channel
    public void ToggleMusic(bool pause)
    {
        if (musicSource == null) return;
        if (pause) musicSource.Pause();
        else musicSource.UnPause();
    }

    // Global volume control. Persists to PlayerPrefs and updates AudioListener and mixer if present.
    public void SetVolume(float val)
    {
        _volume = Mathf.Clamp01(val);
        PlayerPrefs.SetFloat(VolumePrefKey, _volume);
        PlayerPrefs.Save();
        ApplyVolumeToSystem(_volume);
    }

    public float GetVolume()
    {
        return _volume;
    }

    private void ApplyVolumeToSystem(float val)
    {
        // Update AudioListener global volume
        AudioListener.volume = val;

        // If an AudioMixer is provided, optionally set exposed params (assumes exposed in mixer)
        if (audioMixer != null)
        {
            // Convert linear [0,1] to mixer dB (-80..0) for a reasonable mapping
            float dB = Mathf.Lerp(-80f, 0f, val);
            if (!string.IsNullOrEmpty(musicMixerParam)) audioMixer.SetFloat(musicMixerParam, dB);
            if (!string.IsNullOrEmpty(sfxMixerParam)) audioMixer.SetFloat(sfxMixerParam, dB);
        }

        // Also adjust source volumes so PlayOneShot respects overall volume
        if (musicSource != null) musicSource.volume = val;
        if (sfxSource != null) sfxSource.volume = val;
    }
}