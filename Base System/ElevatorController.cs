using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BuildingSystem;

namespace BaseSystem
{

    public class ElevatorController : MonoBehaviour
    {
        public static ElevatorController Instance { get; private set; }

        [Header("Elevator Settings")]
        [SerializeField] private float _elevatorSpeed = 0f;
        [SerializeField] private float _waitSpeed = 0f;
        [SerializeField] private float _horizontalOffset = 0f;

        private class Request
        {
            public RoomTransition Transition;
            public NonCombatUnit Unit;
            public Room From;
            public Room To;
            public Vector3? TargetExitPos;
            public bool Done;
        }

        private Queue<Request> _procQueue = new();
        private bool _isProcessing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        public IEnumerator UseElevator(RoomTransition transition, NonCombatUnit unit, Room from, Room to, Vector3? targetExitPos = null)
        {
            if (transition == null) yield break;
            var r = new Request { Transition = transition, Unit = unit, From = from, To = to, TargetExitPos = targetExitPos, Done = false };
            _procQueue.Enqueue(r);

            if (!_isProcessing) StartCoroutine(ProcessQueue());

            while (!r.Done) yield return null;
        }

        private IEnumerator ProcessQueue()
        {
            _isProcessing = true;
            while (_procQueue.Count > 0)
            {
                var r = _procQueue.Dequeue();

                if (r.Unit == null || r.Transition == null)
                {
                    r.Done = true;
                    continue;
                }

                var entry = r.Transition.GetEntryFor(r.From);
                var exit = r.Transition.GetExitFor(r.To);

                Vector3 startPos = entry != null ? entry.position : r.From.transform.position;
                Vector3 endPos = exit != null ? exit.position : r.To.transform.position;

                if (r.TargetExitPos.HasValue)
                {
                    endPos = r.TargetExitPos.Value;
                }

                if (!r.TargetExitPos.HasValue)
                {
                    startPos.x += _horizontalOffset;
                    endPos.x += _horizontalOffset;
                }
                else
                {

                    startPos.x += _horizontalOffset;
                }

                r.Unit.transform.position = startPos;

                if (_waitSpeed > 0)
                {
                    yield return new WaitForSeconds(1f / _waitSpeed);
                }

                float distance = Vector3.Distance(startPos, endPos);
                float duration = _elevatorSpeed > 0 ? (distance / _elevatorSpeed) : r.Transition.ElevatorTravelDuration;
                duration = Mathf.Max(0.01f, duration);

                float t = 0f;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Clamp01(t / duration);

                    if (r.Unit != null)
                        r.Unit.transform.position = Vector3.Lerp(startPos, endPos, alpha);

                    yield return null;
                }

                if (r.Unit != null) r.Unit.transform.position = endPos;

                r.Done = true;
            }
            _isProcessing = false;
        }
    }
}
