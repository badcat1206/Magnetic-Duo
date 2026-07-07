using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup sfxGroup;

    public AudioMixerGroup SfxGroup => sfxGroup;

    public const float DefaultVolume = 0.5f;

    private const string SfxKey = "SFXVolume";
    private const string BgmKey = "BGMVolume";
    private const float MinDb = -80f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetSfxVolume(GetSfxVolume());
        SetBgmVolume(GetBgmVolume());
    }

    public float GetSfxVolume() => PlayerPrefs.GetFloat(SfxKey, DefaultVolume);
    public float GetBgmVolume() => PlayerPrefs.GetFloat(BgmKey, DefaultVolume);

    public void SetSfxVolume(float linear01)
    {
        mixer.SetFloat(SfxKey, LinearToDb(linear01));
        PlayerPrefs.SetFloat(SfxKey, linear01);
    }

    public void SetBgmVolume(float linear01)
    {
        mixer.SetFloat(BgmKey, LinearToDb(linear01));
        PlayerPrefs.SetFloat(BgmKey, linear01);
    }

    private static float LinearToDb(float linear01)
    {
        return linear01 <= 0.0001f ? MinDb : Mathf.Log10(linear01) * 20f;
    }
}
