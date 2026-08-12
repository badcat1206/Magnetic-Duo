using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup bgmGroup;

    public AudioMixerGroup SfxGroup => sfxGroup;

    public const float DefaultVolume = 0.5f;

    private const string SfxKey = "SFXVolume";
    private const string BgmKey = "BGMVolume";
    private const float MinDb = -80f;

    private AudioSource bgmSource;
    private Coroutine bgmFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = bgmGroup;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        SetSfxVolume(GetSfxVolume());
        SetBgmVolume(GetBgmVolume());
    }

    public void PlayBgm(AudioClip clip, float fadeInDuration = 0f)
    {
        if (clip == null) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);

        if (bgmSource.clip != clip || !bgmSource.isPlaying)
        {
            bgmSource.clip = clip;
            bgmSource.volume = fadeInDuration > 0f ? 0f : 1f;
            bgmSource.Play();
        }

        if (fadeInDuration > 0f)
            bgmFadeCoroutine = StartCoroutine(FadeBgmVolume(1f, fadeInDuration));
    }

    public void FadeOutBgm(float duration)
    {
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeBgmVolume(0f, duration));
    }

    private IEnumerator FadeBgmVolume(float target, float duration)
    {
        float start = bgmSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        bgmSource.volume = target;
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
