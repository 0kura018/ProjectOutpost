using UnityEngine;
using System.Collections.Generic;

namespace BuildingSystem
{

    public class BuildingGrid : MonoBehaviour
    {
        public static BuildingGrid Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private int _gridWidth = 50;
        [SerializeField] private int _gridHeight = 20;
        [SerializeField] private float _cellSize = 1f;

        [Header("Origin")]
        [Tooltip("����� ������ ���� �����")]
        [SerializeField] private Vector2 _originOffset = Vector2.zero;

        [Header("Debug")]
        [SerializeField] private bool _drawGrid = true;
        [SerializeField] private Color _gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color _occupiedColor = new Color(1f, 0f, 0f, 0.3f);

        private Room[,] _cells;

        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public float CellSize => _cellSize;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _cells = new Room[_gridWidth, _gridHeight];
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector2 localPos = (Vector2)worldPos - _originOffset;
            int x = Mathf.FloorToInt(localPos.x / _cellSize);
            int y = Mathf.FloorToInt(localPos.y / _cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float x = gridPos.x * _cellSize + _cellSize * 0.5f + _originOffset.x;
            float y = gridPos.y * _cellSize + _cellSize * 0.5f + _originOffset.y;
            return new Vector3(x, y, 0f);
        }

        public Vector3 GridToWorldCorner(Vector2Int gridPos)
        {
            float x = gridPos.x * _cellSize + _originOffset.x;
            float y = gridPos.y * _cellSize + _originOffset.y;
            return new Vector3(x, y, 0f);
        }

        public bool CanPlaceRoom(RoomConfig config, Vector2Int gridPos)
        {

            if (gridPos.x < 0 || gridPos.y < 0) return false;
            if (gridPos.x + config.Width > _gridWidth) return false;
            if (gridPos.y + config.Height > _gridHeight) return false;

            for (int x = 0; x < config.Width; x++)
            {
                for (int y = 0; y < config.Height; y++)
                {
                    if (_cells[gridPos.x + x, gridPos.y + y] != null)
                        return false;
                }
            }

            if (config.RequiresConnection)
            {
                if (!HasValidConnection(config, gridPos))
                    return false;
            }

            if (gridPos.y == 0 && !config.CanBuildOnGround)
                return false;

            return true;
        }

        #region Door Connection System

        private bool HasValidConnection(RoomConfig config, Vector2Int gridPos)
        {

            if (gridPos.y == 0 && config.CanBuildOnGround)
                return true;

            if (CanConnectThroughSide(config, gridPos, Direction.Left)) return true;
            if (CanConnectThroughSide(config, gridPos, Direction.Right)) return true;
            if (CanConnectThroughSide(config, gridPos, Direction.Bottom)) return true;
            if (CanConnectThroughSide(config, gridPos, Direction.Top)) return true;

            return false;
        }

        private bool CanConnectThroughSide(RoomConfig config, Vector2Int gridPos, Direction side)
        {

            int[] myDoors = GetDoorsOnSide(config, side);
            if (myDoors == null || myDoors.Length == 0) return false;

            bool isVertical = (side == Direction.Left || side == Direction.Right);
            int myBase = isVertical ? gridPos.y : gridPos.x;
            int myMaxSize = isVertical ? config.Height : config.Width;

            var myBlocks = GroupDoorsIntoBlocks(myDoors, myBase, myMaxSize);
            if (myBlocks.Count == 0) return false;

            var neighborBlocks = CollectNeighborDoorBlocks(config, gridPos, side);
            if (neighborBlocks.Count == 0) return false;

            foreach (var myBlock in myBlocks)
            {
                bool hasMatch = false;
                foreach (var neighborBlock in neighborBlocks)
                {

                    if (myBlock.Start == neighborBlock.Start && myBlock.End == neighborBlock.End)
                    {
                        hasMatch = true;
                        break;
                    }
                }
                if (!hasMatch) return false;
            }

            return true;
        }

