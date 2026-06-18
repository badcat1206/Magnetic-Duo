using System;
using System.Collections;
using UnityEngine;

// 웨이포인트 배열을 순환하며 이동하는 발판
// 발판 위에 플레이어(NBot/SBot)가 올라오면 자식으로 설정해 함께 이동시킴
public class ShuttlePlatform : MonoBehaviour
{
    [Header("발판 설정")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float waitTime = 1f;

    [Header("위치 지정")]
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;

    [Header("비주얼 오브젝트 (자성 OFF/ON 이미지)")]
    [SerializeField] private SpriteRenderer visualOff;
    [SerializeField] private SpriteRenderer visualOn;

    private Vector2 targetPoint;
    private bool isWaiting = false;
    private bool wasMagneticActive = false;

    private MagneticAbility riderAbility;

    void Start()
    {
        transform.position = startPoint;
        targetPoint = endPoint;
        UpdateVisual(false);
    }

    void Update()
    {
        bool isActive = riderAbility != null && riderAbility.IsActive;

        if (isActive != wasMagneticActive)
        {
            wasMagneticActive = isActive;
            UpdateVisual(isActive);
        }

        if(isWaiting) return;

        if(!isActive) return;
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

    private void UpdateVisual(bool isActive)
    {
        if (visualOff != null) visualOff.enabled = !isActive;
        if (visualOn != null) visualOn.enabled = isActive;
    }

    // 플레이어가 발판에 올라오면 자식으로 설정 → 발판과 함께 이동
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("NBot") || collision.gameObject.CompareTag("SBot") ||
        collision.gameObject.CompareTag("NBox") || collision.gameObject.CompareTag("SBox"))
        {
            collision.gameObject.transform.SetParent(transform);
        }

        MagneticAbility magneticAbility = collision.gameObject.GetComponent<MagneticAbility>();
        if(magneticAbility != null)
        {
            riderAbility = magneticAbility;
        }
    }

    // 플레이어가 발판에서 내려가면 부모 해제
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("NBot") || collision.gameObject.CompareTag("SBot") ||
        collision.gameObject.CompareTag("NBox") || collision.gameObject.CompareTag("SBox"))
        {
            collision.gameObject.transform.SetParent(null);
        }

        if (riderAbility != null && collision.gameObject == riderAbility.gameObject)
        {
            riderAbility = null; 
        }
    }
}
