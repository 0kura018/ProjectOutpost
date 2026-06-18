using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace BuildingSystem
{

    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private BuildingGrid _grid;

        [Header("Settings")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private bool _continuousBuildMode = true;
        [Tooltip("���� ��������� � ����� ��������� ����� ������������")]

        [Header("Free Doors Display")]
        [SerializeField] private Color _freeDoorColor = new Color(1f, 0.8f, 0.2f, 0.8f);

        [Header("State")]
        [SerializeField] private BuildMode _mode = BuildMode.None;
        public BuildMode Mode => _mode;

        public bool ContinuousBuildMode
        {
            get => _continuousBuildMode;
            set => _continuousBuildMode = value;
        }

        private RoomConfig _selectedConfig;
        private Room _previewRoom;
        private Vector2Int _lastPreviewPos;

        private List<Room> _builtRooms = new();
        public IReadOnlyList<Room> BuiltRooms => _builtRooms;

        private GameObject _freeDoorsContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_camera == null)
                _camera = Camera.main;

            if (_grid == null)
                _grid = FindAnyObjectByType<BuildingGrid>();
        }

        private void Update()
        {
            if (_mode == BuildMode.Build && _previewRoom != null)
            {
                UpdatePreview();
                HandleBuildInput();
            }
            else if (_mode == BuildMode.Demolish)
            {
                HandleDemolishInput();
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelBuildMode();
            }
        }

        public void StartBuildMode(RoomConfig config)
        {
            if (config == null) return;

            CancelBuildMode();

            _selectedConfig = config;
            _mode = BuildMode.Build;

            CreatePreviewRoom();
            ShowFreeDoors();

            Debug.Log($"[BuildingSystem] Build mode: {config.RoomName}");
        }

        public void StartDemolishMode()
        {
            CancelBuildMode();
            _mode = BuildMode.Demolish;
            Debug.Log("[BuildingSystem] Demolish mode");
        }

        public void CancelBuildMode()
        {
            _mode = BuildMode.None;
            _selectedConfig = null;

            if (_previewRoom != null)
            {
                Destroy(_previewRoom.gameObject);
                _previewRoom = null;
            }

            HideFreeDoors();
        }

        #region Free Doors Display

        private void ShowFreeDoors()
        {
            HideFreeDoors();

            _freeDoorsContainer = new GameObject("FreeDoorsContainer");

            float cellSize = _grid != null ? _grid.CellSize : 1f;

            foreach (var room in _builtRooms)
            {
                if (room == null || room.Config == null) continue;

                ShowFreeDoorsForRoom(room, cellSize);
            }
        }

        private void ShowFreeDoorsForRoom(Room room, float cellSize)
        {
            var config = room.Config;
            var pos = room.GridPosition;

            if (config.LeftConnections != null)
            {
                var freeDoors = GetFreeDoorsOnSide(room, Direction.Left);
                CreateFreeDoorIndicators(room, Direction.Left, freeDoors, cellSize);
            }

            if (config.RightConnections != null)
            {
                var freeDoors = GetFreeDoorsOnSide(room, Direction.Right);
                CreateFreeDoorIndicators(room, Direction.Right, freeDoors, cellSize);
            }

            if (config.BottomConnections != null)
            {
                var freeDoors = GetFreeDoorsOnSide(room, Direction.Bottom);
                CreateFreeDoorIndicators(room, Direction.Bottom, freeDoors, cellSize);
            }

            if (config.TopConnections != null)
            {
                var freeDoors = GetFreeDoorsOnSide(room, Direction.Top);
                CreateFreeDoorIndicators(room, Direction.Top, freeDoors, cellSize);
            }
        }

        private List<int> GetFreeDoorsOnSide(Room room, Direction dir)
        {
            var freeDoors = new List<int>();
            var config = room.Config;
            var pos = room.GridPosition;

            int[] connections = dir switch
            {
                Direction.Left => config.LeftConnections,
                Direction.Right => config.RightConnections,
                Direction.Bottom => config.BottomConnections,
                Direction.Top => config.TopConnections,
                _ => null
            };

            if (connections == null) return freeDoors;

            int maxIndex = (dir == Direction.Left || dir == Direction.Right) ? config.Height : config.Width;

            foreach (int doorIndex in connections)
            {
                if (doorIndex < 0 || doorIndex >= maxIndex) continue;

                Vector2Int checkPos = dir switch
                {
                    Direction.Left => new Vector2Int(pos.x - 1, pos.y + doorIndex),
                    Direction.Right => new Vector2Int(pos.x + config.Width, pos.y + doorIndex),
                    Direction.Bottom => new Vector2Int(pos.x + doorIndex, pos.y - 1),
                    Direction.Top => new Vector2Int(pos.x + doorIndex, pos.y + config.Height),
                    _ => pos
                };

                var neighbor = _grid.GetRoomAt(checkPos);
                if (neighbor == null)
                {
                    freeDoors.Add(doorIndex);
                }
            }

            return freeDoors;
        }

        private void CreateFreeDoorIndicators(Room room, Direction dir, List<int> doorIndices, float cellSize)
        {
            if (doorIndices.Count == 0) return;

            doorIndices.Sort();
            var groups = new List<(int start, int count)>();
            int groupStart = doorIndices[0];
            int groupCount = 1;

            for (int i = 1; i < doorIndices.Count; i++)
            {
                if (doorIndices[i] == doorIndices[i - 1] + 1)
                {
                    groupCount++;
                }
                else
                {
                    groups.Add((groupStart, groupCount));
                    groupStart = doorIndices[i];
                    groupCount = 1;
                }
            }
            groups.Add((groupStart, groupCount));

            foreach (var (start, count) in groups)
            {
                CreateFreeDoorIndicator(room, dir, start, count, cellSize);
            }
        }

        private void CreateFreeDoorIndicator(Room room, Direction dir, int startIndex, int count, float cellSize)
        {
            bool isVertical = (dir == Direction.Left || dir == Direction.Right);

            float centerOffset = startIndex + (count - 1) * 0.5f + 0.5f;
            Vector3 worldPos = GetFreeDoorWorldPosition(room, dir, centerOffset, cellSize);

            float thickness = cellSize * 0.15f;
            float length = cellSize * 0.5f + (count - 1) * cellSize;

            float w = isVertical ? thickness : length;
            float h = isVertical ? length : thickness;

            var doorGO = new GameObject($"FreeDoor_{room.name}_{dir}_{startIndex}");
            doorGO.transform.SetParent(_freeDoorsContainer.transform);
            doorGO.transform.position = new Vector3(worldPos.x, worldPos.y, -1f);
            doorGO.transform.localScale = new Vector3(w, h, 1f);

            var sr = doorGO.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateWhiteSprite();
            sr.color = _freeDoorColor;
            sr.sortingOrder = 200;
        }

        private Vector3 GetFreeDoorWorldPosition(Room room, Direction dir, float centerOffset, float cellSize)
        {
            Vector3 roomCenter = room.transform.position;
            float halfW = room.Config.Width * cellSize * 0.5f;
            float halfH = room.Config.Height * cellSize * 0.5f;

            return dir switch
            {
                Direction.Left => roomCenter + new Vector3(-halfW, -halfH + centerOffset * cellSize, 0),
                Direction.Right => roomCenter + new Vector3(halfW, -halfH + centerOffset * cellSize, 0),
                Direction.Bottom => roomCenter + new Vector3(-halfW + centerOffset * cellSize, -halfH, 0),
                Direction.Top => roomCenter + new Vector3(-halfW + centerOffset * cellSize, halfH, 0),
                _ => roomCenter
            };
        }

        private void HideFreeDoors()
        {
            if (_freeDoorsContainer != null)
            {
                Destroy(_freeDoorsContainer);
                _freeDoorsContainer = null;
            }
        }

        private static Sprite _cachedWhiteSprite;
        private static Sprite GetOrCreateWhiteSprite()
        {
            if (_cachedWhiteSprite == null)
            {

                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var colors = new Color[16];
                for (int i = 0; i < 16; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                tex.filterMode = FilterMode.Point;

                _cachedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return _cachedWhiteSprite;
        }

        #endregion

        [Header("Snap Settings")]
        [SerializeField] private float _snapRadius = 2f;
        [SerializeField] private bool _enableSnapping = true;

        private Vector2Int? _snappedGridPos;

        private void CreatePreviewRoom()
        {
            if (_selectedConfig.RoomPrefab == null)
            {
                Debug.LogError($"[BuildingSystem] RoomPrefab is null for {_selectedConfig.RoomName}");
                return;
            }

            var go = Instantiate(_selectedConfig.RoomPrefab);
            _previewRoom = go.GetComponent<Room>();

            if (_previewRoom == null)
            {
                _previewRoom = go.AddComponent<Room>();
            }

            _previewRoom.Initialize(_selectedConfig);
        }

        private void UpdatePreview()
        {
            if (_grid == null || _camera == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -_camera.transform.position.z));

            float halfWidth = _selectedConfig.Width * _grid.CellSize * 0.5f;
            float halfHeight = _selectedConfig.Height * _grid.CellSize * 0.5f;
            Vector3 centeredPos = new Vector3(worldPos.x, worldPos.y, 0f);

            Vector2Int gridPos;
            _snappedGridPos = null;

            if (_enableSnapping)
            {
                Vector2Int? snapPos = _grid.FindSnapPosition(_selectedConfig, centeredPos, _snapRadius);
                if (snapPos.HasValue)
                {
                    gridPos = snapPos.Value;
                    _snappedGridPos = gridPos;
                }
                else
                {

                    Vector3 cornerPos = centeredPos - new Vector3(halfWidth, halfHeight, 0);
                    gridPos = _grid.WorldToGrid(cornerPos);
                }
            }
            else
            {
                Vector3 cornerPos = centeredPos - new Vector3(halfWidth, halfHeight, 0);
                gridPos = _grid.WorldToGrid(cornerPos);
            }

            if (_snappedGridPos.HasValue)
            {

                _previewRoom.SetGridPosition(gridPos);
            }
            else
            {

                _previewRoom.SetWorldPosition(centeredPos);
                _previewRoom.SetGridPosition(gridPos);
            }

            _lastPreviewPos = gridPos;

            bool canPlace = _grid.CanPlaceRoom(_selectedConfig, gridPos);
            canPlace = canPlace && HasEnoughResources(_selectedConfig);

            _previewRoom.SetPlacementValidity(canPlace);
        }

        private void HandleBuildInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                TryPlaceRoom();
            }

            if (mouse.rightButton.wasPressedThisFrame)
            {
                CancelBuildMode();
            }
        }

        private void HandleDemolishInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                TryDemolishRoom();
            }

            if (mouse.rightButton.wasPressedThisFrame)
            {
                CancelBuildMode();
            }
        }

        private void TryPlaceRoom()
        {
            if (_grid == null || _previewRoom == null) return;

            Vector2Int gridPos = _previewRoom.GridPosition;

            if (!_grid.CanPlaceRoom(_selectedConfig, gridPos))
            {
                Debug.Log("[BuildingSystem] Cannot place room here!");
                return;
            }

            if (!HasEnoughResources(_selectedConfig))
            {
                Debug.Log("[BuildingSystem] Not enough resources!");
                return;
            }

            SpendResources(_selectedConfig);

            _grid.OccupyCells(_previewRoom, gridPos);

            _previewRoom.StartBuilding();

            _builtRooms.Add(_previewRoom);

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.RegisterRoom(_previewRoom);
            }

            Debug.Log($"[BuildingSystem] Placed '{_selectedConfig.RoomName}' at {gridPos}");

            _previewRoom = null;

            if (_continuousBuildMode)
            {

                CreatePreviewRoom();
                ShowFreeDoors();
            }
            else
            {

                CancelBuildMode();
            }
        }

        private void TryDemolishRoom()
        {
            if (_grid == null || _camera == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -_camera.transform.position.z));
            Vector2Int gridPos = _grid.WorldToGrid(worldPos);

            Room room = _grid.GetRoomAt(gridPos);
            if (room != null)
            {
                _builtRooms.Remove(room);
                room.Demolish();

                if (_mode == BuildMode.Build)
                {
                    ShowFreeDoors();
                }
            }
        }

        private bool HasEnoughResources(RoomConfig config)
        {
            if (config.BuildCost == null) return true;

            foreach (var cost in config.BuildCost)
            {
                if (!ResourceManager.Instance?.HasResource(cost.Type, cost.Amount) ?? false)
                    return false;
            }
            return true;
        }

        private void SpendResources(RoomConfig config)
        {
            if (config.BuildCost == null) return;

            foreach (var cost in config.BuildCost)
            {
                ResourceManager.Instance?.SpendResource(cost.Type, cost.Amount);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public enum BuildMode
    {
        None,
        Build,
        Demolish
    }
}
