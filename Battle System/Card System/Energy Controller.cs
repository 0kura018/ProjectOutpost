using System;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.CardSystem
{

    public class EnergyController : MonoBehaviour
    {
        public static EnergyController Instance { get; private set; }

        [Header("Energy Settings")]
        [SerializeField] private int maxEnergy = 5;
        [SerializeField] private float rechargeTime = 3f;

        [Header("UI References")]
        [SerializeField] private Transform energyContainer;
        [SerializeField] private GameObject energySegmentPrefab;
        [SerializeField] private Slider rechargeSlider;

        [Header("Colors")]
        [SerializeField] private Color filledColor = Color.yellow;
        [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f);

        private int _currentEnergy;
        private float _rechargeProgress;
        private Image[] _energySegments;

        public int CurrentEnergy => _currentEnergy;
        public int MaxEnergy => maxEnergy;

        public event Action<int, int> OnEnergyChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeUI();
            SetEnergy(maxEnergy);
        }

        private void Update()
        {
            UpdateRecharge();
        }

        private void InitializeUI()
        {
            if (energyContainer == null || energySegmentPrefab == null) return;

            foreach (Transform child in energyContainer)
            {
                Destroy(child.gameObject);
            }

            _energySegments = new Image[maxEnergy];

            for (int i = 0; i < maxEnergy; i++)
            {
                var segment = Instantiate(energySegmentPrefab, energyContainer);
                _energySegments[i] = segment.GetComponent<Image>();
            }

            if (rechargeSlider != null)
            {
                rechargeSlider.minValue = 0f;
                rechargeSlider.maxValue = 1f;
                rechargeSlider.value = 0f;
            }
        }

        private void UpdateRecharge()
        {
            if (_currentEnergy >= maxEnergy)
            {
                _rechargeProgress = 0f;
                UpdateRechargeSlider();
                return;
            }

            _rechargeProgress += Time.deltaTime / rechargeTime;

            if (_rechargeProgress >= 1f)
            {
                _rechargeProgress = 0f;
                AddEnergy(1);
            }

            UpdateRechargeSlider();
        }

        private void UpdateRechargeSlider()
        {
            if (rechargeSlider != null)
            {
                rechargeSlider.value = _currentEnergy >= maxEnergy ? 1f : _rechargeProgress;
            }
        }

        public void SetEnergy(int amount)
        {
            _currentEnergy = Mathf.Clamp(amount, 0, maxEnergy);
            UpdateUI();
            OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
        }

        public void AddEnergy(int amount)
        {
            SetEnergy(_currentEnergy + amount);
        }

        public bool SpendEnergy(int amount)
        {
            if (_currentEnergy < amount) return false;

            SetEnergy(_currentEnergy - amount);
            return true;
        }

        public bool HasEnergy(int amount)
        {
            return _currentEnergy >= amount;
        }

        public void OnEnemyKilled()
        {
            AddEnergy(1);
        }

        public void SetMaxEnergy(int newMax)
        {
            maxEnergy = Mathf.Max(1, newMax);
            InitializeUI();
            SetEnergy(Mathf.Min(_currentEnergy, maxEnergy));
        }

        private void UpdateUI()
        {
            if (_energySegments == null) return;

            for (int i = 0; i < _energySegments.Length; i++)
            {
                if (_energySegments[i] != null)
                {
                    _energySegments[i].color = i < _currentEnergy ? filledColor : emptyColor;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
