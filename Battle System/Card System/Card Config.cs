using System.Collections.Generic;
using UnityEngine;
using BattleSystem.Units;

namespace BattleSystem.CardSystem
{
    [CreateAssetMenu(menuName = "Battle/Cards/Card Config")]
    public class CardConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public int CardID;
        public string CardName;
        [TextArea] public string Description;
        public Sprite CardImage;

        [Header("Cost")]
        public int EnergyCost;

        [Header("Targeting")]
        public CardTargetType TargetType;

        [Header("AOE Settings")]
        [Tooltip("������ AOE ������� (��� AOE ����)")]
        public float AOERadius = 3f;

        [Header("Effects")]
        [Tooltip("������ ��������, ������� ����������� ��� ������������� �����")]
        public List<CardEffect> Effects = new();

        [Header("Potency")]
        [Tooltip("��������� ���� �������� �����")]
        public float Potency = 1f;

        public void ApplyEffects(UnitStateMachine target, UnitStateMachine caster = null)
        {
            if (Effects == null || Effects.Count == 0)
            {
                Debug.LogWarning($"[CardConfig] {CardName}: No effects to apply!");
                return;
            }

            foreach (var effect in Effects)
            {
                if (effect == null) continue;

                if (CardEffectRunner.Instance != null)
                {
                    CardEffectRunner.Instance.RunEffect(effect, target, caster, Potency);
                }
                else
                {

                    Debug.LogWarning("[CardConfig] CardEffectRunner.Instance is null! Executing directly.");
                    effect.Execute(target, caster, Potency);
                }
            }
        }

        public void ApplyEffectsAtPoint(Vector3 worldPoint)
        {
            if (Effects == null || Effects.Count == 0)
            {
                Debug.LogWarning($"[CardConfig] {CardName}: No effects to apply at point!");
                return;
            }

            foreach (var effect in Effects)
            {
                if (effect == null) continue;

                if (CardEffectRunner.Instance != null)
                {
                    CardEffectRunner.Instance.RunEffectAtPoint(effect, worldPoint, Potency);
                }
                else
                {
                    Debug.LogWarning("[CardConfig] CardEffectRunner.Instance is null! Executing directly.");
                    effect.ExecuteAtPoint(worldPoint, Potency);
                }
            }
        }
    }

    public enum CardTargetType
    {
        Unit_All,
        Unit_Enemy,
        Unit_Ally,
        AOE_All,
        AOE_Enemy,
        AOE_Ally,
        Self,
        Card
    }
}