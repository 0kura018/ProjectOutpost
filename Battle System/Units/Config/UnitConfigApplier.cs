using UnityEngine;

namespace BattleSystem.Units.Config
{

    public class UnitConfigApplier : MonoBehaviour
    {
        [Header("Applied Config (Runtime)")]
        [SerializeField] private UnitConfig _appliedConfig;
        [SerializeField] private UnitFinalStats _finalStats;

        public UnitConfig AppliedConfig => _appliedConfig;
        public UnitFinalStats FinalStats => _finalStats;

        public void ApplyConfig(UnitConfig config, UnitFinalStats stats, UnitAffiliation affiliation)
        {
            _appliedConfig = config;
            _finalStats = stats;

            var unitStats = GetComponent<UnitCurrentStats>();
            if (unitStats != null)
            {
                ApplyToUnitCurrentStats(unitStats, stats, affiliation, config);
            }

            Debug.Log($"[UnitConfigApplier] Applied config '{config.UnitName}' to {gameObject.name}");
        }

        private void ApplyToUnitCurrentStats(UnitCurrentStats unitStats, UnitFinalStats stats, UnitAffiliation affiliation, UnitConfig config)
        {

            StartCoroutine(ApplyStatsDelayed(unitStats, stats, affiliation, config));
        }

        private System.Collections.IEnumerator ApplyStatsDelayed(UnitCurrentStats unitStats, UnitFinalStats stats, UnitAffiliation affiliation, UnitConfig config)
        {

            yield return null;

            unitStats.ApplyConfigStats(stats, affiliation, config.BattlePosition, config.HealthBarOffsetY);
        }
    }
}
