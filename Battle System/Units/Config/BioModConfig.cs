using UnityEngine;

namespace BattleSystem.Units.Config
{

    [CreateAssetMenu(menuName = "Battle/Units/Equipment/BioMod")]
    public class BioModConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public string ModName;
        public Sprite Icon;
        [TextArea] public string Description;

        [Header("Stats Modifiers")]
        [Tooltip("����� � ������������� ��������")]
        public int MaxHealthBonus = 0;

        [Tooltip("����� � ����� �����")]
        public int AttackDamageBonus = 0;

        [Tooltip("����� � ����� ������")]
        public int SkillDamageBonus = 0;

        [Tooltip("��������� �������� ������ (1 = ��� ���������, ������ = �������)")]
        public float SkillCooldownMultiplier = 1f;

    }
}
