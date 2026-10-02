using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingPlatform : MonoBehaviour
{
    [System.Serializable]
    private class LeverTarget
    {
        public Lever lever;
        public Vector3 offset;
    }

    [System.Serializable]
    private class ButtonTarget
    {
        public PressureButton button;
        public Vector3 pressOffset;
        public Vector3 releaseOffset;
    }

    [Header("연결된 버튼 (하나라도 눌리면 이동)")]
    [SerializeField] private PressureButton[] buttons;

    [Header("이동 설정")]
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float moveSpeed = 3f;

    [Header("추가 하강 설정 (primaryActive 상태에서 추가 이동)")]
    [SerializeField] private PressureButton[] extraButtons;
    [SerializeField] private Vector3 extraOffset;

    [Header("레버별 이동 설정 (시작 위치 기준, 마지막에 켠 레버 우선)")]
    [SerializeField] private LeverTarget[] leverTargets;

    [Header("버튼별 이동 설정 (밟으면 Press, 떼면 Release 위치로. 시작 위치 기준)")]
    [SerializeField] private ButtonTarget[] buttonTargets;

    [Header("화면 흔들림 설정")]
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeMagnitude = 0.18f;

    [Header("체인 설정 (타일맵 방식)")]
    [SerializeField] private Tilemap hideChainTilemap;

    [Header("체인 설정 (스프라이트 방식 - 타일맵 대신 사용)")]
    [SerializeField] private SpriteRenderer chainSpriteRenderer;

    [Header("오디오")]
    [SerializeField] private AudioClip chainMoveClip;
    private AudioSource audioSource;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 extraTargetPosition;
    private Transform chainMaskTransform;
    private readonly List<Lever> activeLevers = new(); // 켠 순서대로
    private readonly Dictionary<PressureButton, bool> buttonWasPressed = new();

    // 레버별/버튼별 설정으로 마지막에 정해진 목적지
    private bool hasCommand;
    private Vector3 commandedDestination;
    private Vector3 previousDestination;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && AudioManager.Instance != null)
            audioSource.outputAudioMixerGroup = AudioManager.Instance.SfxGroup;

        startPosition = transform.position;
        targetPosition = startPosition + targetOffset;
        extraTargetPosition = targetPosition + extraOffset;
        previousDestination = startPosition;

        if (hideChainTilemap != null)
        {
            hideChainTilemap.gameObject.SetActive(true);
            InitChain();
        }
    }

    public void SetLeverActive(Lever lever, bool active)
    {
        activeLevers.Remove(lever);
        if (active) activeLevers.Add(lever);
        hasCommand = TryGetLeverDestination(out commandedDestination);
    }

    // 버튼별 설정: 밟거나 떼는 순간에 목적지를 정함
    private void CheckButtonTargets()
    {
        if (buttonTargets == null) return;

        foreach (var target in buttonTargets)
        {
            if (target.button == null) continue;

            bool pressed = target.button.IsPressed;
            buttonWasPressed.TryGetValue(target.button, out bool wasPressed);
            if (pressed == wasPressed) continue;

            buttonWasPressed[target.button] = pressed;
            commandedDestination = startPosition + (pressed ? target.pressOffset : target.releaseOffset);
            hasCommand = true;
        }
    }

    // 켜진 레버 중 마지막에 켠 레버의 목표 위치 (레버별 설정이 없으면 false)
    private bool TryGetLeverDestination(out Vector3 destination)
    {
        destination = startPosition;
        if (leverTargets == null) return false;

        for (int i = activeLevers.Count - 1; i >= 0; i--)
        {
            foreach (var target in leverTargets)
            {
                if (target.lever == activeLevers[i])
                {
                    destination = startPosition + target.offset;
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsPrimaryActive()
    {
        if (activeLevers.Count > 0) return true;
        if (buttons != null)
            foreach (var b in buttons)
                if (b != null && b.IsPressed) return true;
        return false;
    }

    private bool IsExtraActive()
    {
        if (extraButtons == null) return false;
        foreach (var b in extraButtons)
            if (b != null && b.IsPressed) return true;
        return false;
    }

    private void Update()
    {
        bool primaryActive = IsPrimaryActive();
        bool extraActive = IsExtraActive();

        CheckButtonTargets();

        Vector3 destination;
        if (hasCommand)
            destination = commandedDestination;
        else if (primaryActive && extraActive)
            destination = extraTargetPosition;
        else if (primaryActive)
            destination = targetPosition;
        else
            destination = startPosition;

        if (destination != previousDestination)
        {
            CameraFollow.Instance?.Shake(shakeDuration, shakeMagnitude);
            if (audioSource != null && chainMoveClip != null)
                audioSource.PlayOneShot(chainMoveClip);
            previousDestination = destination;
        }

        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        if (transform.position == destination && audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void InitChain()
    {
        hideChainTilemap.CompressBounds();

        float cellW = hideChainTilemap.layoutGrid.cellSize.x;
        float cellH = hideChainTilemap.layoutGrid.cellSize.y;

        int maxTileY = int.MinValue;
        float sumX = 0f;
        int count = 0;

        foreach (Vector3Int pos in hideChainTilemap.cellBounds.allPositionsWithin)
        {
            if (!hideChainTilemap.HasTile(pos)) continue;
            if (pos.y > maxTileY) maxTileY = pos.y;
            sumX += hideChainTilemap.GetCellCenterWorld(pos).x;
            count++;
        }

        if (count == 0) return;

        float centerX = sumX / count;
        Vector3Int topCell = new Vector3Int(hideChainTilemap.cellBounds.xMin, maxTileY, 0);
        float chainTopY = hideChainTilemap.GetCellCenterWorld(topCell).y + cellH * 0.5f;

        hideChainTilemap.GetComponent<TilemapRenderer>().maskInteraction =
            SpriteMaskInteraction.VisibleInsideMask;

        var maskObj = new GameObject("ChainMask");
        maskObj.transform.SetParent(hideChainTilemap.transform.parent);
        maskObj.transform.position = new Vector3(centerX, chainTopY, 0f);

        float maskWidth = hideChainTilemap.cellBounds.size.x * cellW + cellW;
        maskObj.transform.localScale = new Vector3(maskWidth, 0f, 1f);

        var mask = maskObj.AddComponent<SpriteMask>();

        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        mask.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 1f), 1f);

        chainMaskTransform = maskObj.transform;
    }

    private void LateUpdate()
    {
        float movedDistance = Mathf.Max(0f, startPosition.y - transform.position.y);

        if (chainMaskTransform != null)
        {
            Vector3 scale = chainMaskTransform.localScale;
            scale.y = movedDistance;
            chainMaskTransform.localScale = scale;
        }

        if (chainSpriteRenderer != null)
        {
            Vector2 size = chainSpriteRenderer.size;
            size.y = movedDistance;
            chainSpriteRenderer.size = size;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + targetOffset, transform.localScale);
        Gizmos.DrawLine(transform.position, transform.position + targetOffset);

        if (extraOffset != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + targetOffset + extraOffset, transform.localScale);
            Gizmos.DrawLine(transform.position + targetOffset, transform.position + targetOffset + extraOffset);
        }
    }
}
