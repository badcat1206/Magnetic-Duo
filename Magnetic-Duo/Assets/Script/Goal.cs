using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("밟을 오브젝트 태그")]
    [SerializeField] private string targetTag;

    public bool IsReached { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            IsReached = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            IsReached = false;
        }
    }
}