        private System.Collections.Generic.List<DoorBlock> CollectNeighborDoorBlocks(
            RoomConfig config, Vector2Int gridPos, Direction side)
        {
            var result = new System.Collections.Generic.List<DoorBlock>();
            var processedRooms = new System.Collections.Generic.HashSet<Room>();

            bool isVertical = (side == Direction.Left || side == Direction.Right);
            int sideLength = isVertical ? config.Height : config.Width;

            for (int offset = 0; offset < sideLength; offset++)
            {
                Vector2Int neighborCell = GetNeighborCell(gridPos, side, offset, config);

                if (!IsValidCell(neighborCell)) continue;

                Room neighbor = _cells[neighborCell.x, neighborCell.y];
                if (neighbor == null || processedRooms.Contains(neighbor)) continue;

                processedRooms.Add(neighbor);

                int[] neighborDoors = GetDoorsOnOppositeSide(neighbor.Config, side);
                if (neighborDoors == null || neighborDoors.Length == 0) continue;

                int neighborBase = isVertical ? neighbor.GridPosition.y : neighbor.GridPosition.x;
                int neighborMaxSize = isVertical ? neighbor.Config.Height : neighbor.Config.Width;

                var blocks = GroupDoorsIntoBlocks(neighborDoors, neighborBase, neighborMaxSize);
                result.AddRange(blocks);
            }

            return result;
        }

        private Vector2Int GetNeighborCell(Vector2Int gridPos, Direction side, int offset, RoomConfig config)
        {
            return side switch
            {
                Direction.Left => new Vector2Int(gridPos.x - 1, gridPos.y + offset),
                Direction.Right => new Vector2Int(gridPos.x + config.Width, gridPos.y + offset),
                Direction.Bottom => new Vector2Int(gridPos.x + offset, gridPos.y - 1),
                Direction.Top => new Vector2Int(gridPos.x + offset, gridPos.y + config.Height),
                _ => gridPos
            };
        }

        private bool IsValidCell(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridWidth && pos.y >= 0 && pos.y < _gridHeight;
        }

        private struct DoorBlock
        {
            public int Start;
            public int End;
        }

        private System.Collections.Generic.List<DoorBlock> GroupDoorsIntoBlocks(int[] doors, int basePos, int maxSize)
        {
            var blocks = new System.Collections.Generic.List<DoorBlock>();
            if (doors == null || doors.Length == 0) return blocks;

            var validDoors = new System.Collections.Generic.List<int>();
            foreach (int d in doors)
            {
                if (d >= 0 && d < maxSize && !validDoors.Contains(d))
                    validDoors.Add(d);
            }
            validDoors.Sort();

            if (validDoors.Count == 0) return blocks;

            int blockStart = validDoors[0];
            int blockEnd = validDoors[0];

            for (int i = 1; i < validDoors.Count; i++)
            {
                if (validDoors[i] == blockEnd + 1)
                {

                    blockEnd = validDoors[i];
                }
                else
                {

                    blocks.Add(new DoorBlock
                    {
                        Start = basePos + blockStart,
                        End = basePos + blockEnd
                    });
                    blockStart = validDoors[i];
                    blockEnd = validDoors[i];
                }
            }

            blocks.Add(new DoorBlock
            {
                Start = basePos + blockStart,
                End = basePos + blockEnd
            });

            return blocks;
        }

        private int[] GetDoorsOnSide(RoomConfig config, Direction side)
        {
            return side switch
            {
                Direction.Left => config.LeftConnections,
                Direction.Right => config.RightConnections,
                Direction.Bottom => config.BottomConnections,
                Direction.Top => config.TopConnections,
                _ => null
            };
        }

        private int[] GetDoorsOnOppositeSide(RoomConfig config, Direction side)
        {
            return side switch
            {
                Direction.Left => config.RightConnections,
                Direction.Right => config.LeftConnections,
                Direction.Bottom => config.TopConnections,
                Direction.Top => config.BottomConnections,
                _ => null
            };
        }

        #endregion

        public void OccupyCells(Room room, Vector2Int gridPos)
        {
            for (int x = 0; x < room.Config.Width; x++)
            {
                for (int y = 0; y < room.Config.Height; y++)
                {
                    _cells[gridPos.x + x, gridPos.y + y] = room;
                }
            }
        }

