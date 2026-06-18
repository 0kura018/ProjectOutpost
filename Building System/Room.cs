using UnityEngine;
using System;
using System.Collections.Generic;
using BaseSystem;

namespace BuildingSystem
{

    public class Room : MonoBehaviour
    {

        public event Action<int> BuildStarted;
        public event Action<float> BuildProgressChanged;
        public event Action BuildCompleted;
        public event Action<float> NightPenaltyApplied;

        [Header("Config")]
        [SerializeField] private RoomConfig _config;
        public RoomConfig Config => _config;

        [Header("State")]
        [SerializeField] private RoomState _state = RoomState.Planning;
        public RoomState State => _state;

        [Header("Grid Position")]
        [SerializeField] private Vector2Int _gridPosition;
        public Vector2Int GridPosition => _gridPosition;

        [Header("Build Progress")]
        [SerializeField] private float _buildProgress;
        public float BuildProgress => _buildProgress;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _planningColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private Color _buildingColor = new Color(1f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color _completedColor = Color.white;
        [SerializeField] private Color _invalidColor = new Color(1f, 0.3f, 0.3f, 0.5f);

        [Header("Door Settings")]
        [SerializeField] private Color _doorColor = new Color(0.2f, 0.9f, 0.3f, 1f);

        private bool _isValidPlacement = false;

        private GameObject _doorsContainer;

        [Header("Room Navigation")]
        public List<RoomTransition> Transitions = new();
        public List<RoomSpot> Spots = new();

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Initialize(RoomConfig config)
        {
            _config = config;
            _state = RoomState.Planning;
            _buildProgress = 0f;
            UpdateVisuals();
        }

        public void SetGridPosition(Vector2Int gridPos)
        {
            _gridPosition = gridPos;

            if (BuildingGrid.Instance != null)
            {
                Vector3 worldPos = BuildingGrid.Instance.GridToWorldCorner(gridPos);

                float offsetX = _config.Width * BuildingGrid.Instance.CellSize * 0.5f;
                float offsetY = _config.Height * BuildingGrid.Instance.CellSize * 0.5f;

                transform.position = worldPos + new Vector3(offsetX, offsetY, 0f);
            }

            if (_state == RoomState.Planning)
            {
                RebuildDoors();
            }
        }

        public void SetWorldPosition(Vector3 worldPos)
        {
            transform.position = worldPos;

            if (_state == RoomState.Planning)
            {
                RebuildDoors();
            }
        }

        public void SetPlacementValidity(bool isValid)
        {
            _isValidPlacement = isValid;
            UpdateVisuals();
        }

        #region Door System

        private void RebuildDoors()
        {
            DestroyDoors();

            if (_config == null) return;
            if (_state != RoomState.Planning) return;

            _doorsContainer = new GameObject("DoorsContainer");
            _doorsContainer.transform.position = Vector3.zero;

            float cellSize = BuildingGrid.Instance != null ? BuildingGrid.Instance.CellSize : 1f;

            CreateMergedDoors(_config.LeftConnections, Direction.Left, _config.Height, cellSize);
            CreateMergedDoors(_config.RightConnections, Direction.Right, _config.Height, cellSize);
            CreateMergedDoors(_config.BottomConnections, Direction.Bottom, _config.Width, cellSize);
            CreateMergedDoors(_config.TopConnections, Direction.Top, _config.Width, cellSize);
        }

        private void CreateMergedDoors(int[] connections, Direction dir, int maxIndex, float cellSize)
        {
            if (connections == null || connections.Length == 0) return;

            var sorted = new System.Collections.Generic.List<int>();
            foreach (int idx in connections)
            {
                if (idx >= 0 && idx < maxIndex && !sorted.Contains(idx))
                    sorted.Add(idx);
            }
            sorted.Sort();

            if (sorted.Count == 0) return;

            var groups = new System.Collections.Generic.List<(int start, int count)>();
            int groupStart = sorted[0];
            int groupCount = 1;

            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] == sorted[i - 1] + 1)
                {

                    groupCount++;
                }
                else
                {

                    groups.Add((groupStart, groupCount));
                    groupStart = sorted[i];
                    groupCount = 1;
                }
            }
            groups.Add((groupStart, groupCount));

