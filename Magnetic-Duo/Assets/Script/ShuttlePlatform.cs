using System;
using System.Collections;
using UnityEngine;

// 웨이포인트 배열을 순환하며 이동하는 발판
// 발판 위에 플레이어(NBot/SBot)가 올라오면 자식으로 설정해 함께 이동시킴
public class ShuttlePlatform : MonoBehaviour
{
    [Header("발판 설정")]
    [SerializeField] private float speed = 3f;           // 이동 속도
    [SerializeField] private float waitTime = 1f;
    [Header("위치 지정")]
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;
   
    private Vector2 targetPoint;
    private bool isWaiting = false;

    void Start()
    {
        targetPoint = startPoint;
    }

    void Update()
    {
        if(isWaiting) return;
        
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);

        if(Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            StartCoroutine(WaitAndTurn());
        }
    }

    private IEnumerator WaitAndTurn()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitTime);

       if(targetPoint == endPoint)
        {
            targetPoint = startPoint;
        }
        else
        {
            targetPoint = endPoint;
        }

        isWaiting = false;
    }

    // 플레이어가 발판에 올라오면 자식으로 설정 → 발판과 함께 이동
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("NBot") || collision.gameObject.CompareTag("SBot"))
        {
            collision.gameObject.transform.SetParent(transform);
        }
    }

    // 플레이어가 발판에서 내려가면 부모 해제
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("NBot") || collision.gameObject.CompareTag("SBot"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}
