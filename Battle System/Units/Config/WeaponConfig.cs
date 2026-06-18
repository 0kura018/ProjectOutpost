using UnityEngine;

namespace BattleSystem.Units.Config
{

    [CreateAssetMenu(menuName = "Battle/Units/Equipment/Weapon")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public string WeaponName;
        public Sprite Icon;

        [Header("Position Restriction")]
        [Tooltip("��� ����� ������� ������������� ������")]
        public WeaponPosition AllowedPosition = WeaponPosition.Any;

        [Header("Stats Modifiers")]
        [Tooltip("����� � ����� �����")]
        public int AttackDamageBonus = 0;

        [Tooltip("����� � ����� ������")]
        public int SkillDamageBonus = 0;

        [Tooltip("��������� �������� ����� (1 = ��� ���������)")]
        public float AttackSpeedMultiplier = 1f;

        [Tooltip("����� � ��������� �����")]
        public float AttackRangeBonus = 0f;

        public bool IsValidForPosition(UnitBattlePosition position)
        {
            return AllowedPosition switch
            {
                WeaponPosition.Any => true,
                WeaponPosition.Vanguard => position == UnitBattlePosition.Avanguard,
                WeaponPosition.Rearguard => position == UnitBattlePosition.Arierguard,
                _ => true
            };
        }
    }

    public enum WeaponPosition
    {
        Any,
        Vanguard,
        Rearguard
    }
}
