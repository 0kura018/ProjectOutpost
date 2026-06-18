using UnityEngine;
using BattleSystem.Units;

namespace BattleSystem.CardSystem
{

    public abstract class CardEffect : ScriptableObject
    {
        [Header("Base Effect Info")]
        public string EffectName;
        [TextArea] public string EffectDescription;

        [Header("Visual")]
        [Tooltip("���������� ������, ��������� � ����� ����������")]
        public GameObject VisualEffectPrefab;

        [Tooltip("����� ����� ����������� ������� (0 = �� ����������)")]
        public float VisualEffectLifetime = 2f;

        [Header("Timing")]
        [Tooltip("�������� ����� ����������� �������")]
        public float Delay = 0f;

        public abstract void Execute(UnitStateMachine target, UnitStateMachine caster = null, float potency = 1f);

        public virtual void ExecuteAtPoint(Vector3 worldPoint, float potency = 1f)
        {
            SpawnVisualEffect(worldPoint);
        }

        protected GameObject SpawnVisualEffect(Vector3 position)
        {
            if (VisualEffectPrefab == null) return null;

            var vfx = Instantiate(VisualEffectPrefab, position, Quaternion.identity);

            if (VisualEffectLifetime > 0f)
            {
                Destroy(vfx, VisualEffectLifetime);
            }

            return vfx;
        }

        protected GameObject SpawnVisualEffectOnTarget(UnitStateMachine target)
        {
            if (target == null) return null;
            return SpawnVisualEffect(target.transform.position);
        }
    }
}
