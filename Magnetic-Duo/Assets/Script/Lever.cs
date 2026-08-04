using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    // 켜졌을 때와 꺼졌을 때의 이미지 변수
    [Header("레버 이미지 설정")]
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Sprite spriteOff;

    [Header("자력 상호작용 설정")]
    public Polarity polarity; // N, S 중 선택

    [Header("연결할 장치")]
    [SerializeField] private ConveyorBelt[] connectedBelts;
    [SerializeField] private MagneticField[] connectedMagneticFields;
    [SerializeField] private PolarityWall[] connectedWalls;
    [SerializeField] private MovingPlatform[] connectedPlatforms;

    [Header("상태를 동기화할 다른 레버들")]
    [SerializeField] private Lever[] linkedLevers;

    private SpriteRenderer spriteRenderer;
    private bool isOn = false;
    public bool IsOn => isOn;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Interact() => ActivateLever();

    public void ActivateLever()
    {
        Debug.Log("레버 당겨짐");
        isOn = !isOn;

        UpdateVisual();

        if (connectedBelts != null)
        {
            foreach (ConveyorBelt belt in connectedBelts)
           {
                if (belt != null) belt.ToggleGBelt(); // 컨베이어 벨트의 isRunning 상태를 뒤집음
            }
        }

        if (connectedMagneticFields != null)
        {
            foreach (MagneticField field in connectedMagneticFields)
            {
                if (field != null) field.ToggleField();
            }
        }

        if(connectedWalls != null)
        {
            foreach(PolarityWall wall in connectedWalls)
            {
                if(wall != null) wall.ToggleWall();
            }
        }

        if(linkedLevers != null)
        {
            foreach(Lever lever in linkedLevers)
            {
                if(lever != null) lever.SyncState(isOn);
            }
        }

        if (connectedPlatforms != null)
        {
            foreach (MovingPlatform platform in connectedPlatforms)
            {
                if (platform != null) platform.SetLeverActive(isOn);
            }
        }
    }

    public void SyncState(bool targetState)
    {
        if(isOn == targetState) return;
        isOn = targetState;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOn ? spriteOn : spriteOff;
        }
    }
}
