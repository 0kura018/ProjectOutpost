using BattleSystem.BattleControler;
using BattleSystem.CardSystem;
using BattleSystem.Units.Config;
using BattleSystem.Visual;
using UnityEngine;

namespace BattleSystem.Units
{
    public class UnitCurrentStats : MonoBehaviour
    {
        [Header("Links")]
        public MainBattleManager MainBattleManager;
        [SerializeField] private MainUnitProfile _mainUnitProfile; public MainUnitProfile MainUnitProfile { get { return _mainUnitProfile; } }

        private HealthBarUI _healthBarUI;
        private UnitStateMachine _unitStateMachine;
        private UnitCanvas _unitCanvas;

        [Header("Unit Info")]
        private UnitAffiliation _unitAffiliation; public UnitAffiliation UnitAffiliation { get { return _unitAffiliation; } }
        private UnitBattlePosition _unitBattlePosition; public UnitBattlePosition UnitBattlePosition { get { return _unitBattlePosition; } }
        private int _health; public int Health { get { return _health; } }
        private int _attackDamage; public int AttackDamage { get { return _attackDamage; } }
        private int _skillDamage; public int SkillDamage { get { return _skillDamage; } }
        private float _attackCooldownTime; public float AttackCooldownTime { get { return _attackCooldownTime; } }
        private float _skillCooldownTime; public float SkillCooldownTime { get { return _skillCooldownTime; } }
        private float _movementSpeed; public float MovementSpeed { get { return _movementSpeed; } }
        private float _attackRange; public float AttackRange { get { return _attackRange; } }
        private float _skillRange; public float SkillRange { get { return _skillRange; } }

        [Header("Battle")]
        [SerializeField] private bool _isAlive = true; public bool IsAlive { get { return _isAlive; } }
        public bool CanAttack = false;
        public UnitStateMachine CurrentTarget;
        public GameObject CurrentGoTo;

        [Header("Ally Return Point")]

        private Vector3 _spawnPosition;

        private GameObject _personalReturnPoint;

        public GameObject GetPersonalReturnPoint()
        {
            if (_personalReturnPoint == null)
            {
                _personalReturnPoint = new GameObject($"ReturnPoint_{gameObject.name}");
            }

            _personalReturnPoint.transform.position = _spawnPosition + new Vector3(10f, 0f, 0f);
            return _personalReturnPoint;
        }

        public void SaveSpawnPosition()
        {
            _spawnPosition = transform.position;
        }

        private void Awake()
        {
            _mainUnitProfile = GetComponent<MainUnitProfile>();
            _unitStateMachine = GetComponent<UnitStateMachine>();
            _unitCanvas = GetComponentInChildren<UnitCanvas>();

            _spawnPosition = transform.position;
            CanAttack = false;
        }

        private void Start()
        {
            InitializeProfile();

            if (HealthBarManager.Instance != null)
            {
                bool isAlly = _mainUnitProfile.UnitAffiliation == UnitAffiliation.Ally;
                float offsetY = _mainUnitProfile.HealthBarOffsetY;
                _healthBarUI = HealthBarManager.Instance.RegisterUnit(transform, _mainUnitProfile.MaxHealth, _health, isAlly, offsetY);
            }
        }

        public virtual void InitializeProfile()
        {
            _unitAffiliation = _mainUnitProfile.UnitAffiliation;
            _unitBattlePosition = _mainUnitProfile.UnitBattlePosition;
            _health = _mainUnitProfile.MaxHealth;
            _attackDamage = _mainUnitProfile.DefaultAttackDamage;
            _skillDamage = _mainUnitProfile.DefaultSkillDamage;
            _attackCooldownTime = _mainUnitProfile.DefaultAttackCooldownTime;
            _skillCooldownTime = _mainUnitProfile.DefaultSkillCooldownTime;
            _movementSpeed = _mainUnitProfile.DefaultMovementSpeed;
            _attackRange = _mainUnitProfile.DefaultAttackRange;
            _skillRange = _mainUnitProfile.DefaultSkillRange;
        }

        public virtual void TakeDamage(int damage)
        {
            print("Taking damage: " + damage);
            _health -= damage;
            if (_health < 0)
            {
                _health = 0;
                _mainUnitProfile.Die();
            }

            UpdateHealthUI();
        }

        public virtual void Heal(int amount)
        {
            print("Taking Heal: " + amount);
            _health += amount;
            if (_health > _mainUnitProfile.MaxHealth)
            {
                _health = _mainUnitProfile.MaxHealth;
            }

            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {

            if (HealthBarManager.Instance != null)
            {
                HealthBarManager.Instance.UpdateHealth(transform, _health, _mainUnitProfile.MaxHealth);
            }

            if (_unitCanvas != null)
            {
                _unitCanvas.UpdateHealthBar(_health);
            }
        }

        public virtual void Die()
        {
            print("Unit Died");

            if (_unitAffiliation == UnitAffiliation.Enemy)
            {
                EnergyController.Instance?.OnEnemyKilled();
            }

            if (HealthBarManager.Instance != null)
            {
                HealthBarManager.Instance.UnregisterUnit(transform);
            }
            _isAlive = false;
        }

        public virtual void DownCooldowns(int time, int id)
        {
            print("Down Cooldown id: " + id + ", time: " + time);
        }

        public virtual void ResetCooldown(int id)
        {
            print("Reset Cooldown id: " + id);
            if (id == 0)
            {
                _attackCooldownTime = _mainUnitProfile.DefaultAttackCooldownTime;
            }
            else if (id == 1)
            {
                _skillCooldownTime = _mainUnitProfile.DefaultSkillCooldownTime;
            }
        }

        public void ApplyConfigStats(UnitFinalStats stats, UnitAffiliation affiliation, UnitBattlePosition position, float healthBarOffsetY)
        {
            _unitAffiliation = affiliation;
            _unitBattlePosition = position;
            _health = stats.MaxHealth;
            _attackDamage = stats.AttackDamage;
            _skillDamage = stats.SkillDamage;
            _attackCooldownTime = stats.AttackCooldown;
            _skillCooldownTime = stats.SkillCooldown;
            _movementSpeed = stats.MovementSpeed;
            _attackRange = stats.AttackRange;
            _skillRange = stats.SkillRange;

            if (HealthBarManager.Instance != null)
            {
                bool isAlly = affiliation == UnitAffiliation.Ally;
                HealthBarManager.Instance.UnregisterUnit(transform);
                HealthBarManager.Instance.RegisterUnit(transform, stats.MaxHealth, _health, isAlly, healthBarOffsetY);
            }

            Debug.Log($"[UnitCurrentStats] Config stats applied: HP={stats.MaxHealth}, ATK={stats.AttackDamage}");
        }
    }
}
