using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private float interactRange = 1.5f;

    public void TrtInteract()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);
        foreach(Collider2D col in colliders)
        {
            Lever lever = col.GetComponent<Lever>();
            if(lever != null)
            {
                lever.ActivateLever();
                break;
            }
        }
    }
    // 상호작용 범위 그리기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);     
    }
}
