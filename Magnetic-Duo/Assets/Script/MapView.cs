using UnityEngine;
using UnityEngine.UI;

// M키로 화면 오른쪽 위의 전체 맵을 열고 닫음 (봇과 배경 그림은 표시하지 않음)
// 맵이 열려 있는 동안 봇 조작(이동·점프·Q·E·Tab)만 막고 나머지는 그대로 진행, ESC를 누르면 맵만 닫힘 (PauseMenu에서 처리)
// 맵 범위는 같은 오브젝트의 CameraFollow 맵 경계를 사용
// 맵 카메라 → 렌더 텍스처 → UI 이미지로 띄워서 배경색의 투명도(A)가 적용됨
[RequireComponent(typeof(Camera), typeof(CameraFollow))]
public class MapView : MonoBehaviour
{
    [Header("맵 창 최대 크기 (화면 대비 비율, 맵 비율에 맞춰 이 안에 들어가게 맞춤)")]
    [SerializeField] private float maxWidth = 0.5f;
    [SerializeField] private float maxHeight = 0.5f;
    [SerializeField] private float margin = 0.01f;

    [Header("맵 배경 / 테두리")]
    [Tooltip("배경 그림 대신 맵 뒤를 채울 색 (A값을 낮추면 뒤의 게임 화면이 비침)")]
    [SerializeField] private Color backgroundColor = new Color(0.06f, 0.08f, 0.14f, 0.8f);
    [SerializeField] private Color borderColor = new Color(0.55f, 0.75f, 1f, 1f);
    [SerializeField] private float borderPixels = 4f;

    private Camera mapCamera;
    private RenderTexture mapTexture;
    private GameObject mapCanvas;
    private RawImage mapImage;
    private readonly Image[] borders = new Image[4];
    private Rect mapBounds;

    public static MapView Instance { get; private set; }
    public bool IsOpen { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Camera mainCamera = GetComponent<Camera>();
        mapBounds = GetComponent<CameraFollow>().MapBounds;

        // 오디오 리스너·MainCamera 태그 없이 카메라만 가진 별도 오브젝트
        mapCamera = new GameObject("MapCamera").AddComponent<Camera>();
        mapCamera.CopyFrom(mainCamera);
        mapCamera.cullingMask &= ~LayerMask.GetMask("NBot", "SBot", "Background"); // 봇과 배경 그림은 맵에 표시하지 않음
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.allowHDR = false; // 배경색의 투명도가 텍스처에 그대로 남도록
        mapCamera.orthographicSize = mapBounds.height / 2f;
        mapCamera.transform.position = new Vector3(mapBounds.center.x, mapBounds.center.y, mainCamera.transform.position.z);
        mapCamera.enabled = false;

        // HUD·일시정지·페이드 UI보다 아래에 그려지는 캔버스
        mapCanvas = new GameObject("MapCanvas", typeof(Canvas));
        Canvas canvas = mapCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;

        mapImage = new GameObject("MapImage", typeof(RawImage)).GetComponent<RawImage>();
        mapImage.raycastTarget = false;
        mapImage.rectTransform.SetParent(mapCanvas.transform, false);

        // 테두리: 맵 이미지 바깥쪽 네 변 (위, 아래, 왼쪽, 오른쪽)
        for (int i = 0; i < borders.Length; i++)
        {
            borders[i] = new GameObject("MapBorder", typeof(Image)).GetComponent<Image>();
            borders[i].raycastTarget = false;
            borders[i].rectTransform.SetParent(mapImage.rectTransform, false);
        }

        mapCanvas.SetActive(false);
    }

    void Update()
    {
        // 맵이 열린 동안 봇이 죽으면 맵을 닫고 사망 연출 진행
        if (IsOpen && IsAnyBotDead()) Close();

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (IsOpen) Close();
            else if (Time.timeScale > 0f && !IsAnyBotDead()) Open(); // 일시정지 창이 떠 있거나 사망 연출 중이면 열지 않음
        }

        if (IsOpen) FitToScreen();
    }

    private bool IsAnyBotDead()
    {
        CharacterManager characters = CharacterManager.Instance;
        return characters != null && (characters.NBotInput.IsDead || characters.SBotInput.IsDead);
    }

    private void Open()
    {
        IsOpen = true;
        SetBotsFrozen(true);
        FitToScreen();
        mapCamera.enabled = true;
        mapCanvas.SetActive(true);
    }

    public void Close()
    {
        IsOpen = false;
        SetBotsFrozen(false);
        mapCamera.enabled = false;
        mapCanvas.SetActive(false);
    }

    // 두 봇 모두 그 자리에 정지 (공중에서도 멈춤, 자성 등 나머지는 그대로 작동)
    private void SetBotsFrozen(bool frozen)
    {
        CharacterManager characters = CharacterManager.Instance;
        if (characters == null) return;
        characters.NBotInput.GetComponent<PlayerMovement>().SetFrozen(frozen);
        characters.SBotInput.GetComponent<PlayerMovement>().SetFrozen(frozen);
    }

    // 맵 비율에 맞춰 오른쪽 위 창 크기를 계산 (창 크기가 바뀌거나 인스펙터 값을 바꿔도 바로 반영)
    private void FitToScreen()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float mapAspect = mapBounds.width / mapBounds.height;

        float w = maxWidth;
        float h = w * screenAspect / mapAspect;
        if (h > maxHeight)
        {
            h = maxHeight;
            w = h * mapAspect / screenAspect;
        }

        RectTransform rect = mapImage.rectTransform;
        rect.anchorMin = new Vector2(1f - margin - w, 1f - margin - h);
        rect.anchorMax = new Vector2(1f - margin, 1f - margin);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        // 맵 창의 실제 픽셀 크기로 렌더 텍스처 생성 (크기가 바뀌면 다시 만듦)
        int pw = Mathf.Max(1, Mathf.RoundToInt(w * Screen.width));
        int ph = Mathf.Max(1, Mathf.RoundToInt(h * Screen.height));
        if (mapTexture == null || mapTexture.width != pw || mapTexture.height != ph)
        {
            ReleaseTexture();
            mapTexture = new RenderTexture(pw, ph, 24);
            mapCamera.targetTexture = mapTexture;
            mapCamera.ResetAspect();
            mapImage.texture = mapTexture;
        }

        mapCamera.backgroundColor = backgroundColor;

        float b = borderPixels;
        SetBorder(borders[0], new Vector2(0, 1), new Vector2(1, 1), new Vector2(-b, 0), new Vector2(b, b));   // 위
        SetBorder(borders[1], new Vector2(0, 0), new Vector2(1, 0), new Vector2(-b, -b), new Vector2(b, 0));  // 아래
        SetBorder(borders[2], new Vector2(0, 0), new Vector2(0, 1), new Vector2(-b, 0), Vector2.zero);        // 왼쪽
        SetBorder(borders[3], new Vector2(1, 0), new Vector2(1, 1), Vector2.zero, new Vector2(b, 0));         // 오른쪽
    }

    private void SetBorder(Image border, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        border.color = borderColor;
        RectTransform rect = border.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private void ReleaseTexture()
    {
        if (mapTexture == null) return;
        if (mapCamera != null) mapCamera.targetTexture = null;
        mapTexture.Release();
        Destroy(mapTexture);
        mapTexture = null;
    }

    void OnDestroy()
    {
        ReleaseTexture();
    }
}
