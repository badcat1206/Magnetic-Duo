using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("NBot") || collision.CompareTag("SBot"))
        {
            PlayerInput deadPlayer = collision.GetComponent<PlayerInput>();
            StartCoroutine(DieSequence(deadPlayer));
        }
    }

    private IEnumerator DieSequence(PlayerInput deadPlayer)
    {
        Debug.Log("죽음");

        if(deadPlayer != null)
        {
            deadPlayer.SetLocked(true);

            Rigidbody2D rb = deadPlayer.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            PlayerAnimation playerAnim = deadPlayer.GetComponent<PlayerAnimation>();
            if(playerAnim != null)
            {
                playerAnim.TriggerDeath();
            }
        }

        yield return new WaitForSeconds(2f);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}
