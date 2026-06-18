using UnityEngine;
namespace BattleSystem.Units
{
    public class UnitIdleState : UnitBaseState
    {
        private UnitCurrentStats _unitCurrentStats;
        private float _recheckInterval = 0.5f;
        private float _recheckTimer;
        private bool _isAlly;
        private const float RETURN_POINT_THRESHOLD = 0.5f;

        public override void EnterState(UnitStateMachine unit)
        {
            _unitCurrentStats = unit.UnitCurrentStats;
            _recheckTimer = 0f;

            var affiliation = _unitCurrentStats.MainUnitProfile != null
                ? _unitCurrentStats.MainUnitProfile.UnitAffiliation
                : _unitCurrentStats.UnitAffiliation;
            _isAlly = affiliation == UnitAffiliation.Ally;

            if (_unitCurrentStats.CanAttack)
            {
                _unitCurrentStats.MainBattleManager.AssignNearestTarget(unit);

                if (_unitCurrentStats.CurrentTarget != null)
                {
                    unit.SwitchState(unit.ReadyToAttackState);
                    return;
                }
            }

            if (!_isAlly)
            {
                unit.SwitchState(unit.WalkState);
                return;
            }

            var returnPoint = _unitCurrentStats.GetPersonalReturnPoint();
            if (returnPoint != null)
            {
                float distToReturn = Vector2.Distance(
                    unit.transform.position,
                    returnPoint.transform.position
                );

                if (distToReturn > RETURN_POINT_THRESHOLD)
                {
                    unit.SwitchState(unit.WalkState);
                    return;
                }
            }

            var visual = unit.VisualObject.transform;
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x);
            visual.localScale = scale;
        }

        public override void UpdateState(UnitStateMachine unit)
        {
            _recheckTimer -= Time.deltaTime;
            if (_recheckTimer <= 0f)
            {
                _recheckTimer = _recheckInterval;

                if (_unitCurrentStats.CanAttack)
                {
                    _unitCurrentStats.MainBattleManager.AssignNearestTarget(unit);

                    if (_unitCurrentStats.CurrentTarget != null)
                    {
                        unit.SwitchState(unit.ReadyToAttackState);
                        return;
                    }
                }

                if (!_isAlly)
                {
                    unit.SwitchState(unit.WalkState);
                    return;
                }

                var returnPoint = _unitCurrentStats.GetPersonalReturnPoint();
                if (returnPoint != null)
                {
                    float distToReturn = Vector2.Distance(
                        unit.transform.position,
                        returnPoint.transform.position
                    );

                    if (distToReturn > RETURN_POINT_THRESHOLD)
                    {
                        unit.SwitchState(unit.WalkState);
                    }
                }
            }
        }
    }
}