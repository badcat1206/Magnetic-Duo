using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private SurfaceEffector2D effector;

    [Header("벨트 설정")]
    [SerializeField] private float activeSpeed = 5f;

    private bool isRunning = false;

    void Start()
    {
        effector = GetComponent<SurfaceEffector2D>();
        effector.speed = 0f;
    }

    public void ToggleGBelt()
    {
        isRunning = !isRunning;

        if(isRunning)
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
