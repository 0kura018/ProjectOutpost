using UnityEngine;
namespace BattleSystem.Units
{
    public class UnitAttackState : UnitBaseState
    {
        public UnitCurrentStats UnitCurrentStats;
        public override void EnterState(UnitStateMachine unit)
        {
            unit.UnitCurrentStats.MainUnitProfile.Attack();
        }
        public override void UpdateState(UnitStateMachine unit)
        {
            unit.SwitchState(unit.ReadyToAttackState);
        }
    }
}