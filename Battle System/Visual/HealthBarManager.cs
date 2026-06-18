using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.Visual
{
    public class HealthBarManager : MonoBehaviour
    {
        public static HealthBarManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private GameObject _healthBarPrefab;
        [SerializeField] private Vector2 _worldOffset = new Vector2(0f, 1.5f);
        [SerializeField] private Vector2 _screenOffset = Vector2.zero;

        [Header("Stacking")]
        [SerializeField] private float _stackGroupDistanceX = 50f;
        [SerializeField] private float _stackGroupDistanceY = 30f;
        [SerializeField] private float _stackOffsetY = 25f;

        [Header("References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Camera _camera;

        private readonly Dictionary<Transform, HealthBarUI> _activeBars = new();
        private readonly List<HealthBarUI> _pool = new();
        private readonly List<HealthBarUI> _sortList = new();
        private readonly List<Transform> _toRemove = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_canvas == null)
            {
                _canvas = GetComponentInChildren<Canvas>();
                if (_canvas == null) CreateCanvas();
            }

            if (_camera == null) _camera = Camera.main;
        }

        private void CreateCanvas()
        {
            var canvasGO = new GameObject("HealthBarCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        private void LateUpdate()
        {
            UpdateAllPositions();
            CleanupDeadBars();
            ApplyStacking();
        }

        public HealthBarUI RegisterUnit(Transform unitTransform, int maxHealth, int currentHealth, bool isAlly, float customOffsetY = -1f)
        {
            if (unitTransform == null) return null;
            if (_activeBars.ContainsKey(unitTransform)) return _activeBars[unitTransform];

            var bar = GetFromPool();
            if (bar == null) return null;

            float offsetY = customOffsetY >= 0f ? customOffsetY : _worldOffset.y;
            bar.Initialize(unitTransform, maxHealth, currentHealth, isAlly, offsetY);
            bar.gameObject.SetActive(true);
            _activeBars[unitTransform] = bar;
            return bar;
        }

        public void UnregisterUnit(Transform unitTransform)
        {
            if (unitTransform == null) return;
            if (!_activeBars.TryGetValue(unitTransform, out var bar)) return;

            bar.RectTransform.position = new Vector3(10000f, 10000f, 0f);
            bar.gameObject.SetActive(false);
            bar.Clear();
            _pool.Add(bar);
            _activeBars.Remove(unitTransform);
        }

        public void UpdateHealth(Transform unitTransform, int currentHealth, int maxHealth)
        {
            if (unitTransform == null) return;
            if (_activeBars.TryGetValue(unitTransform, out var bar))
            {
                bar.UpdateValue(currentHealth, maxHealth);

                if (currentHealth <= 0)
                {
                    UnregisterUnit(unitTransform);
                }
            }
        }

        private HealthBarUI GetFromPool()
        {
            if (_pool.Count > 0)
            {
                var bar = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
                return bar;
            }

            if (_healthBarPrefab == null) return null;

            var instance = Instantiate(_healthBarPrefab, _canvas.transform);
            var barUI = instance.GetComponent<HealthBarUI>();
            if (barUI == null) barUI = instance.AddComponent<HealthBarUI>();
            return barUI;
        }

        private void UpdateAllPositions()
        {
            if (_camera == null) return;

            foreach (var kvp in _activeBars)
            {
                var unitTransform = kvp.Key;
                var bar = kvp.Value;

                if (unitTransform == null || bar == null)
                {
                    _toRemove.Add(unitTransform);
                    continue;
                }

                if (!bar.IsAlive)
                {
                    bar.gameObject.SetActive(false);
                    _toRemove.Add(unitTransform);
                    continue;
                }

                Vector3 worldPos = unitTransform.position + new Vector3(_worldOffset.x, bar.WorldOffsetY, 0f);
                Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);

                if (screenPos.z < 0)
                {
                    bar.gameObject.SetActive(false);
                    continue;
                }

                if (!bar.gameObject.activeSelf)
                {
                    bar.gameObject.SetActive(true);
                }

                screenPos.x += _screenOffset.x;
                screenPos.y += _screenOffset.y;

                bar.RectTransform.position = screenPos;
                bar.BaseScreenY = screenPos.y;
            }
        }

        private void CleanupDeadBars()
        {
            foreach (var unitTransform in _toRemove)
            {
                UnregisterUnit(unitTransform);
            }
            _toRemove.Clear();
        }

        private void ApplyStacking()
        {
            _sortList.Clear();
            foreach (var bar in _activeBars.Values)
                if (bar != null && bar.gameObject.activeSelf) _sortList.Add(bar);

            if (_sortList.Count < 2) return;

            _sortList.Sort((a, b) => a.BaseScreenY.CompareTo(b.BaseScreenY));

            for (int i = 0; i < _sortList.Count; i++)
            {
                var current = _sortList[i];
                float currentY = current.BaseScreenY;

                for (int j = 0; j < i; j++)
                {
                    var other = _sortList[j];
                    float dx = Mathf.Abs(current.RectTransform.position.x - other.RectTransform.position.x);
                    float dy = Mathf.Abs(currentY - other.RectTransform.position.y);

                    if (dx < _stackGroupDistanceX && dy < _stackGroupDistanceY)
                        currentY = other.RectTransform.position.y + _stackOffsetY;
                }

                var pos = current.RectTransform.position;
                pos.y = currentY;
                current.RectTransform.position = pos;
            }
        }

        public void ClearAll()
        {
            foreach (var bar in _activeBars.Values)
            {
                if (bar != null) { bar.gameObject.SetActive(false); bar.Clear(); _pool.Add(bar); }
            }
            _activeBars.Clear();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}