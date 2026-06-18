using UnityEngine;
using BattleSystem.Units;

namespace BattleSystem.CardSystem.Effects
{

    public class AreaDamageZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private SpriteRenderer areaSprite;
        [SerializeField] private float damageDelay = 0f;
        [SerializeField] private float lifetime = 2f;

        private int _damage;
        private float _radius;
        private LayerMask _unitLayerMask;
        private UnitAffiliation? _targetAffiliation;
        private bool _damageApplied;

        public void Initialize(int damage, float radius, LayerMask unitLayerMask, UnitAffiliation? targetAffiliation = null, float damageDelay = 0f, float zlifetime = 0f)
        {
            _damage = damage;
            _radius = radius;
            _unitLayerMask = unitLayerMask;
            _targetAffiliation = targetAffiliation;
            this.damageDelay = damageDelay;
            _damageApplied = false;
            lifetime = zlifetime;

            if (damageDelay <= 0f)
            {
                ApplyDamage();
            }
            else
            {
                Invoke(nameof(ApplyDamage), damageDelay);
            }

            if (lifetime > 0f)
            {
                Destroy(gameObject, lifetime);
            }
        }

        private void ApplyDamage()
        {
            if (_damageApplied) return;
            _damageApplied = true;
            areaSprite = GetComponent<SpriteRenderer>();
            areaSprite.enabled = true;
            var colliders = Physics2D.OverlapCircleAll(transform.position, _radius, _unitLayerMask);

            foreach (var col in colliders)
            {
                var unit = col.GetComponentInParent<UnitStateMachine>();
                if (unit == null) continue;

                if (_targetAffiliation.HasValue &&
                    unit.UnitCurrentStats.UnitAffiliation != _targetAffiliation.Value)
                {
                    continue;
                }

                unit.UnitCurrentStats.MainUnitProfile.TakeDamage(_damage);
                Debug.Log($"[AreaDamageZone] Dealt {_damage} damage to {unit.name}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius > 0 ? _radius : 1f);
        }
    }
}
