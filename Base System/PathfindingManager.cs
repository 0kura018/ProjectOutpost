using System.Collections.Generic;
using UnityEngine;
using BuildingSystem;

namespace BaseSystem
{

    public class PathfindingManager : MonoBehaviour
    {
        public static PathfindingManager Instance { get; private set; }

        private Queue<Room> _queue = new();
        private HashSet<Room> _visited = new();
        private Dictionary<Room, Room> _cameFrom = new();
        private List<Room> _neighborsTemp = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public bool FindPath(Room start, Room target, List<Room> outPath, bool allowRandomness = true)
        {
            outPath.Clear();
            if (start == null || target == null) return false;
            if (start == target)
            {
                outPath.Add(start);
                return true;
            }

            _queue.Clear();
            _visited.Clear();
            _cameFrom.Clear();

            _queue.Enqueue(start);
            _visited.Add(start);

            while (_queue.Count > 0)
            {
                var current = _queue.Dequeue();

                current.GetNeighbors(_neighborsTemp);

                if (allowRandomness && _neighborsTemp.Count > 1)
                {
                    int swapIndex = Random.Range(0, _neighborsTemp.Count);
                    var tmp = _neighborsTemp[0];
                    _neighborsTemp[0] = _neighborsTemp[swapIndex];
                    _neighborsTemp[swapIndex] = tmp;
                }

                foreach (var next in _neighborsTemp)
                {
                    if (next == null) continue;
                    if (_visited.Contains(next)) continue;

                    _visited.Add(next);
                    _cameFrom[next] = current;
                    _queue.Enqueue(next);

                    if (next == target)
                    {
                        ReconstructPath(start, target, outPath);
                        return true;
                    }
                }
            }

            return false;
        }

        private void ReconstructPath(Room start, Room target, List<Room> outPath)
        {
            var cur = target;
            while (cur != start)
            {
                outPath.Add(cur);
                cur = _cameFrom[cur];
            }
            outPath.Add(start);
            outPath.Reverse();
        }
    }
}
