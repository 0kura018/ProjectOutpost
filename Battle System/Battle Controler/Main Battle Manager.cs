using BattleSystem.Units;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.BattleControler
{
    public class MainBattleManager : MonoBehaviour
    {
        public List<UnitStateMachine> Alies = new();
        public List<UnitStateMachine> Enemies = new();

        [Header("Targeting Setting")]
        [Tooltip("���� ������ ������ (� ��������) � ���� � �������� ����� ���� ����� ������������")]
        [SerializeField] private float _forwardAngleDeg = 25f;

        [Tooltip("���������� ������� �� ������ (������� �������) ��� �������� ��������")]
        [SerializeField] private float _verticalTolerance = 1.5f;

        [Tooltip("���� true � ���������� AttackRange ����� ��� ��������� ������, ����� ���������� ForwardRangeOverride")]
        [SerializeField] private bool _useAttackRange = true;

        [Tooltip("��������� ������ (���� _useAttackRange == false)")]
        [SerializeField] private float _forwardRangeOverride = 5f;

        [Header("Debug / Visualization")]
        [Tooltip("�������� ������ � ����� (Gizmos)")]
        [SerializeField] private bool _drawGizmos = true;

        [Tooltip("���� true � �������� ����� ��� ���� ������; ����� �������� ������ ��� ���, ��� � ������ (Alies + Enemies)")]
        [SerializeField] private bool _drawForAll = false;

        [Tooltip("���������� ��������� ��� ��������� ���� ������")]
        [SerializeField] private int _coneSegments = 20;

        [Tooltip("���� ������ (Gizmos)")]
        [SerializeField] private Color _coneColor = new Color(1f, 0.3f, 0.3f, 0.8f);

        [Tooltip("������ �������")]
        [SerializeField] private float _targetingRadius = 25f;

        [Header("Target Switching")]
        [Tooltip("����� ��� ����� ����: ���� ��������� ���� ����� ��������� � N ���, ������������� �� ���������� (0.5 = ��������� ������ ���� � 2 ���� �����)")]
        [SerializeField] private float _nearestOverrideFactor = 0.6f;

        [Tooltip("����������� ������� ���������� ��� ����� ���� (������������� ��������)")]
        [SerializeField] private float _targetSwitchMinDiff = 0.5f;

        [Header("Ally Return / Trigger Settings")]
        [Tooltip("��������� ����� �������� ��� ��������� (start point)")]
        [SerializeField] private Transform _allyReturnPoint;

        [Tooltip("�������� �� ����� �������� (start point + offset)")]
        [SerializeField] private Vector3 _allyReturnOffset = Vector3.zero;

        private List<UnitStateMachine> _enemiesInTrigger = new();
        private GameObject _allyReturnGoTo;

        private void Start()
        {

            foreach (var unit in FindObjectsByType<UnitStateMachine>(sortMode: FindObjectsSortMode.None))
            {

                if (unit.UnitCurrentStats.MainBattleManager != null) continue;

                unit.UnitCurrentStats.MainBattleManager = this;

                var affiliation = unit.UnitCurrentStats.MainUnitProfile != null
                    ? unit.UnitCurrentStats.MainUnitProfile.UnitAffiliation
                    : unit.UnitCurrentStats.UnitAffiliation;

                if (affiliation == UnitAffiliation.Ally)
                {
                    if (!Alies.Contains(unit))
                    {
                        Alies.Add(unit);
                        unit.UnitCurrentStats.SaveSpawnPosition();
                    }
                }
                else
                {
                    if (!Enemies.Contains(unit))
                        Enemies.Add(unit);
                }
            }
        }

        public GameObject GetAllyReturnGoTo()
        {

            if (_allyReturnPoint != null)
            {
                if (_allyReturnGoTo == null)
                    _allyReturnGoTo = new GameObject("AllyReturnGoTo");

                _allyReturnGoTo.transform.position = _allyReturnPoint.position + _allyReturnOffset;
                _allyReturnGoTo.transform.rotation = _allyReturnPoint.rotation;
                return _allyReturnGoTo;
            }

            var spawner = FindAnyObjectByType<BattleSpawner>();
            Vector3 basePos = (spawner != null) ? spawner.GetFirstSpawnPointPosition() : transform.position;

            if (_allyReturnGoTo == null)
                _allyReturnGoTo = new GameObject("AllyReturnGoTo");

            Vector3 computed = basePos + new Vector3(_allyReturnOffset.x, _allyReturnOffset.y, _allyReturnOffset.z);
            _allyReturnGoTo.transform.position = computed;
            return _allyReturnGoTo;
        }

        private void OnTriggerEnter(Collider other)
        {
            var unit = other.GetComponentInParent<UnitStateMachine>();
            if (unit == null) return;

            var affiliation = unit.UnitCurrentStats.MainUnitProfile != null
                ? unit.UnitCurrentStats.MainUnitProfile.UnitAffiliation
                : unit.UnitCurrentStats.UnitAffiliation;

            if (affiliation != UnitAffiliation.Ally)
            {
                if (!_enemiesInTrigger.Contains(unit)) _enemiesInTrigger.Add(unit);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var unit = other.GetComponentInParent<UnitStateMachine>();
            if (unit == null) return;
            if (_enemiesInTrigger.Contains(unit)) _enemiesInTrigger.Remove(unit);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var unit = other.GetComponentInParent<UnitStateMachine>();
            if (unit == null) return;

            var affiliation = unit.UnitCurrentStats.MainUnitProfile != null
                ? unit.UnitCurrentStats.MainUnitProfile.UnitAffiliation
                : unit.UnitCurrentStats.UnitAffiliation;

            if (affiliation != UnitAffiliation.Ally)
            {
                if (!_enemiesInTrigger.Contains(unit))
                {
                    _enemiesInTrigger.Add(unit);
                    Debug.Log($"[Trigger] Enemy entered: {unit.gameObject.name}. Total enemies in trigger: {_enemiesInTrigger.Count}");

                    ReassignAllyTargets();
                }
            }
        }

        private void ReassignAllyTargets()
        {
            for (int i = 0; i < Alies.Count; i++)
            {
                var ally = Alies[i];
                if (ally == null || !ally.UnitCurrentStats.IsAlive) continue;

                var previousTarget = ally.UnitCurrentStats.CurrentTarget;

                AssignNearestTarget(ally);

                var newTarget = ally.UnitCurrentStats.CurrentTarget;

                if (newTarget != null && newTarget != previousTarget)
                {
                    if (ally.currentState == ally.IdleState || ally.currentState == ally.WalkState)
                    {
                        ally.SwitchState(ally.ReadyToAttackState);
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var unit = other.GetComponentInParent<UnitStateMachine>();
            if (unit == null) return;
            if (_enemiesInTrigger.Contains(unit)) _enemiesInTrigger.Remove(unit);
        }

        public void RegisterAlly(UnitStateMachine unit)
        {
            if (unit == null) return;
            if (Alies.Contains(unit)) return;

            Alies.Add(unit);
            unit.UnitCurrentStats.MainBattleManager = this;
            unit.UnitCurrentStats.SaveSpawnPosition();
        }

        public void RegisterEnemy(UnitStateMachine unit)
        {
            if (unit == null) return;
            if (Enemies.Contains(unit)) return;

            Enemies.Add(unit);
            unit.UnitCurrentStats.MainBattleManager = this;
        }

        private int _totalEnemiesKilled;
        private int _totalAlliesLost;

        public void AliveCheck()
        {
            int prevAllies = Alies.Count;
            int prevEnemies = Enemies.Count;

            Alies.RemoveAll(unit => !unit.UnitCurrentStats.IsAlive);
            Enemies.RemoveAll(unit => !unit.UnitCurrentStats.IsAlive);
            _enemiesInTrigger.RemoveAll(unit => unit == null || !unit.UnitCurrentStats.IsAlive);

            _totalAlliesLost += prevAllies - Alies.Count;
            _totalEnemiesKilled += prevEnemies - Enemies.Count;

            AssignNearestTargetsForAll();

            if (Alies.Count == 0)
            {
                Debug.Log("Defeat");
                EndBattleWithResult(false);
            }

            else if (Enemies.Count == 0 && (BattleSpawner.Instance == null || BattleSpawner.Instance.AllWavesCompleted))
            {
                Debug.Log("Victory");
                EndBattleWithResult(true);
            }
        }

        private void EndBattleWithResult(bool victory)
        {
            var result = new BattleResult(victory)
            {
                EnemiesKilled = _totalEnemiesKilled,
                AlliesLost = _totalAlliesLost,
                BattleDuration = Time.timeSinceLevelLoad
            };

            if (BattleSceneManager.Instance != null)
            {
                BattleSceneManager.Instance.EndBattle(result);
            }
        }

        public UnitStateMachine GetNearestTarget(UnitStateMachine requester)
        {
            if (requester == null) return null;

            var requesterAffil = requester.UnitCurrentStats.MainUnitProfile != null
                ? requester.UnitCurrentStats.MainUnitProfile.UnitAffiliation
                : requester.UnitCurrentStats.UnitAffiliation;

            var candidates = requesterAffil == UnitAffiliation.Ally ? _enemiesInTrigger : Alies;
            if (candidates == null || candidates.Count == 0) return null;

            Vector2 myPos = requester.transform.position;
            Vector2 myForward = requester.transform.right;

            float attackRange = requester.UnitCurrentStats.AttackRange;
            float coneRange = attackRange + (_useAttackRange ? attackRange : _forwardRangeOverride);

            var currentTarget = requester.UnitCurrentStats.CurrentTarget;

            UnitStateMachine bestConeTarget = null;
            float bestConeDist = float.MaxValue;

            UnitStateMachine bestNearestTarget = null;
            float bestNearestDist = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                var cand = candidates[i];
                if (cand == null || !cand.UnitCurrentStats.IsAlive) continue;
                if (cand == requester) continue;

                Vector2 candPos = cand.transform.position;
                Vector2 toTarget = candPos - myPos;
                float dist = toTarget.magnitude;

                if (dist < bestNearestDist)
                {
                    bestNearestDist = dist;
                    bestNearestTarget = cand;
                }

                if (dist <= coneRange && dist > 0.01f)
                {

                    if (Mathf.Abs(toTarget.y) <= _verticalTolerance)
                    {
                        Vector2 dirNorm = toTarget / dist;
                        float angle = Vector2.Angle(myForward, dirNorm);

                        if (angle <= _forwardAngleDeg)
                        {

                            if (dist < bestConeDist)
                            {
                                bestConeDist = dist;
                                bestConeTarget = cand;
                            }
                        }
                    }
                }
            }

            if (bestConeTarget != null)
            {

                if (bestNearestTarget != null && bestNearestTarget != bestConeTarget)
                {
                    if (bestNearestDist < bestConeDist * _nearestOverrideFactor)
                    {

                        return bestNearestTarget;
                    }
                }

                if (currentTarget != null && currentTarget.UnitCurrentStats.IsAlive && currentTarget != bestConeTarget)
                {
                    float currentDist = Vector2.Distance(myPos, (Vector2)currentTarget.transform.position);
                    if (Mathf.Abs(currentDist - bestConeDist) < _targetSwitchMinDiff)
                    {
                        return currentTarget;
                    }
                }

                return bestConeTarget;
            }

            return bestNearestTarget;
        }

        public void AssignNearestTarget(UnitStateMachine requester)
        {
            if (requester == null) return;

            var target = GetNearestTarget(requester);
            requester.UnitCurrentStats.CurrentTarget = target;
            if (target != null)
            {
                requester.UnitCurrentStats.CurrentGoTo = target.VisualObject;
            }
            else if (requester.UnitCurrentStats.MainUnitProfile != null && requester.UnitCurrentStats.MainUnitProfile.UnitAffiliation == UnitAffiliation.Ally)
            {

                requester.UnitCurrentStats.CurrentGoTo = requester.UnitCurrentStats.GetPersonalReturnPoint();
            }
        }

        public void AssignNearestTargetsForAll()
        {
            for (int i = 0; i < Alies.Count; i++)
            {
                var unit = Alies[i];
                if (unit == null) continue;
                AssignNearestTarget(unit);
            }

            for (int i = 0; i < Enemies.Count; i++)
            {
                var unit = Enemies[i];
                if (unit == null) continue;
                AssignNearestTarget(unit);
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            Gizmos.color = _coneColor;

            var drawUnits = new List<UnitStateMachine>();
            if (!_drawForAll)
            {
                drawUnits.AddRange(Alies);
                drawUnits.AddRange(Enemies);
            }
            else
            {
                drawUnits.AddRange(FindObjectsByType<UnitStateMachine>(sortMode: FindObjectsSortMode.None));
            }

            foreach (var u in drawUnits)
            {
                if (u == null) continue;
                float attackRange = u.UnitCurrentStats.AttackRange;
                float coneRange = attackRange + (_useAttackRange ? attackRange : _forwardRangeOverride);
                DrawConeGizmo2D(u.transform.position, u.transform.right, _forwardAngleDeg, coneRange, _coneSegments);
            }
        }

        private static void DrawConeGizmo2D(Vector3 origin, Vector3 forward, float angleDeg, float range, int segments)
        {
            if (range <= 0f) range = 0.01f;

            Vector2 forward2D = new Vector2(forward.x, forward.y).normalized;
            if (forward2D.sqrMagnitude < 0.0001f) forward2D = Vector2.right;

            float baseAngle = Mathf.Atan2(forward2D.y, forward2D.x) * Mathf.Rad2Deg;
            float halfAngle = angleDeg;

            int segs = Mathf.Max(segments, 3);
            float step = (halfAngle * 2f) / segs;

            float leftAngle = (baseAngle - halfAngle) * Mathf.Deg2Rad;
            Vector3 leftPoint = origin + new Vector3(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle), 0f) * range;
            Gizmos.DrawLine(origin, leftPoint);

            float rightAngle = (baseAngle + halfAngle) * Mathf.Deg2Rad;
            Vector3 rightPoint = origin + new Vector3(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle), 0f) * range;
            Gizmos.DrawLine(origin, rightPoint);

            Vector3 prevPoint = leftPoint;
            for (int i = 1; i <= segs; i++)
            {
                float a = (baseAngle - halfAngle + step * i) * Mathf.Deg2Rad;
                Vector3 point = origin + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * range;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }
        public void SetWorldTime(float timeSpeed)
        {
            Time.timeScale = timeSpeed;
        }
    }
}
