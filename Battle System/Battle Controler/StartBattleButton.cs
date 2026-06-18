using UnityEngine;
using UnityEngine.UI;
using BattleSystem.BattleControler;

public class StartBattleButton : MonoBehaviour
{
    [Header("Battle Data")]
    [Tooltip("������� ������")]
    [SerializeField] private PartyConfig _partyConfig;

    [Tooltip("����� ������")]
    [SerializeField] private WaveConfig _waveConfig;

    [Header("Optional UI")]
    [SerializeField] private Button _startButton;

    private void Start()
    {

        if (BattleSceneManager.Instance == null)
        {
            var manager = new GameObject("BattleSceneManager");
            manager.AddComponent<BattleSceneManager>();
        }

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(StartBattle);
        }
    }

    public void StartBattle()
    {
        if (_partyConfig == null || _waveConfig == null)
        {
            Debug.LogError("[StartBattleButton] PartyConfig or WaveConfig is not assigned!");
            return;
        }

        Debug.Log("[StartBattleButton] Starting battle...");

        BattleSceneManager.Instance.StartBattle(
            party: _partyConfig,
            waves: _waveConfig,
            onComplete: OnBattleComplete
        );
    }

    private void OnBattleComplete(BattleResult result)
    {
        if (result.IsVictory)
        {
            Debug.Log($"?? VICTORY! Enemies killed: {result.EnemiesKilled}, Duration: {result.BattleDuration:F1}s");

        }
        else
        {
            Debug.Log($"?? DEFEAT! Allies lost: {result.AlliesLost}, Duration: {result.BattleDuration:F1}s");

        }
    }
}
