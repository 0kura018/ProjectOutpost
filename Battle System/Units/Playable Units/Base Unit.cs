using BattleSystem.Units;
using UnityEngine;

namespace BattleSystem.Units.PlayableUnits
{
    public class BaseUnit : MainUnitProfile
    {
        private UnitStateMachine _unitStateMachine;

        public void Start()
        {
            _unitStateMachine = GetComponent<UnitStateMachine>();
        }
        public override void TakeDamage(int damage)
        {
            if(_unitStateMachine.currentState == _unitStateMachine.WalkTowardState)
            {
                _unitStateMachine.SwitchState(_unitStateMachine.IdleState);
                print("Unit interrupted while taking damage.");
            }
            _unitCurrentStats.TakeDamage(damage);
        }
        public override void Heal(int amount)
        {
            _unitCurrentStats.Heal(amount);
        }
        public override void Die()
        {
            _unitCurrentStats.Die();
            _unitCurrentStats.MainBattleManager.AliveCheck();
            Destroy(gameObject);
        }
        public override void Attack()
        {
            _unitCurrentStats.CurrentTarget.UnitCurrentStats.MainUnitProfile.TakeDamage(_unitCurrentStats.AttackDamage);
        }
        public override void UseSkill()
        {
            _unitCurrentStats.CurrentTarget.UnitCurrentStats.MainUnitProfile.TakeDamage(_unitCurrentStats.SkillDamage);
        }
        public override void ResetSkillCooldown()
        {

        }
    }
}
