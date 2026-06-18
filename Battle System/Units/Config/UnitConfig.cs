using UnityEngine;
using BattleSystem.CardSystem;

namespace BattleSystem.Units.Config
{

    [CreateAssetMenu(menuName = "Battle/Units/Unit Config")]
    public class UnitConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public string UnitName;
        public Sprite Portrait;
        [TextArea] public string Description;

        [Header("Prefab")]
        [Tooltip("������ ����� (������ ����� MainUnitProfile)")]
        public GameObject UnitPrefab;

        [Header("Unit Type")]
        public UnitAffiliation DefaultAffiliation = UnitAffiliation.Ally;
        public UnitBattlePosition BattlePosition = UnitBattlePosition.Avanguard;

        [Header("Base Stats")]
        public int MaxHealth = 100;
        public int AttackDamage = 10;
        public int SkillDamage = 15;

        [Header("Timing")]
        public float AttackCooldown = 1f;
        public float SkillCooldown = 5f;

        [Header("Movement & Range")]
        public float MovementSpeed = 3f;
        public float AttackRange = 1.5f;
        public float SkillRange = 3f;

        [Header("UI")]
        public float HealthBarOffsetY = 1.5f;

        [Header("Equipment Slots")]
        [Tooltip("������ �����")]
        public WeaponConfig Weapon;

        [Tooltip("����� �����")]
        public ArmorConfig Armor;

        [Tooltip("�������������� �����")]
        public BioModConfig BioMod;

        [Header("Personal Card")]
        [Tooltip("������ ����� ����� (���� �������, ����� ����������� � ������)")]
        public CardConfig PersonalCard;

        [Tooltip("���������� ����� ������ �����")]
        public int PersonalCardCopies = 2;

        public UnitFinalStats CalculateFinalStats()
        {
            var stats = new UnitFinalStats
            {
                MaxHealth = MaxHealth,
                AttackDamage = AttackDamage,
                SkillDamage = SkillDamage,
                AttackCooldown = AttackCooldown,
                SkillCooldown = SkillCooldown,
                MovementSpeed = MovementSpeed,
                AttackRange = AttackRange,
                SkillRange = SkillRange
            };

            if (Weapon != null && Weapon.IsValidForPosition(BattlePosition))
            {
                stats.AttackDamage += Weapon.AttackDamageBonus;
                stats.SkillDamage += Weapon.SkillDamageBonus;
                stats.AttackCooldown *= Weapon.AttackSpeedMultiplier;
                stats.AttackRange += Weapon.AttackRangeBonus;
            }
            else if (Weapon != null)
            {
                Debug.LogWarning($"[UnitConfig] {UnitName}: Weapon '{Weapon.WeaponName}' is not valid for {BattlePosition}! Weapon bonuses ignored.");
            }

            if (Armor != null)
            {
                stats.MaxHealth += Armor.MaxHealthBonus;
                stats.DamageReduction = Armor.DamageReduction;
                stats.MovementSpeed *= Armor.MovementSpeedMultiplier;
            }

            if (BioMod != null)
            {
                stats.MaxHealth += BioMod.MaxHealthBonus;
                stats.AttackDamage += BioMod.AttackDamageBonus;
                stats.SkillDamage += BioMod.SkillDamageBonus;
                stats.SkillCooldown *= BioMod.SkillCooldownMultiplier;
            }

            return stats;
        }

        public bool ValidateConfig(out string errorMessage)
        {
            errorMessage = "";

            if (Weapon != null && !Weapon.IsValidForPosition(BattlePosition))
            {
                errorMessage = $"Weapon '{Weapon.WeaponName}' requires {Weapon.AllowedPosition}, but unit is {BattlePosition}";
                return false;
            }

            return true;
        }

        public GameObject SpawnUnit(Vector3 position, UnitAffiliation? overrideAffiliation = null)
        {
            if (UnitPrefab == null)
            {
                Debug.LogError($"[UnitConfig] {UnitName}: UnitPrefab is null!");
                return null;
            }

            var unitObj = Instantiate(UnitPrefab, position, Quaternion.identity);
            ApplyConfigToUnit(unitObj, overrideAffiliation);

            return unitObj;
        }

        public void ApplyConfigToUnit(GameObject unitObj, UnitAffiliation? overrideAffiliation = null)
        {
            var profile = unitObj.GetComponent<MainUnitProfile>();
            if (profile == null)
            {
                Debug.LogError($"[UnitConfig] {unitObj.name}: MainUnitProfile not found!");
                return;
            }

            var stats = CalculateFinalStats();
            var affiliation = overrideAffiliation ?? DefaultAffiliation;

            var applier = unitObj.GetComponent<UnitConfigApplier>();
            if (applier == null)
            {
                applier = unitObj.AddComponent<UnitConfigApplier>();
            }

            applier.ApplyConfig(this, stats, affiliation);
        }
    }

    [System.Serializable]
    public class UnitFinalStats
    {
        public int MaxHealth;
        public int AttackDamage;
        public int SkillDamage;
        public float AttackCooldown;
        public float SkillCooldown;
        public float MovementSpeed;
        public float AttackRange;
        public float SkillRange;
        public int DamageReduction;
    }
}
