using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("연결할 장치")]
    [SerializeField] private ConveyorBelt[] connectedBelts; // 레버 작동시 작동할 컨베이어 벨트

    // 임시로 색깔을 바꾸기 위해 추가
    private SpriteRenderer spriteRenderer;
    private bool isOn = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ActivateLever()
    {
        Debug.Log("레버 당겨짐");
        isOn = !isOn;

        if(spriteRenderer != null)
        {
            if(isOn)
            {
                spriteRenderer.color = Color.green;
            } 
            else
            {
                spriteRenderer.color = Color.white;    
            }
        }
        // 컨베이어 벨트 작동
        if(connectedBelts != null)
        {
            foreach(ConveyorBelt belt in connectedBelts)
            {
                if(belt != null)
                {
                    belt.ToggleGBelt();
                }
            }
        }
    }
}
