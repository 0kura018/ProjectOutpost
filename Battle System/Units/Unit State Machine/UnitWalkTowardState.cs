using UnityEngine;

namespace BattleSystem.Units
{
    public class UnitWalkTowardState : UnitBaseState
    {
        public UnitCurrentStats UnitCurrentStats;

        private Vector2 _startPos;
        private float _desiredDistance = 15f;
        public override void EnterState(UnitStateMachine unit)
        {
            UnitCurrentStats = unit.UnitCurrentStats;
            _startPos = unit.transform.position;
        }
        public override void UpdateState(UnitStateMachine unit)
        {

            float sqrDistanceTravelled = ((Vector2)unit.transform.position - _startPos).sqrMagnitude;

            if (sqrDistanceTravelled >= _desiredDistance * _desiredDistance || UnitCurrentStats.CurrentTarget != null)
            {
                unit.SwitchState(unit.IdleState);
                return;
            }

            Vector2 forward = unit.transform.right;

            float speed = UnitCurrentStats.MovementSpeed;
            unit.transform.position += (Vector3)(forward * speed * Time.deltaTime);

            FlipHorizontally(unit.VisualObject.transform, forward.x);

            Vector2 currentPos = unit.transform.position;
            float distanceTravelled = Vector2.Distance(currentPos, _startPos);
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