        public void FreeCells(Room room)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_cells[x, y] == room)
                        _cells[x, y] = null;
                }
            }
        }

        public Room GetRoomAt(Vector2Int gridPos)
        {
            if (gridPos.x < 0 || gridPos.x >= _gridWidth) return null;
            if (gridPos.y < 0 || gridPos.y >= _gridHeight) return null;
            return _cells[gridPos.x, gridPos.y];
        }

        public Vector2Int? FindSnapPosition(RoomConfig config, Vector3 worldPos, float snapRadius)
        {
            Vector2Int baseGridPos = WorldToGrid(worldPos);
            Vector2Int? bestSnap = null;
            float bestDist = snapRadius;

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    Room neighbor = _cells[x, y];
                    if (neighbor == null || neighbor.State != RoomState.Completed) continue;
                    if (_cells[x, y] != neighbor) continue;

                    var snapPositions = GetPossibleSnapPositions(config, neighbor);

                    foreach (var snapPos in snapPositions)
                    {
                        if (!CanPlaceRoom(config, snapPos)) continue;

                        Vector3 snapWorld = GridToWorldCorner(snapPos);
                        snapWorld.x += config.Width * _cellSize * 0.5f;
                        snapWorld.y += config.Height * _cellSize * 0.5f;

                        float dist = Vector2.Distance(worldPos, snapWorld);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestSnap = snapPos;
                        }
                    }
                }
            }

            return bestSnap;
        }

        private System.Collections.Generic.List<Vector2Int> GetPossibleSnapPositions(RoomConfig newConfig, Room existingRoom)
        {
            var positions = new System.Collections.Generic.List<Vector2Int>();
            var existingConfig = existingRoom.Config;
            var existingPos = existingRoom.GridPosition;

            foreach (int existingY in existingConfig.RightConnections)
            {
                foreach (int newY in newConfig.LeftConnections)
                {
                    int snapX = existingPos.x + existingConfig.Width;
                    int snapY = existingPos.y + existingY - newY;
                    positions.Add(new Vector2Int(snapX, snapY));
                }
            }

            foreach (int existingY in existingConfig.LeftConnections)
            {
                foreach (int newY in newConfig.RightConnections)
                {
                    int snapX = existingPos.x - newConfig.Width;
                    int snapY = existingPos.y + existingY - newY;
                    positions.Add(new Vector2Int(snapX, snapY));
                }
            }

            foreach (int existingX in existingConfig.TopConnections)
            {
                foreach (int newX in newConfig.BottomConnections)
                {
                    int snapX = existingPos.x + existingX - newX;
                    int snapY = existingPos.y + existingConfig.Height;
                    positions.Add(new Vector2Int(snapX, snapY));
                }
            }

            foreach (int existingX in existingConfig.BottomConnections)
            {
                foreach (int newX in newConfig.TopConnections)
                {
                    int snapX = existingPos.x + existingX - newX;
                    int snapY = existingPos.y - newConfig.Height;
                    positions.Add(new Vector2Int(snapX, snapY));
                }
            }

            return positions;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGrid) return;

            Gizmos.color = _gridColor;

            for (int x = 0; x <= _gridWidth; x++)
            {
                Vector3 start = new Vector3(x * _cellSize + _originOffset.x, _originOffset.y, 0);
                Vector3 end = new Vector3(x * _cellSize + _originOffset.x, _gridHeight * _cellSize + _originOffset.y, 0);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= _gridHeight; y++)
            {
                Vector3 start = new Vector3(_originOffset.x, y * _cellSize + _originOffset.y, 0);
                Vector3 end = new Vector3(_gridWidth * _cellSize + _originOffset.x, y * _cellSize + _originOffset.y, 0);
                Gizmos.DrawLine(start, end);
            }

            if (_cells == null) return;

            Gizmos.color = _occupiedColor;
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_cells[x, y] != null)
                    {
                        Vector3 center = GridToWorld(new Vector2Int(x, y));
                        Gizmos.DrawCube(center, new Vector3(_cellSize * 0.9f, _cellSize * 0.9f, 0.1f));
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public enum Direction
    {
        Left, Right, Top, Bottom
    }
}
