using UnityEngine;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [Header("패널 연결")]
    [SerializeField] private RectTransform panel;

    [Header("슬라이드 연출 설정")]
    [SerializeField] private float slideDuration = 0.4f;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private bool isPaused = false;
    private Tween slideTween;

    private void Awake()
    {
        shownPosition = panel.anchoredPosition;
        hiddenPosition = shownPosition - new Vector2(0f, panel.rect.height + 40f);
        panel.anchoredPosition = hiddenPosition;
        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        slideTween?.Kill();

        if (isPaused)
        {
            Time.timeScale = 0f;
            panel.gameObject.SetActive(true);
            slideTween = panel.DOAnchorPos(shownPosition, slideDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
        else
        {
            slideTween = panel.DOAnchorPos(hiddenPosition, slideDuration)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    panel.gameObject.SetActive(false);
                    Time.timeScale = 1f;
                });
        }
    }
}
