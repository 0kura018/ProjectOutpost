using UnityEngine;

namespace BattleSystem.Units
{
    public class UnitReadyToAttackState : UnitBaseState
    {
        private UnitCurrentStats _unitCurrentStats;

        private float _attackCooldownTime;
        private Transform _target;

        private float _retargetTimer;
        private const float RETARGET_INTERVAL = 0.5f;

        public override void EnterState(UnitStateMachine unit)
        {
            _unitCurrentStats = unit.UnitCurrentStats;
            _retargetTimer = RETARGET_INTERVAL;

            if (_unitCurrentStats.CurrentTarget == null)
            {
                _target = null;
                return;
            }

            _target = unit.UnitCurrentStats.CurrentTarget.transform;

            if (_attackCooldownTime <= 0f)
            {
                _attackCooldownTime = _unitCurrentStats.AttackCooldownTime;
            }
        }
        public override void UpdateState(UnitStateMachine unit)
        {
            if (!_unitCurrentStats.CanAttack) { unit.SwitchState(unit.WalkState); return; }
            if (_target == null)
            {

                unit.SwitchState(unit.IdleState);
                return;
            }

            _retargetTimer -= Time.deltaTime;
            if (_retargetTimer <= 0f)
            {
                _retargetTimer = RETARGET_INTERVAL;

                var previousTarget = _unitCurrentStats.CurrentTarget;
                _unitCurrentStats.MainBattleManager.AssignNearestTarget(unit);

                if (_unitCurrentStats.CurrentTarget != previousTarget && _unitCurrentStats.CurrentTarget != null)
                {
                    _target = _unitCurrentStats.CurrentTarget.transform;
                }
                else if (_unitCurrentStats.CurrentTarget == null)
                {
                    _target = null;
                    unit.SwitchState(unit.IdleState);
                    return;
                }
            }

            Vector2 pos = unit.transform.position;
            Vector2 targetPos = _target.position;

            Vector2 toTarget = targetPos - pos;
            float sqrDist = toTarget.sqrMagnitude;

            float attackRange = unit.UnitCurrentStats.AttackRange;
            if (sqrDist > attackRange * attackRange)
            {
                unit.SwitchState(unit.WalkState);
                return;
            }

            _attackCooldownTime -= Time.deltaTime;

            if (unit.CanUseSkill)
            {
                unit.SwitchState(unit.UseSkillState);
                _attackCooldownTime = _unitCurrentStats.AttackCooldownTime;
            }
            else if (_attackCooldownTime <= 0f)
            {
                unit.SwitchState(unit.AttackState);
                _attackCooldownTime = _unitCurrentStats.AttackCooldownTime;
            }
        }
    }
}
