namespace BattleSystem.Units
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UnitCanvas : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider _healthBar; public Slider HealthBar => _healthBar;

        private MainUnitProfile _mainUnitProfile;

        private void Awake()
        {
            _mainUnitProfile = GetComponentInParent<MainUnitProfile>();
            _healthBar.maxValue = _mainUnitProfile.MaxHealth;
            _healthBar.value = _mainUnitProfile.MaxHealth;
        }

        public void UpdateHealthBar(int currentHealth)
        {
            _healthBar.value = currentHealth;
        }
    }
}