using UnityEngine;

public class StageBgm : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;

    private void Start()
    {
        if (AudioManager.Instance == null) return;

        float fadeInDuration = ScreenFader.Instance != null ? ScreenFader.Instance.FadeDuration : 0f;
        AudioManager.Instance.PlayBgm(bgmClip, fadeInDuration);
    }
}
