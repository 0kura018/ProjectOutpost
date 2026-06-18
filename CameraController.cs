using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Pan Settings")]
    [Tooltip("Скорость перемещения камеры при удержании средней кнопки мыши")]
    [SerializeField] private float _panSpeed = 0.5f;

    [Header("Zoom Settings")]
    [Tooltip("Скорость зума колесиком мыши")]
    [SerializeField] private float _zoomSpeed = 2f;

    [Tooltip("Минимальный размер камеры (максимальное приближение)")]
    [SerializeField] private float _minZoom = 2f;

    [Tooltip("Максимальный размер камеры (максимальное отдаление)")]
    [SerializeField] private float _maxZoom = 15f;

    [Header("Movement Limits")]
    [Tooltip("Включить ограничения движения камеры")]
    [SerializeField] private bool _limitMovement = true;

    [Tooltip("Минимальная позиция камеры по X")]
    [SerializeField] private float _minX = -20f;

    [Tooltip("Максимальная позиция камеры по X")]
    [SerializeField] private float _maxX = 20f;

    [Tooltip("Минимальная позиция камеры по Y")]
    [SerializeField] private float _minY = -20f;

    [Tooltip("Максимальная позиция камеры по Y")]
    [SerializeField] private float _maxY = 20f;

    private Camera _camera;
    private Vector3 _dragOrigin;
    private bool _isDragging = false;

    private Mouse _mouse;
    private Vector2 _mousePosition;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("[CameraController] Camera component not found!");
        }

        _mouse = Mouse.current;
        if (_mouse == null)
        {
            Debug.LogWarning("[CameraController] Mouse device not found!");
        }
    }

    private void Update()
    {
        if (_mouse == null) return;

        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
        _mousePosition = _mouse.position.ReadValue();

        if (_mouse.middleButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            _dragOrigin = _camera.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, _camera.nearClipPlane));
            _isDragging = true;
        }

        if (_mouse.middleButton.isPressed && _isDragging)
        {
            Vector3 currentMousePosition = _camera.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, _camera.nearClipPlane));
            Vector3 difference = _dragOrigin - currentMousePosition;
            Vector3 newPosition = transform.position + difference * _panSpeed;

            if (_limitMovement)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, _minX, _maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, _minY, _maxY);
            }

            transform.position = newPosition;
        }

        if (_mouse.middleButton.wasReleasedThisFrame)
        {
            _isDragging = false;
        }
    }

    private void HandleZoom()
    {
        Vector2 scrollDelta = _mouse.scroll.ReadValue();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (scrollDelta.magnitude > 0.01f)
        {
            Debug.Log($"[Camera] Raw scroll: {scrollDelta}, y: {scrollDelta.y}");
        }

        if (Mathf.Abs(scrollDelta.y) > 0.1f)
        {
            float scrollInput = scrollDelta.y / 120f;

            if (_camera.orthographic)
            {
                float oldSize = _camera.orthographicSize;
                float newSize = _camera.orthographicSize - scrollInput * _zoomSpeed;
                _camera.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);

                Debug.Log($"[Camera] Zoom: scroll={scrollInput:F3}, oldSize={oldSize:F2}, newSize={_camera.orthographicSize:F2}");
            }
            else
            {
                Vector3 newPosition = transform.position;
                newPosition.z += scrollInput * _zoomSpeed;
                newPosition.z = Mathf.Clamp(newPosition.z, -_maxZoom, -_minZoom);
                transform.position = newPosition;
            }
        }
    }

    public void SetMovementLimits(float minX, float maxX, float minY, float maxY)
    {
        _minX = minX;
        _maxX = maxX;
        _minY = minY;
        _maxY = maxY;
        _limitMovement = true;
    }

    public void SetZoomLimits(float minZoom, float maxZoom)
    {
        _minZoom = minZoom;
        _maxZoom = maxZoom;
    }

    public void MoveToPosition(Vector3 position, float duration = 0f)
    {
        if (duration <= 0f)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
        else
        {
            StartCoroutine(SmoothMoveCoroutine(position, duration));
        }
    }

    private System.Collections.IEnumerator SmoothMoveCoroutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        targetPosition.z = startPosition.z;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }
}
