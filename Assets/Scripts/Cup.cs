using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cup : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging;
    private Vector3 dragOffset;
    private Vector3 defaultPosition;

    public bool CanDrag { get; set; } = true;

    [Header("音声")]

    [SerializeField]
    private SoundManager soundManager;

    [Header("コップの容量")]

    [SerializeField]
    private float capacityLiters = 1.0f;

    [SerializeField]
    private float currentLiters = 0.5f;

    [Header("残量表示")]

    [SerializeField]
    private TMP_Text currentLitersText;

    [Header("液体アニメーション")]

    [SerializeField]
    private AnimationStateController animationStateController;

    public float CapacityLiters => capacityLiters;

    public float CurrentLiters => currentLiters;

    public float FreeSpaceLiters =>
        capacityLiters - currentLiters;

    private void Awake()
    {
        mainCamera = Camera.main;
        defaultPosition = transform.position;

        if (mainCamera == null)
        {
            Debug.LogError(
                "Main Cameraが見つかりません。" +
                "CameraのTagを確認してください。",
                gameObject
            );
        }

        if (animationStateController == null)
        {
            animationStateController =
                GetComponentInChildren
                <AnimationStateController>(true);
        }

        if (animationStateController == null)
        {
            Debug.LogError(
                gameObject.name +
                "にAnimationStateControllerがありません",
                gameObject
            );
        }

        if (soundManager == null)
        {
            soundManager =
                FindFirstObjectByType<SoundManager>();
        }
    }

    private void Start()
    {
        UpdateLitersText();
        UpdateLiquidAnimation();
    }

    private void Update()
    {
        if (!CanDrag)
        {
            return;
        }

        if (mainCamera == null ||
            Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton
            .wasPressedThisFrame)
        {
            StartDragging();
        }

        if (isDragging &&
            Mouse.current.leftButton.isPressed)
        {
            Drag();
        }

        if (Mouse.current.leftButton
            .wasReleasedThisFrame)
        {
            StopDragging();
        }
    }

    private void StartDragging()
    {
        Vector2 mousePosition =
            GetMouseWorldPosition();

        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                mousePosition
            );

        foreach (Collider2D hit in hits)
        {
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                isDragging = true;

                dragOffset =
                    transform.position -
                    (Vector3)mousePosition;

                Debug.Log(
                    gameObject.name +
                    "のドラッグ開始"
                );

                return;
            }
        }
    }

    private void Drag()
    {
        Vector2 mousePosition =
            GetMouseWorldPosition();

        Vector3 newPosition =
            (Vector3)mousePosition +
            dragOffset;

        newPosition.z =
            transform.position.z;

        transform.position =
            newPosition;
    }

    private void StopDragging()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;

        Cup targetCup =
            FindTargetCup();

        if (targetCup != null)
        {
            PourInto(targetCup);

            Debug.Log(
                targetCup.gameObject.name +
                "にドロップしました"
            );
        }
        else
        {
            Debug.Log(
                "移動先のコップがありません"
            );
        }

        transform.position =
            defaultPosition;

        Debug.Log(
            gameObject.name +
            "のドラッグ終了"
        );
    }

    private Cup FindTargetCup()
    {
        Vector2 worldPosition =
            GetMouseWorldPosition();

        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                worldPosition
            );

        foreach (Collider2D hit in hits)
        {
            Cup targetCup =
                hit.GetComponentInParent<Cup>();

            if (targetCup == null)
            {
                continue;
            }

            if (targetCup == this)
            {
                continue;
            }

            return targetCup;
        }

        return null;
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 screenPosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );

        return new Vector2(
            worldPosition.x,
            worldPosition.y
        );
    }

    public void PourInto(Cup targetCup)
    {
        if (targetCup == null ||
            targetCup == this)
        {
            return;
        }

        float transferableAmount =
            Mathf.Min(
                currentLiters,
                targetCup.FreeSpaceLiters
            );

        if (transferableAmount <= 0f)
        {
            Debug.Log(
                "液体を移せません"
            );

            return;
        }

        currentLiters -=
            transferableAmount;

        targetCup.currentLiters +=
            transferableAmount;

        UpdateLitersText();
        targetCup.UpdateLitersText();

        UpdateLiquidAnimation();
        targetCup.UpdateLiquidAnimation();

        if (soundManager != null)
        {
            soundManager.PlayPourSE();
        }

        Debug.Log(
            transferableAmount +
            "L移しました"
        );

        Debug.Log(
            gameObject.name +
            "の残量: " +
            currentLiters.ToString("F2") +
            "L"
        );

        Debug.Log(
            targetCup.gameObject.name +
            "の残量: " +
            targetCup.currentLiters
                .ToString("F2") +
            "L"
        );
    }

    private void UpdateLitersText()
    {
        if (currentLitersText == null)
        {
            return;
        }

        currentLitersText.text =
            currentLiters.ToString("F2") +
            " L";
    }

    private void UpdateLiquidAnimation()
    {
        if (animationStateController == null)
        {
            return;
        }

        int liters =
            Mathf.Clamp(
                Mathf.RoundToInt(
                    currentLiters
                ),
                0,
                10
            );

        animationStateController
            .ChangeState(liters);
    }

    private void OnValidate()
    {
        capacityLiters =
            Mathf.Clamp(
                capacityLiters,
                0f,
                10f
            );

        currentLiters =
            Mathf.Clamp(
                currentLiters,
                0f,
                capacityLiters
            );
    }
}