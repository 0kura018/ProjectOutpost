using UnityEngine;

namespace BattleSystem.CardSystem
{

    [RequireComponent(typeof(LineRenderer))]
    public class CardTargetingLine : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int segmentCount = 20;
        [SerializeField] private float curveHeight = 100f;
        [SerializeField] private AnimationCurve curveFalloff = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Line Settings")]
        [SerializeField] private float lineWidth = 5f;
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private Material lineMaterial;

        private LineRenderer _lineRenderer;
        private Camera _camera;
        private Canvas _canvas;
        private float _zDistance;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = segmentCount;
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth * 0.5f;
            _lineRenderer.startColor = lineColor;
            _lineRenderer.endColor = lineColor;

            if (lineMaterial != null)
                _lineRenderer.material = lineMaterial;

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.enabled = false;
        }

        public void Initialize(Camera camera, Canvas canvas)
        {
            _camera = camera;
            _canvas = canvas;

            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _zDistance = _canvas.planeDistance;
            }
            else
            {
                _zDistance = Mathf.Abs(_camera.transform.position.z);
            }
        }

        public void Show(Vector3 startScreenPos, Vector3 endScreenPos)
        {
            if (_camera == null) return;

            _lineRenderer.enabled = true;

            Vector3 startWorld = _camera.ScreenToWorldPoint(new Vector3(startScreenPos.x, startScreenPos.y, _zDistance));
            Vector3 endWorld = _camera.ScreenToWorldPoint(new Vector3(endScreenPos.x, endScreenPos.y, _zDistance));

            float lineZ = _camera.transform.position.z + _zDistance - 0.1f;
            startWorld.z = lineZ;
            endWorld.z = lineZ;

            float screenDistance = Vector2.Distance(startScreenPos, endScreenPos);
            float dynamicHeight = curveHeight * (screenDistance / Screen.height);

            Vector3 controlPoint = (startWorld + endWorld) / 2f + Vector3.up * dynamicHeight;
            controlPoint.z = lineZ;

            for (int i = 0; i < segmentCount; i++)
            {
                float t = i / (float)(segmentCount - 1);
                Vector3 point = CalculateBezierPoint(t, startWorld, controlPoint, endWorld);
                point.z = lineZ;
                _lineRenderer.SetPosition(i, point);
            }
        }

        public void Hide()
        {
            _lineRenderer.enabled = false;
        }

        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }
    }
}