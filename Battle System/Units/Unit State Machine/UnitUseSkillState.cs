using UnityEngine;
namespace BattleSystem.Units
{
    public class UnitUseSkillState : UnitBaseState
    {
        public UnitCurrentStats UnitCurrentStats;
        public override void EnterState(UnitStateMachine unit)
        {
            unit.UnitCurrentStats.MainUnitProfile.UseSkill();
            unit.ResetSkillCooldown();
        }
        public override void UpdateState(UnitStateMachine unit)
        {
            unit.SwitchState(unit.ReadyToAttackState);
        }
    }
}