using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class Trap : MonoBehaviour
{
    [Header("상자 리스폰 설정")]
    [Tooltip("체크하면 이 트랩에 닿은 상자가 리스폰됩니다.")]
    [SerializeField] private bool canDestroyBox = true;
    [SerializeField] private Vector2 boxSpawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NBot") || collision.CompareTag("SBot"))
        {
            PlayerInput deadPlayer = collision.GetComponent<PlayerInput>();
            StartCoroutine(DieSequence(deadPlayer));
        }
        else if (canDestroyBox &&
            (collision.CompareTag("NBox") || collision.CompareTag("SBox")))
        {
            RespawnBox(collision.gameObject);
        }
    }

    private void RespawnBox(GameObject box)
    {
        if (boxSpawnPoint != null)
        {
            box.transform.position = boxSpawnPoint;

            Rigidbody2D boxRb = box.GetComponent<Rigidbody2D>();
            if (boxRb != null)
            {
                boxRb.linearVelocity = Vector2.zero;
                boxRb.angularVelocity = 0f;
            }
        }
        else
        {
            Debug.LogWarning("상자 리스폰 위치가 할당되지 않음.");
        }
    }

    private IEnumerator DieSequence(PlayerInput deadPlayer)
    {
        Debug.Log("죽음");

        if (deadPlayer != null)
        {
            deadPlayer.SetLocked(true);

            Rigidbody2D rb = deadPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            PlayerAnimation playerAnim = deadPlayer.GetComponent<PlayerAnimation>();
            if (playerAnim != null)
            {
                playerAnim.TriggerDeath();
            }
        }

        yield return new WaitForSeconds(2f);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}
