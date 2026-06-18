namespace BattleSystem.Units
{
    using UnityEngine;

    public class UnitStateMachine : MonoBehaviour
    {
        public UnitBaseState currentState;

        public UnitIdleState IdleState = new UnitIdleState();
        public UnitWalkState WalkState = new UnitWalkState();
        public UnitReadyToAttackState ReadyToAttackState = new UnitReadyToAttackState();
        public UnitUseSkillState UseSkillState = new UnitUseSkillState();
        public UnitAttackState AttackState = new UnitAttackState();
        public UnitWalkTowardState WalkTowardState = new UnitWalkTowardState();

        private UnitCurrentStats _unitCurrentStats; public UnitCurrentStats UnitCurrentStats { get { return _unitCurrentStats; } }

        public GameObject VisualObject;

        public bool CanUseSkill = false;
        private float _skillCooldownTime;

        private void Awake()
        {
            _unitCurrentStats = GetComponent<UnitCurrentStats>();
        }
        private void Start()
        {

            var affiliation = _unitCurrentStats.MainUnitProfile != null
                ? _unitCurrentStats.MainUnitProfile.UnitAffiliation
                : _unitCurrentStats.UnitAffiliation;

            if (_unitCurrentStats.MainBattleManager == null)
            {
                _unitCurrentStats.MainBattleManager = FindAnyObjectByType<BattleSystem.BattleControler.MainBattleManager>();
            }

            if (affiliation == UnitAffiliation.Ally && _unitCurrentStats.MainBattleManager != null)
            {

                _unitCurrentStats.CurrentGoTo = _unitCurrentStats.GetPersonalReturnPoint();
                _unitCurrentStats.CurrentTarget = null;
                currentState = WalkState;
                Debug.Log($"[{gameObject.name}] Ally starting WalkState to return point: {_unitCurrentStats.CurrentGoTo?.transform.position}");
            }
            else
            {
                currentState = WalkTowardState;
                Debug.Log($"[{gameObject.name}] Enemy starting WalkTowardState");
            }

            currentState.EnterState(this);

            _skillCooldownTime = _unitCurrentStats.SkillCooldownTime;
        }

        private void Update()
        {
            currentState.UpdateState(this);

            if (!CanUseSkill)
            {
                if (_skillCooldownTime <= 0f)
                {
                    CanUseSkill = true;
                    _skillCooldownTime = _unitCurrentStats.SkillCooldownTime;
                    return;
                }
                _skillCooldownTime -= Time.deltaTime;
            }
        }

        public void SwitchState(UnitBaseState state)
        {
            currentState = state;
            currentState.EnterState(this);
        }

        public void ResetSkillCooldown()
        {
            _skillCooldownTime = _unitCurrentStats.SkillCooldownTime;
            CanUseSkill = false;
        }

        private void OnDrawGizmosSelected()
        {

            float attackRange = _unitCurrentStats != null
                ? _unitCurrentStats.AttackRange
                : GetComponent<MainUnitProfile>()?.DefaultAttackRange ?? 1.5f;

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            float skillRange = _unitCurrentStats != null
                ? _unitCurrentStats.SkillRange
                : GetComponent<MainUnitProfile>()?.DefaultSkillRange ?? 3f;

            if (skillRange > attackRange)
            {
                Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, skillRange);
            }
        }
    }
}