            foreach (var (start, count) in groups)
            {
                CreateMergedDoor(dir, start, count, cellSize);
            }
        }

        private void CreateMergedDoor(Direction dir, int startIndex, int count, float cellSize)
        {
            bool isVertical = (dir == Direction.Left || dir == Direction.Right);

            float centerOffset = startIndex + (count - 1) * 0.5f + 0.5f;
            Vector3 doorWorldPos = GetMergedDoorWorldPosition(dir, centerOffset, cellSize);

            float thickness = cellSize * 0.15f;
            float length = cellSize * 0.5f + (count - 1) * cellSize;

            float w = isVertical ? thickness : length;
            float h = isVertical ? length : thickness;

            var doorGO = new GameObject($"Door_{dir}_{startIndex}_{count}");
            doorGO.transform.SetParent(_doorsContainer.transform);
            doorGO.transform.position = new Vector3(doorWorldPos.x, doorWorldPos.y, -1f);
            doorGO.transform.localScale = new Vector3(w, h, 1f);

            var sr = doorGO.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateWhiteSprite();
            sr.color = _doorColor;
            sr.sortingOrder = 150;
        }

        public Vector3 GetConnectionWorldPosition(Direction dir, int index)
        {
            float cellSize = BuildingGrid.Instance != null ? BuildingGrid.Instance.CellSize : 1f;
            return GetMergedDoorWorldPosition(dir, index + 0.5f, cellSize);
        }

        public Vector3 GetMergedDoorWorldPosition(Direction dir, float centerOffset, float cellSize)
        {
            Vector3 roomCenter = transform.position;

            float halfW = _config.Width * cellSize * 0.5f;
            float halfH = _config.Height * cellSize * 0.5f;

            return dir switch
            {
                Direction.Left => roomCenter + new Vector3(-halfW, -halfH + centerOffset * cellSize, 0),
                Direction.Right => roomCenter + new Vector3(halfW, -halfH + centerOffset * cellSize, 0),
                Direction.Bottom => roomCenter + new Vector3(-halfW + centerOffset * cellSize, -halfH, 0),
                Direction.Top => roomCenter + new Vector3(-halfW + centerOffset * cellSize, halfH, 0),
                _ => roomCenter
            };
        }

        private void DestroyDoors()
        {
            if (_doorsContainer != null)
            {
                Destroy(_doorsContainer);
                _doorsContainer = null;
            }
        }

        private static Sprite _cachedSprite;
        private static Sprite GetOrCreateWhiteSprite()
        {
            if (_cachedSprite == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var colors = new Color[16];
                for (int i = 0; i < 16; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                tex.filterMode = FilterMode.Point;
                _cachedSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return _cachedSprite;
        }

        #endregion

        #region Building with Game Time

        private TimeSystem.TimeBasedProgress _buildProgressComponent;

        public void StartBuilding()
        {
            if (_state != RoomState.Planning) return;

            _state = RoomState.Building;
            _buildProgress = 0f;

            DestroyDoors();

            if (_config.BuildTime <= 0f)
            {
                CompleteBuilding();
                return;
            }

            _buildProgressComponent = gameObject.AddComponent<TimeSystem.TimeBasedProgress>();
            _buildProgressComponent.Initialize(_config.BuildTime, true);
            _buildProgressComponent.OnProgressChanged += OnBuildProgressChanged;
            _buildProgressComponent.OnCompleted += CompleteBuilding;
            _buildProgressComponent.OnNightPenaltyApplied += OnNightPenalty;

            int estimatedTicks = _buildProgressComponent.EstimateTicksToComplete();
            BuildStarted?.Invoke(estimatedTicks);

            RoomBuildUI buildUI = FindObjectOfType<RoomBuildUI>();
            if (buildUI != null)
            {
                buildUI.ShowTimerForRoom(this);
            }
            else
            {
                Debug.LogWarning("[Room] RoomBuildUI не найден на сцене!");
            }

            UpdateVisuals();
        }

        private void OnBuildProgressChanged(float progress)
        {
            _buildProgress = progress;
            BuildProgressChanged?.Invoke(progress);
        }

        private void OnNightPenalty(float hoursLost)
        {
            Debug.Log($"[Room] '{_config.RoomName}' night penalty: -{hoursLost:F1}h");
            NightPenaltyApplied?.Invoke(hoursLost);
        }

        public void CompleteBuilding()
        {
            _state = RoomState.Completed;
            _buildProgress = 1f;
            UpdateVisuals();

            DestroyDoors();

            if (_buildProgressComponent != null)
            {
                _buildProgressComponent.OnProgressChanged -= OnBuildProgressChanged;
                _buildProgressComponent.OnCompleted -= CompleteBuilding;
                _buildProgressComponent.OnNightPenaltyApplied -= OnNightPenalty;
                Destroy(_buildProgressComponent);
                _buildProgressComponent = null;
            }

            BuildCompleted?.Invoke();
            Debug.Log($"[Room] '{_config.RoomName}' completed at {_gridPosition}");
        }

        public int GetEstimatedTicksToComplete()
        {
            if (_buildProgressComponent != null)
                return _buildProgressComponent.EstimateTicksToComplete();
            return 0;
        }

        #endregion

        private void UpdateVisuals()
        {
            if (_spriteRenderer == null) return;

            _spriteRenderer.color = _state switch
            {
                RoomState.Planning => _isValidPlacement ? _planningColor : _invalidColor,
                RoomState.Building => _buildingColor,
                RoomState.Completed => _completedColor,
                _ => Color.white
            };
        }

        public void Demolish()
        {
            if (BuildingGrid.Instance != null)
            {
                BuildingGrid.Instance.FreeCells(this);
            }

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.UnregisterRoom(this);
            }

            DestroyDoors();
            Debug.Log($"[Room] '{_config.RoomName}' demolished");
            Destroy(gameObject);
        }

        public void GetNeighbors(List<Room> outNeighbors)
        {
            outNeighbors.Clear();
            foreach (var t in Transitions)
            {
                if (t == null) continue;
                if (t.From == this && t.To != null) outNeighbors.Add(t.To);
                else if (t.To == this && t.From != null) outNeighbors.Add(t.From);
            }
        }

        public RoomTransition GetTransitionTo(Room to)
        {
            foreach (var t in Transitions)
            {
                if (t != null && t.Connects(this, to))
                    return t;
            }
            return null;
        }

        private void OnDestroy()
        {
            DestroyDoors();
        }
    }

    public enum RoomState
    {
        Planning,
        Building,
        Completed
    }
}
