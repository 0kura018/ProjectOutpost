using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

namespace BattleSystem.BattleControler
{

    public class BattleSceneManager : MonoBehaviour
    {
        public static BattleSceneManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string battleSceneName = "BattleScene";

        [Header("State")]
        [SerializeField] private bool _isBattleActive;
        public bool IsBattleActive => _isBattleActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartBattle(PartyConfig party, WaveConfig waves, Action<BattleResult> onComplete = null, BattleReward reward = null)
        {
            if (_isBattleActive)
            {
                Debug.LogWarning("[BattleSceneManager] Battle already in progress!");
                return;
            }

            EnsureBattleDataHolder();

            BattleDataHolder.Instance.SetBattleData(party, waves, reward);
            BattleDataHolder.Instance.OnBattleComplete = onComplete;

            LoadBattleScene();
        }

        private bool _wasStartedViaManager;

        private void Update()
        {

            if (_wasStartedViaManager && Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame && _isBattleActive)
            {
                EndBattle(new BattleResult(true));
            }
        }

        public void EndBattle(BattleResult result)
        {
            if (!_isBattleActive)
            {
                Debug.LogWarning("[BattleSceneManager] No battle to end!");
                return;
            }

            Debug.Log($"[BattleSceneManager] Battle ended. Victory: {result.IsVictory}");

            BattleDataHolder.Instance?.OnBattleComplete?.Invoke(result);
            BattleDataHolder.Instance?.ClearData();

            UnloadBattleScene();
        }

        private void EnsureBattleDataHolder()
        {
            if (BattleDataHolder.Instance == null)
            {
                var holder = new GameObject("BattleDataHolder");
                holder.AddComponent<BattleDataHolder>();
            }
        }

        private void LoadBattleScene()
        {
            _isBattleActive = true;
            _wasStartedViaManager = true;

            var loadOp = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
            if (loadOp != null)
            {
                loadOp.completed += OnBattleSceneLoaded;
            }

            Debug.Log($"[BattleSceneManager] Loading battle scene: {battleSceneName}");
        }

        private void OnBattleSceneLoaded(AsyncOperation op)
        {
            Debug.Log("[BattleSceneManager] Battle scene loaded!");

            Scene battleScene = SceneManager.GetSceneByName(battleSceneName);
            if (battleScene.IsValid())
            {
                SceneManager.SetActiveScene(battleScene);
            }
        }

        private void UnloadBattleScene()
        {

            Scene battleScene = SceneManager.GetSceneByName(battleSceneName);
            if (!battleScene.IsValid() || !battleScene.isLoaded)
            {
                Debug.LogWarning($"[BattleSceneManager] Scene '{battleSceneName}' is not loaded, skipping unload.");
                _isBattleActive = false;
                return;
            }

            var unloadOp = SceneManager.UnloadSceneAsync(battleSceneName);
            if (unloadOp != null)
            {
                unloadOp.completed += OnBattleSceneUnloaded;
            }

            Debug.Log($"[BattleSceneManager] Unloading battle scene: {battleSceneName}");
        }

        private void OnBattleSceneUnloaded(AsyncOperation op)
        {
            _isBattleActive = false;
            _wasStartedViaManager = false;
            Debug.Log("[BattleSceneManager] Battle scene unloaded!");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
