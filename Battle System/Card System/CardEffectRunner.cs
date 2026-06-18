using System.Collections;
using UnityEngine;
using BattleSystem.Units;

namespace BattleSystem.CardSystem
{

    public class CardEffectRunner : MonoBehaviour
    {
        public static CardEffectRunner Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RunEffect(CardEffect effect, UnitStateMachine target, UnitStateMachine caster = null, float potency = 1f)
        {
            if (effect == null) return;

            if (effect.Delay > 0f)
            {
                StartCoroutine(DelayedExecute(effect, target, caster, potency));
            }
            else
            {
                ExecuteEffect(effect, target, caster, potency);
            }
        }

        public void RunEffectAtPoint(CardEffect effect, Vector3 worldPoint, float potency = 1f)
        {
            if (effect == null) return;

            if (effect.Delay > 0f)
            {
                StartCoroutine(DelayedExecuteAtPoint(effect, worldPoint, potency));
            }
            else
            {
                effect.ExecuteAtPoint(worldPoint, potency);
            }
        }

        private IEnumerator DelayedExecute(CardEffect effect, UnitStateMachine target, UnitStateMachine caster, float potency)
        {
            yield return new WaitForSeconds(effect.Delay);

            if (target != null)
            {
                ExecuteEffect(effect, target, caster, potency);
            }
        }

        private IEnumerator DelayedExecuteAtPoint(CardEffect effect, Vector3 worldPoint, float potency)
        {
            yield return new WaitForSeconds(effect.Delay);
            effect.ExecuteAtPoint(worldPoint, potency);
        }

        private void ExecuteEffect(CardEffect effect, UnitStateMachine target, UnitStateMachine caster, float potency)
        {
            effect.Execute(target, caster, potency);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}