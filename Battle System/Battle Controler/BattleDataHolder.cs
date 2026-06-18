using UnityEngine;
using BattleSystem.BattleControler;

namespace BattleSystem.BattleControler
{

    public class BattleDataHolder : MonoBehaviour
    {
        public static BattleDataHolder Instance { get; private set; }

        [Header("Battle Setup")]
        [Tooltip("������������ ������� ������")]
        public PartyConfig PartyConfig;

        [Tooltip("������������ ���� ������")]
        public WaveConfig WaveConfig;

        [Header("Rewards")]
        [Tooltip("������� �� ������")]
        public BattleReward Reward;

        public System.Action<BattleResult> OnBattleComplete;

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

        public void SetBattleData(PartyConfig party, WaveConfig waves, BattleReward reward = null)
        {
            PartyConfig = party;
            WaveConfig = waves;
            Reward = reward;
        }

        public void ClearData()
        {
            PartyConfig = null;
            WaveConfig = null;
            Reward = null;
            OnBattleComplete = null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    [System.Serializable]
    public class BattleResult
    {
        public bool IsVictory;
        public int EnemiesKilled;
        public int AlliesLost;
        public float BattleDuration;

        public BattleResult(bool victory)
        {
            IsVictory = victory;
        }
    }

    [System.Serializable]
    public class BattleReward
    {
        public int Gold;
        public int Experience;

    }
}
