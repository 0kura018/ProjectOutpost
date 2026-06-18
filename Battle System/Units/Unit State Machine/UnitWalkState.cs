using UnityEngine;
namespace BattleSystem.Units
{
    public class UnitWalkState : UnitBaseState
    {
        private Transform _target;
        private bool _isReturnPoint = false;
        private float _recheckInterval = 0.3f;
        private float _recheckTimer;

        public override void EnterState(UnitStateMachine unit)
        {
            _target = null;
            _isReturnPoint = false;
            _recheckTimer = 0f;

            if (unit.UnitCurrentStats.CanAttack)
            {
                unit.UnitCurrentStats.MainBattleManager.AssignNearestTarget(unit);
                if (unit.UnitCurrentStats.CurrentTarget != null)
                {
                    _target = unit.UnitCurrentStats.CurrentTarget.transform;
                    _isReturnPoint = false;
                    return;
                }
            }

            if (unit.UnitCurrentStats.UnitAffiliation == UnitAffiliation.Enemy)
            {

                _target = null;
                _isReturnPoint = false;
            }
            else
            {

                var returnPoint = unit.UnitCurrentStats.GetPersonalReturnPoint();
                if (returnPoint != null)
                {
                    _target = returnPoint.transform;
                    _isReturnPoint = true;
                }
            }
        }

        public override void UpdateState(UnitStateMachine unit)
        {

            _recheckTimer -= Time.deltaTime;
            if (_recheckTimer <= 0f)
            {
                _recheckTimer = _recheckInterval;

                if (unit.UnitCurrentStats.CanAttack)
                {
                    unit.UnitCurrentStats.MainBattleManager.AssignNearestTarget(unit);

                    if (unit.UnitCurrentStats.CurrentTarget != null)
                    {
                        float range = unit.UnitCurrentStats.AttackRange;
                        float distToTarget = ((Vector2)unit.transform.position -
                                        (Vector2)unit.UnitCurrentStats.CurrentTarget.transform.position).sqrMagnitude;

                        if (distToTarget <= range * range)
                        {

                            unit.SwitchState(unit.ReadyToAttackState);
                            return;
                        }
                        else
                        {

                            _target = unit.UnitCurrentStats.CurrentTarget.transform;
                            _isReturnPoint = false;
                        }
                    }
                }
            }

            if (unit.UnitCurrentStats.UnitAffiliation == UnitAffiliation.Enemy && _target == null)
            {
                MoveForward(unit, -1f);
                return;
            }

            if (unit.UnitCurrentStats.UnitAffiliation == UnitAffiliation.Ally && _target == null)
            {
                var returnPoint = unit.UnitCurrentStats.GetPersonalReturnPoint();
                if (returnPoint != null)
                {
                    _target = returnPoint.transform;
                    _isReturnPoint = true;
                }
                else
                {
                    unit.SwitchState(unit.IdleState);
                    return;
                }
            }

            if (_target == null)
            {
                unit.SwitchState(unit.IdleState);
                return;
            }

            Vector2 pos = unit.transform.position;
            Vector2 targetPos = _target.position;
            Vector2 toTarget = targetPos - pos;
            float sqrDist = toTarget.sqrMagnitude;

            if (_isReturnPoint)
            {
                float arriveThreshold = 0.25f;
                if (sqrDist <= arriveThreshold * arriveThreshold)
                {
                    var visual = unit.VisualObject.transform;
                    Vector3 scale = visual.localScale;
                    scale.x = Mathf.Abs(scale.x);
                    visual.localScale = scale;

                    unit.SwitchState(unit.IdleState);
                    return;
                }

                MoveToTarget(unit, toTarget);
                return;
            }

            float attackRange = unit.UnitCurrentStats.AttackRange;
            if (sqrDist <= attackRange * attackRange && unit.UnitCurrentStats.CanAttack)
            {
                unit.SwitchState(unit.ReadyToAttackState);
                return;
            }

            MoveToTarget(unit, toTarget);
        }

        private void MoveForward(UnitStateMachine unit, float direction)
        {
            float speed = unit.UnitCurrentStats.MovementSpeed;
            unit.transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);
            FlipHorizontally(unit.VisualObject.transform, direction);
        }

        private void MoveToTarget(UnitStateMachine unit, Vector2 toTarget)
        {
            Vector2 dir = toTarget.normalized;
            float speed = unit.UnitCurrentStats.MovementSpeed;

            unit.transform.position += (Vector3)(dir * speed * Time.deltaTime);
            FlipHorizontally(unit.VisualObject.transform, dir.x);
        }

        private void FlipHorizontally(Transform unitTransform, float dirX)
        {
            if (Mathf.Abs(dirX) < 0.01f)
                return;

            Vector3 scale = unitTransform.localScale;
            scale.x = Mathf.Sign(dirX) * Mathf.Abs(scale.x);
            unitTransform.localScale = scale;
        }
    }
}