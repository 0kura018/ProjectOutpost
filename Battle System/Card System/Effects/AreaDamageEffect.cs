using UnityEngine;
using BattleSystem.Units;

namespace BattleSystem.CardSystem.Effects
{

    [CreateAssetMenu(menuName = "Battle/Cards/Effects/Area Damage")]
    public class AreaDamageEffect : CardEffect
    {
        [Header("Damage Settings")]
        [Tooltip("������� ����")]
        public int BaseDamage = 20;

        [Header("Area Settings")]
        [Tooltip("������ ������� �����")]
        public float DamageRadius = 3f;

        [Tooltip("�������� ����� ���������� ����� ����� ������ �������")]
        public float DamageDelay = 0.5f;

        [Header("Spawn Settings")]
        [Tooltip("������ ���� ����� (������ ����� AreaDamageZone ��� ����� ��������)")]
        public GameObject DamageZonePrefab;

        [Tooltip("����� ����� ����")]
        public float ZoneLifetime = 2f;

        [Header("Target Filter")]
        [Tooltip("���� ������ ��� ������ �����")]
        public LayerMask UnitLayerMask = ~0;

        [Tooltip("���� ���������: Enemy, Ally ��� ��� (����� �������)")]
        public bool OnlyEnemies = true;

        public override void Execute(UnitStateMachine target, UnitStateMachine caster = null, float potency = 1f)
        {
            if (target != null)
            {
                SpawnDamageZone(target.transform.position, potency, OnlyEnemies ? UnitAffiliation.Enemy : (UnitAffiliation?)null);
            }
        }

        public override void ExecuteAtPoint(Vector3 worldPoint, float potency = 1f)
        {
            SpawnDamageZone(worldPoint, potency, OnlyEnemies ? UnitAffiliation.Enemy : (UnitAffiliation?)null);
        }

        private void SpawnDamageZone(Vector3 position, float potency, UnitAffiliation? targetAffiliation)
        {

            SpawnVisualEffect(position);

            GameObject zoneObj;

            if (DamageZonePrefab != null)
            {
                zoneObj = Instantiate(DamageZonePrefab, position, Quaternion.identity);
            }
            else
            {
                zoneObj = new GameObject("AreaDamageZone");
                zoneObj.transform.position = position;
            }

            var damageZone = zoneObj.GetComponent<AreaDamageZone>();
            if (damageZone == null)
            {
                damageZone = zoneObj.AddComponent<AreaDamageZone>();
            }

            int finalDamage = Mathf.RoundToInt(BaseDamage * potency);

            damageZone.Initialize(
                damage: finalDamage,
                radius: DamageRadius,
                unitLayerMask: UnitLayerMask,
                targetAffiliation: targetAffiliation,
                damageDelay: DamageDelay + Delay,
                zlifetime: ZoneLifetime
            );

            Debug.Log($"[AreaDamageEffect] Spawned damage zone at {position}, damage: {finalDamage}, radius: {DamageRadius}");
        }
    }
}
