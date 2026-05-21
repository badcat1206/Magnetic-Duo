using Unity.Mathematics;
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

            PlayerAnimation playerAnim = collision.GetComponent<PlayerAnimation>();
            if(playerAnim != null)
            {
                playerAnim.SetOnGoal(true);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            IsReached = false;

            PlayerAnimation playerAnim = collision.GetComponent<PlayerAnimation>();
            if(playerAnim != null)
            {
                playerAnim.SetOnGoal(false);
            }
        }
        
    }
}
