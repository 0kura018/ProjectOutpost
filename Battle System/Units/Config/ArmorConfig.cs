using UnityEngine;

namespace BattleSystem.Units.Config
{

    [CreateAssetMenu(menuName = "Battle/Units/Equipment/Armor")]
    public class ArmorConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public string ArmorName;
        public Sprite Icon;

        [Header("Stats Modifiers")]
        [Tooltip("����� � ������������� ��������")]
        public int MaxHealthBonus = 0;

        [Tooltip("�������� ����������� ����� (�������)")]
        public int DamageReduction = 0;

        [Tooltip("��������� �������� ������������ (1 = ��� ���������)")]
        public float MovementSpeedMultiplier = 1f;
    }
}
