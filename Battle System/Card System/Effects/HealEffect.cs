using UnityEngine;
using BattleSystem.Units;

namespace BattleSystem.CardSystem.Effects
{
    [CreateAssetMenu(menuName = "Battle/Cards/Effects/Heal")]
    public class HealEffect : CardEffect
    {
        [Header("Heal Settings")]
        public int BaseHeal = 15;

        public override void Execute(UnitStateMachine target, UnitStateMachine caster = null, float potency = 1f)
        {
            if (target == null) return;

            SpawnVisualEffectOnTarget(target);

            int finalHeal = Mathf.RoundToInt(BaseHeal * potency);
            target.UnitCurrentStats.Heal(finalHeal);

            Debug.Log($"[Card Effect] Healed {target.name} for {finalHeal}");
        }
    }
}
