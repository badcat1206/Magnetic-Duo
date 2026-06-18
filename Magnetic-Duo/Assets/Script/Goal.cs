using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour
{
    [Header("밟을 오브젝트 태그")]
    [SerializeField] private string targetTag;

    [Header("오디오")]
    [SerializeField] private AudioClip enterClip;
    private AudioSource audioSource;

    public bool IsReached { get; private set; }

    private Coroutine reachCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            if (audioSource != null && enterClip != null)
                audioSource.PlayOneShot(enterClip);

            PlayerAnimation playerAnim = collision.GetComponent<PlayerAnimation>();
            if(playerAnim != null)
            {
                playerAnim.SetOnGoal(true);
            }

            if(reachCoroutine != null) StopCoroutine(reachCoroutine);
            reachCoroutine = StartCoroutine(WaitAndSetReached());
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            if (reachCoroutine != null)
            {
                StopCoroutine(reachCoroutine);
                reachCoroutine = null;
            }

            IsReached = false;

            PlayerAnimation playerAnim = collision.GetComponent<PlayerAnimation>();
            if(playerAnim != null)
            {
                playerAnim.SetOnGoal(false);
            }
        }
    }

    private IEnumerator WaitAndSetReached()
    {
        yield return new WaitForSeconds(0.5f); // 시간은 입맛에 맞게 조절하세요 (예: 0.3f, 1.0f)
        IsReached = true;
    }
}
