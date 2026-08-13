using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Cup : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging;
    private Vector3 dragOffset;

    [SerializeField]
    private float capacityLiters = 1.0f;

    [SerializeField]
    private float currentLiters = 0.5f;

    [SerializeField]
    private TMP_Text currentLitersText;

    [SerializeField]
    private Vector3 defaultPosition;

    public float CapacityLiters => capacityLiters;
    public float CurrentLiters => currentLiters;
    public float FreeSpaceLiters => capacityLiters - currentLiters;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "Main Cameraが見つかりません。CameraのTagを確認してください。"
            );
        }

        defaultPosition = transform.position;
    }

    private void Update()
    {
        if (mainCamera == null || Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDragging();
        }

        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            Drag();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            StopDragging();
        }

        currentLitersText.text = currentLiters.ToString("F2") + " L";
    }

    private void StartDragging()
    {
        Vector2 mousePosition = GetMouseWorldPosition();

        Collider2D[] hits =
            Physics2D.OverlapPointAll(mousePosition);

        foreach (Collider2D hit in hits)
        {
            Debug.Log("検出: " + hit.gameObject.name);

            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                isDragging = true;

                dragOffset =
                    transform.position -
                    (Vector3)mousePosition;

                Debug.Log("ドラッグ開始");
                return;
            }
        }
    }

    private void Drag()
    {
        Vector2 mousePosition = GetMouseWorldPosition();

        Vector3 newPosition =
            (Vector3)mousePosition + dragOffset;

        newPosition.z = transform.position.z;

        transform.position = newPosition;
    }

    private void StopDragging()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;

        Cup targetCup = FindTargetCup();

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
            Debug.Log("移動先のコップがありません");
        }

        transform.position = defaultPosition;

        Debug.Log("ドラッグ終了");
    }

    private Cup FindTargetCup()
    {
        Vector2 worldPosition = GetMouseWorldPosition();

        Collider2D[] hits =
            Physics2D.OverlapPointAll(worldPosition);

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
            mainCamera.ScreenToWorldPoint(screenPosition);

        return new Vector2(
            worldPosition.x,
            worldPosition.y
        );
    }

    public void PourInto(Cup targetCup)
    {
        if (targetCup == null || targetCup == this)
        {
            return;
        }

        float transferableAmount = Mathf.Min(
            currentLiters,
            targetCup.FreeSpaceLiters
        );

        if (transferableAmount <= 0f)
        {
            Debug.Log("液体を移せません");
            return;
        }

        currentLiters -= transferableAmount;
        targetCup.currentLiters += transferableAmount;

        Debug.Log(
            transferableAmount + "L移しました"
        );

        Debug.Log(
            gameObject.name +
            "の残量: " +
            currentLiters +
            "L"
        );

        Debug.Log(
            targetCup.gameObject.name +
            "の残量: " +
            targetCup.currentLiters +
            "L"
        );
    }

    private void OnValidate()
    {
        capacityLiters = Mathf.Max(
            0f,
            capacityLiters
        );

        currentLiters = Mathf.Clamp(
            currentLiters,
            0f,
            capacityLiters
        );
    }
}