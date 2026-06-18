using UnityEngine;
using System;
using System.Collections.Generic;

namespace BuildingSystem
{

    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Starting Resources")]
        [SerializeField] private int _startingGold = 1000;
        [SerializeField] private int _startingWood = 500;
        [SerializeField] private int _startingStone = 300;
        [SerializeField] private int _startingIron = 100;
        [SerializeField] private int _startingEnergy = 50;

        private Dictionary<ResourceType, int> _resources = new();

        public event Action<ResourceType, int> OnResourceChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeResources();
        }

        private void InitializeResources()
        {
            _resources[ResourceType.Gold] = _startingGold;
            _resources[ResourceType.Wood] = _startingWood;
            _resources[ResourceType.Stone] = _startingStone;
            _resources[ResourceType.Iron] = _startingIron;
            _resources[ResourceType.Energy] = _startingEnergy;
        }

        public int GetResource(ResourceType type)
        {
            return _resources.TryGetValue(type, out int amount) ? amount : 0;
        }

        public bool HasResource(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }

        public void AddResource(ResourceType type, int amount)
        {
            if (amount <= 0) return;

            if (!_resources.ContainsKey(type))
                _resources[type] = 0;

            _resources[type] += amount;
            OnResourceChanged?.Invoke(type, _resources[type]);

            Debug.Log($"[Resources] +{amount} {type}. Total: {_resources[type]}");
        }

        public bool SpendResource(ResourceType type, int amount)
        {
            if (!HasResource(type, amount))
            {
                Debug.LogWarning($"[Resources] Not enough {type}! Need {amount}, have {GetResource(type)}");
                return false;
            }

            _resources[type] -= amount;
            OnResourceChanged?.Invoke(type, _resources[type]);

            Debug.Log($"[Resources] -{amount} {type}. Total: {_resources[type]}");
            return true;
        }

        public void SetResource(ResourceType type, int amount)
        {
            _resources[type] = Mathf.Max(0, amount);
            OnResourceChanged?.Invoke(type, _resources[type]);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
