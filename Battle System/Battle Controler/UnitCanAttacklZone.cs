using BattleSystem.Units;
using UnityEngine;

namespace BattleSystem.BattleControler
{
    public class UnitCanAttacklZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent<UnitStateMachine>(out var unitStateMachine))
            {
                unitStateMachine.UnitCurrentStats.CanAttack = true;
            }
        }
    }
}
