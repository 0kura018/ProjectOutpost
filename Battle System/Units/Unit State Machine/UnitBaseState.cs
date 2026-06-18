using UnityEngine;

namespace BattleSystem.Units
{
    public abstract class UnitBaseState
    {
        public abstract void EnterState(UnitStateMachine unit);
        public abstract void UpdateState(UnitStateMachine unit);
    }
}
