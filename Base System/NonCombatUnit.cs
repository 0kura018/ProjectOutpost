using UnityEngine;
using BuildingSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace BaseSystem
{

    public enum NonCombatUnitState
    {
        Idle,
        Moving,
        Working,
        Sleeping,
        Guarding
    }

    [RequireComponent(typeof(Transform))]
    public class NonCombatUnit : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private NonCombatTeamMember _memberConfig;

        [Header("Sleep")]
        [SerializeField] private RoomConfig _sleepRoomConfig;
        [SerializeField] private bool _isNightSleeping = false;

        [Header("Current State")]
        [SerializeField] private Room _currentRoom;
        [SerializeField] private NonCombatUnitState _currentState = NonCombatUnitState.Idle;
        public NonCombatUnitState CurrentState => _currentState;

        [Header("Movement")]
        [SerializeField] private float _movementSpeed = 2f;
        [SerializeField] private float _approachTolerance = 0.05f;

        [Header("Behavior")]
        [SerializeField] private float _idleTimeInRoom = 5f;

        [Header("Work Assignment")]
        [SerializeField] private Room _assignedWorkRoom;
        public Room AssignedWorkRoom => _assignedWorkRoom;

        private RoomSpot _currentSpot;
        private List<Room> _roomPath = new();

        public void Initialize(NonCombatTeamMember config, Room startRoom)
        {
            _memberConfig = config;
            _sleepRoomConfig = config.SleepRoomConfig;
            _currentRoom = startRoom;
            _movementSpeed = config.MovementSpeed;
            _idleTimeInRoom = config.IdleTimeInRoom;

            if (_currentRoom != null)
            {
                transform.position = _currentRoom.transform.position;
            }

            UpdateStateFromSpot();
        }

                public void AssignToWork(Room room)
        {
            _assignedWorkRoom = room;
            if (_isNightSleeping) return;

            StopAllCoroutines();
            StartCoroutine(BehaviorRoutine());
        }

        public void ClearWorkAssignment()
        {
            _assignedWorkRoom = null;

        }

        private void ReleaseCurrentSpot()
        {
            if (_currentSpot != null)
            {
                _currentSpot.SetOccupied(false);
                _currentSpot = null;
            }
        }

        public void StartNightSleep()
        {
            if (_isNightSleeping) return;

            _isNightSleeping = true;
            StopAllCoroutines();
            StartCoroutine(NightSleepRoutine());
        }

        public void EndNightSleep()
        {
            if (!_isNightSleeping) return;

            _isNightSleeping = false;
            StopAllCoroutines();
            StartCoroutine(BehaviorRoutine());
        }

        private IEnumerator NightSleepRoutine()
        {
            yield return new WaitUntil(() => _currentRoom != null);

            while (_isNightSleeping)
            {

                if (_currentSpot == null || _currentSpot.Type != RoomSpotType.Sleep)
                {
                    Room sleepRoom = FindSleepRoom();
                    if (sleepRoom != null)
                    {

                        yield return StartCoroutine(GoToRoomRoutine(sleepRoom, RoomSpotType.Sleep, true));
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private Room FindSleepRoom()
        {
            List<Room> rooms = new();

            if (BuildingManager.Instance != null)
            {
                rooms = BuildingManager.Instance.GetCompletedRooms()
                    .Where(r => !IsElevatorRoom(r))
                    .ToList();
            }
            else
            {
                rooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None)
                    .Where(r => r.State == RoomState.Completed && !IsElevatorRoom(r))
                    .ToList();
            }

            if (_sleepRoomConfig != null)
            {
                rooms = rooms.Where(r => r.Config == _sleepRoomConfig).ToList();
            }

            if (rooms.Count > 1)
            {
                for (int i = 0; i < rooms.Count; i++)
                {
                    int rnd = Random.Range(i, rooms.Count);
                    var temp = rooms[i];
                    rooms[i] = rooms[rnd];
                    rooms[rnd] = temp;
                }
            }

            foreach (var room in rooms)
            {
                if (RoomSpotSelector.SelectSpotStrict(room, RoomSpotType.Sleep) != null)
                {
                    return room;
                }
            }

            return null;
        }

        private void SetState(NonCombatUnitState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
        }

        private void UpdateStateFromSpot()
        {
            if (_currentSpot == null)
            {
                SetState(NonCombatUnitState.Idle);
                return;
            }

            switch (_currentSpot.Type)
            {
                case RoomSpotType.Work:
                    SetState(NonCombatUnitState.Working);
                    break;
                case RoomSpotType.Sleep:
                    SetState(NonCombatUnitState.Sleeping);
                    break;
                case RoomSpotType.Guard:
                    SetState(NonCombatUnitState.Guarding);
                    break;
                default:
                    SetState(NonCombatUnitState.Idle);
                    break;
            }
        }

        private bool IsElevatorRoom(Room room)
        {
            if (room == null || room.Config == null) return false;

            string roomName = room.Config.RoomName.ToLower();
            if (roomName.Contains("elevator") || roomName.Contains("lift")) return true;

            if (room.Spots.Count == 0) return true;

            return false;
        }

        private IEnumerator Start()
        {
            yield return StartCoroutine(BehaviorRoutine());
        }

        private IEnumerator BehaviorRoutine()
        {

            yield return new WaitUntil(() => _currentRoom != null);

            while (true)
            {

                if (_assignedWorkRoom != null)
                {

                    if (_currentRoom != _assignedWorkRoom || _currentSpot == null || _currentSpot.Type != RoomSpotType.Work)
                    {
                        yield return StartCoroutine(GoToRoomRoutine(_assignedWorkRoom, RoomSpotType.Work));
                    }

                    if (_currentSpot == null || _currentSpot.Type != RoomSpotType.Work)
                    {
                        yield return new WaitForSeconds(2f);
                        continue;
                    }

                    while (_assignedWorkRoom != null && _currentRoom == _assignedWorkRoom && _currentSpot != null && _currentSpot.Type == RoomSpotType.Work)
                    {
                        yield return new WaitForSeconds(1f);
                    }

                    continue;
                }

                List<Room> allRooms = new();
                if (BuildingManager.Instance != null)
                {
                    allRooms = BuildingManager.Instance.GetCompletedRooms()
                        .Where(r => !IsElevatorRoom(r))
                        .ToList();
                }
                else
                {
                    allRooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None)
                        .Where(r => r.State == RoomState.Completed && !IsElevatorRoom(r))
                        .ToList();
                }

                if (allRooms.Count > 0)
                {

                    var activityTypes = System.Enum.GetValues(typeof(RoomSpotType));
                    RoomSpotType activity = (RoomSpotType)activityTypes.GetValue(Random.Range(0, activityTypes.Length));

                    Room target = _currentRoom;
                    if (allRooms.Count > 1)
                    {
                        var otherRooms = allRooms.Where(r => r != _currentRoom).ToList();
                        if (otherRooms.Count > 0)
                        {
                            target = otherRooms[Random.Range(0, otherRooms.Count)];
                        }
                    }

                    Room startRoom = _currentRoom;
                    RoomSpot startSpot = _currentSpot;

                    yield return StartCoroutine(GoToRoomRoutine(target, activity));

                    if (_currentRoom == startRoom && _currentSpot == startSpot && target != startRoom)
                    {
                        yield return new WaitForSeconds(1f);
                        continue;
                    }
                }

                yield return new WaitForSeconds(_idleTimeInRoom);
            }
        }

        public void GoToRoom(Room targetRoom, RoomSpotType spotType)
        {
            StopAllCoroutines();
            StartCoroutine(GoToRoomRoutine(targetRoom, spotType));
        }

        private IEnumerator GoToRoomRoutine(Room targetRoom, RoomSpotType spotType, bool requireExactSpot = false)
        {
            if (targetRoom == null) yield break;

            if (_currentRoom == targetRoom && _currentSpot != null && _currentSpot.Type == spotType)
            {
                UpdateStateFromSpot();
                yield break;
            }

            if (PathfindingManager.Instance == null)
            {
                Debug.LogError("[NonCombatUnit] PathfindingManager not found!");
                yield break;
            }

            SetState(NonCombatUnitState.Moving);

            if (!PathfindingManager.Instance.FindPath(_currentRoom, targetRoom, _roomPath, true))
            {
                Debug.Log($"[Unit] Путь не найден: {name} {_currentRoom?.name} -> {targetRoom.name}");
                UpdateStateFromSpot();
                yield break;
            }
            var spot = requireExactSpot
                ? RoomSpotSelector.SelectSpotStrict(targetRoom, spotType)
                : RoomSpotSelector.SelectSpot(targetRoom, spotType);

            if (requireExactSpot && spot == null)
            {
                UpdateStateFromSpot();
                yield break;
            }

            if (targetRoom == _currentRoom && spot == _currentSpot && spot != null)
            {
                UpdateStateFromSpot();
                yield break;
            }

            if (spot != null)
            {
                ReleaseCurrentSpot();
                spot.SetOccupied(true);
                _currentSpot = spot;
            }
            else if (_currentSpot != null && targetRoom != _currentRoom)
            {

                ReleaseCurrentSpot();
            }

            yield return StartCoroutine(MoveAlongPathAndTakeSpot(_roomPath, spot));

            UpdateStateFromSpot();
        }

        private IEnumerator MoveAlongPathAndTakeSpot(List<Room> path, RoomSpot finalSpot)
        {
            if (path == null || path.Count == 0) yield break;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];

                var transition = from.GetTransitionTo(to);
                if (transition == null)
                {

                    if (Vector3.Distance(transform.position, to.transform.position) > _approachTolerance)
                        yield return StartCoroutine(MoveToPosition(to.transform.position));

                    _currentRoom = to;
                    continue;
                }

                if (transition.Type == TransitionType.Elevator)
                {
                    Room elevatorStartRoom = from;
                    Room elevatorEndRoom = to;
                    RoomTransition lastTransition = transition;

                    while (i + 1 < path.Count - 1)
                    {
                        var nextFrom = path[i + 1];
                        var nextTo = path[i + 2];
                        var nextTrans = nextFrom.GetTransitionTo(nextTo);

                        if (nextTrans != null && nextTrans.Type == TransitionType.Elevator)
                        {
                            elevatorEndRoom = nextTo;
                            lastTransition = nextTrans;
                            i++;
                        }
                        else break;
                    }

                    var entry = transition.GetEntryFor(elevatorStartRoom);
                    if (entry != null && Mathf.Abs(transform.position.x - entry.position.x) > _approachTolerance)
                        yield return StartCoroutine(MoveToX(entry.position.x));

                    if (ElevatorController.Instance != null)
                    {

                        var finalExit = lastTransition.GetExitFor(elevatorEndRoom);
                        Vector3? targetExitPos = finalExit != null ? finalExit.position : (Vector3?)null;

                        yield return StartCoroutine(ElevatorController.Instance.UseElevator(transition, this, elevatorStartRoom, elevatorEndRoom, targetExitPos));
                    }
                    else
                    {

                        var exit = lastTransition.GetExitFor(elevatorEndRoom);
                        if (exit != null) transform.position = exit.position;
                    }

                    _currentRoom = elevatorEndRoom;
                }
                else
                {

                    _currentRoom = to;
                }
            }

            Vector3 targetPos = (finalSpot != null) ? finalSpot.transform.position : path[path.Count - 1].transform.position;
            if (Vector3.Distance(transform.position, targetPos) > _approachTolerance)
                yield return StartCoroutine(MoveToPosition(targetPos));

            _currentRoom = path[path.Count - 1];
        }

        private IEnumerator MoveToX(float targetX)
        {
            if (Mathf.Abs(transform.position.x - targetX) <= _approachTolerance)
            {
                transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
                yield break;
            }

            while (Mathf.Abs(transform.position.x - targetX) > _approachTolerance)
            {
                float dir = Mathf.Sign(targetX - transform.position.x);
                transform.position += Vector3.right * dir * _movementSpeed * Time.deltaTime;
                yield return null;
            }

            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        }

        private IEnumerator MoveToPosition(Vector3 target)
        {
            if (Vector3.Distance(transform.position, target) <= _approachTolerance)
            {
                transform.position = target;
                yield break;
            }

            while (Vector3.Distance(transform.position, target) > _approachTolerance)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, _movementSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }

        private void OnDrawGizmos()
        {
            if (_roomPath != null && _roomPath.Count > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < _roomPath.Count - 1; i++)
                {
                    if (_roomPath[i] != null && _roomPath[i + 1] != null)
                        Gizmos.DrawLine(_roomPath[i].transform.position, _roomPath[i + 1].transform.position);
                }
            }
        }
    }
}

