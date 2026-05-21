using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    // 벨트의 모드 선택. OnOff, 방향전환
    public enum BeltMode
    {
        ToggleOnOff,
        ReverseDirection
    }

    [Header("벨트 설정")]
    [SerializeField] private BeltMode beltMode = BeltMode.ToggleOnOff;
    [SerializeField] private float activeSpeed = 5f;

    [SerializeField] private bool isRunning = false;

    [Header("필터링 설정")]
    [SerializeField] private LayerMask boxLayer;    // 상자 레이어 선택

    private SurfaceEffector2D effector;
    private Collider2D beltCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        effector = GetComponent<SurfaceEffector2D>();
        beltCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateBeltSpeed();
    }

    public void ToggleGBelt()
    {
        // 선택한 모드에 따라 다르게 작동
        if (beltMode == BeltMode.ToggleOnOff)
        {
            // 온오프 모드 : 실행 상태(isRunning)를 뒤집음
            isRunning = !isRunning;
            Debug.Log($"벨트 작동 시작. 현재 속도 {effector.speed}");
        }
        else if (beltMode == BeltMode.ReverseDirection)
        {
            // 방향 반전 모드 : 속도의 부호(+, -)를 뒤집음
            activeSpeed = -activeSpeed;
            Debug.Log($"벨트 방향 반전. 현재 속도 {effector.speed}");
        }

        // 변경된 상태를 실제 컴포넌트에 적용
        UpdateBeltSpeed();

        if (isRunning)
        {
            GiveNudgeToBoxesOnMe();
        }
    }

    private void UpdateBeltSpeed()
    {
        if (isRunning)
        {
            effector.speed = activeSpeed;

            if (spriteRenderer != null)
            {
                if (activeSpeed > 0)
                {
                    spriteRenderer.color = new Color32(30, 160, 30, 255);   // 오른쪽 연두색
                }
                else if (activeSpeed < 0)
                {
                    spriteRenderer.color = new Color32(20, 140, 160, 255);   // 왼쪽 청록색(원하는 색으로 변경 가능)
                }
            }
        }
        else
        {
            effector.speed = 0f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.gray; // 꺼짐 회색
            }
        }
    }

    // 방향이 전환될 시 위에 있는 상자를 살짝 밀어주기 위한 함수
    private void GiveNudgeToBoxesOnMe()
    {
        List<Collider2D> touchingObjects = new List<Collider2D>();

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(boxLayer);
        filter.useLayerMask = true;

        Physics2D.OverlapCollider(beltCollider, filter, touchingObjects);

        foreach (Collider2D obj in touchingObjects)
        {
            // 박스의 중심이 벨트보다 밖에 있을 경우
            if (obj.bounds.center.x >= beltCollider.bounds.min.x &&
                obj.bounds.center.x <= beltCollider.bounds.max.x)
            {
                Rigidbody2D boxRb = obj.GetComponent<Rigidbody2D>();
                if (boxRb != null)
                {
                    boxRb.linearVelocity = Vector2.zero;
                    float forceDirection = Mathf.Sign(-activeSpeed);
                    boxRb.AddForce(new Vector2(forceDirection * 3f, 0), ForceMode2D.Impulse);
                }
            }
        }

    }
}
