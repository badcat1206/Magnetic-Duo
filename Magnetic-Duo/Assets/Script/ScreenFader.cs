using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;


public class ScreenFader : MonoBehaviour
{
   public static ScreenFader Instance {get; private set;}

   [Header("UI 컴포넌트")]
   [SerializeField] private Image fadeImage;

   [Header("암전 설정")]
   [SerializeField] private float fadeDuration = 1f;    // 암전에 걸리는 시간

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if(fadeImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeOut()
    {
        if(fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        yield return fadeImage.DOFade(1f, fadeDuration).WaitForCompletion();
    }

    private IEnumerator FadeIn()
    {
        if(fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        yield return fadeImage.DOFade(0f, fadeDuration).WaitForCompletion();

        fadeImage.gameObject.SetActive(false);
    }
}
