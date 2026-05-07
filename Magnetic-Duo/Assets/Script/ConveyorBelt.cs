using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private SurfaceEffector2D effector;

    [Header("벨트 설정")]
    [SerializeField] private float activeSpeed = 5f;

    [SerializeField] private bool isRunning = false;

    void Awake()
    {
        effector = GetComponent<SurfaceEffector2D>();
    }

    void Start()
    {
        if (isRunning)
        {
            effector.speed = activeSpeed;
        }
        else
        {
            effector.speed = 0f;
        }

    }

    public void ToggleGBelt()
    {
        isRunning = !isRunning;

        if (isRunning)
        {
            effector.speed = activeSpeed;
            Debug.Log($"벨트 작동 시작. 현재 속도 {effector.speed}");
        }
        else
        {
            effector.speed = 0f;
            Debug.Log("벨트 정지");
        }
    }
}
