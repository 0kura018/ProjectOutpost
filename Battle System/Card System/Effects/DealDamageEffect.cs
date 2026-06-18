using UnityEngine;
using BattleSystem.Units;

namespace BattleSystem.CardSystem.Effects
{
    [CreateAssetMenu(menuName = "Battle/Cards/Effects/Deal Damage")]
    public class DealDamageEffect : CardEffect
    {
        [Header("Damage Settings")]
        public int BaseDamage = 10;
        public bool IgnoreArmor = false;

        public override void Execute(UnitStateMachine target, UnitStateMachine caster = null, float potency = 1f)
        {
            if (target == null) return;

            SpawnVisualEffectOnTarget(target);

            int finalDamage = Mathf.RoundToInt(BaseDamage * potency);
            target.UnitCurrentStats.MainUnitProfile.TakeDamage(finalDamage);

            Debug.Log($"[Card Effect] Dealt {finalDamage} damage to {target.name}");
        }
    }
}
