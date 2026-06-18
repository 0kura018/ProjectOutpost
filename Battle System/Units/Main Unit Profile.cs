namespace BattleSystem.Units
{
    using UnityEngine;

    public abstract class MainUnitProfile : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] protected UnitAffiliation _unitAffiliation; public UnitAffiliation UnitAffiliation { get { return _unitAffiliation; } }
        [SerializeField] protected UnitBattlePosition _unitBattlePosition; public UnitBattlePosition UnitBattlePosition { get { return _unitBattlePosition; } }
        [SerializeField] protected int _maxHealth; public int MaxHealth { get { return _maxHealth; } }
        [SerializeField] protected int _defaultAttackDamage; public int DefaultAttackDamage { get { return _defaultAttackDamage; } }
        [SerializeField] protected int _defaultSkillDamage; public int DefaultSkillDamage { get { return _defaultSkillDamage; } }
        [SerializeField] protected float _defaultAttackCooldownTime; public float DefaultAttackCooldownTime { get { return _defaultAttackCooldownTime; } }
        [SerializeField] protected float _defaultSkillCooldownTime; public float DefaultSkillCooldownTime { get { return _defaultSkillCooldownTime; } }
        [SerializeField] protected float _defaultMovementSpeed; public float DefaultMovementSpeed { get { return _defaultMovementSpeed; } }
        [SerializeField] protected float _defaultAttackRange; public float DefaultAttackRange { get { return _defaultAttackRange; } }
        [SerializeField] protected float _defaultSkillRange; public float DefaultSkillRange { get { return _defaultSkillRange; } }

        [Header("UI")]
        [Tooltip("�������� ������� �������� �� Y (� ������� ��������). ��� ������ �������� � ��� ���� ��������.")]
        [SerializeField] protected float _healthBarOffsetY = 1.5f;
        public float HealthBarOffsetY { get { return _healthBarOffsetY; } }

        [SerializeField] protected UnitCurrentStats _unitCurrentStats; public UnitCurrentStats UnitCurrentStats { get { return _unitCurrentStats; } }
        public virtual void Awake()
        {
            _unitCurrentStats = GetComponent<UnitCurrentStats>();
        }

        public abstract void TakeDamage(int damage);
        public abstract void Heal(int amount);
        public abstract void Die();
        public abstract void Attack();
        public abstract void UseSkill();
        public abstract void ResetSkillCooldown();
    }

    public enum UnitAffiliation
    {
        Ally,
        Enemy
    }

    public enum UnitBattlePosition
    {
        Avanguard,
        Arierguard
    }
}
