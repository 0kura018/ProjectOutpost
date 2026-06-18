using UnityEngine;
using System.Collections;
using BattleSystem.Units;
using BattleSystem.Units.Config;
using BattleSystem.CardSystem;
using BattleSystem.BattleControler;

public class BattleSpawner : MonoBehaviour
{
    public static BattleSpawner Instance { get; private set; }

    [Header("Battle Data (������������ ���� ��� BattleDataHolder)")]
    [SerializeField] private WaveConfig _waveConfig;
    [SerializeField] private PartyConfig _partyConfig;

    [Header("Use External Data")]
    [Tooltip("���� true � ���� ������ �� BattleDataHolder (��� �������� ����� BattleSceneManager)")]
    [SerializeField] private bool _useExternalData = true;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _enemySpawnPoints;

    [Header("Party Spawn Points")]
    [Tooltip("����� ������ ��� ��������� (������ 3 ���������)")]
    [SerializeField] private Transform[] _vanguardSpawnPoints;

    [Tooltip("����� ������ ��� ���������� (��������� ���������)")]
    [SerializeField] private Transform[] _rearguardSpawnPoints;

    [SerializeField] private MainBattleManager _mainBattleManager;

    private int _vanguardCount = 3;

    public bool AllWavesCompleted { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        if (_useExternalData && BattleDataHolder.Instance != null)
        {
            if (BattleDataHolder.Instance.PartyConfig != null)
                _partyConfig = BattleDataHolder.Instance.PartyConfig;

            if (BattleDataHolder.Instance.WaveConfig != null)
                _waveConfig = BattleDataHolder.Instance.WaveConfig;

            Debug.Log("[BattleSpawner] Using data from BattleDataHolder");
        }

        StartCoroutine(SpawnBattle());
    }

    private IEnumerator SpawnBattle()
    {
        AllWavesCompleted = false;

        yield return StartCoroutine(SpawnParty());

        foreach (var wave in _waveConfig.Waves)
        {
            yield return StartCoroutine(SpawnWave(wave));
            yield return new WaitForSeconds(wave.DelayAfterWave);
        }

        AllWavesCompleted = true;
        Debug.Log("[BattleSpawner] All waves completed!");

        _mainBattleManager?.AliveCheck();
    }

    private IEnumerator SpawnParty()
    {
        if (_partyConfig == null || _partyConfig.Party == null) yield break;

        int characterIndex = 0;

        foreach (var characterData in _partyConfig.Party)
        {
            if (characterData.UnitConfig != null && characterData.UnitConfig.PersonalCard != null)
            {
                AddPersonalCardToDeck(characterData.UnitConfig, shuffle: false);
            }
        }

        DeckController.Instance?.Shuffle();

        DeckController.Instance?.DrawToFullHand();

        foreach (var characterData in _partyConfig.Party)
        {
            if (characterData.UnitConfig == null && characterData.Prefab == null) continue;

            SpawnPartyMember(characterData, characterIndex);
            characterIndex++;

            float interval = characterData.BaseInterval +
                             Random.Range(-characterData.IntervalRandomOffset,
                                           characterData.IntervalRandomOffset);
            yield return new WaitForSeconds(Mathf.Max(0.05f, interval));
        }
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        foreach (var enemy in wave.Enemies)
        {
            yield return StartCoroutine(SpawnEnemies(enemy));
        }
    }

    private IEnumerator SpawnEnemies(EnemySpawnData data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            SpawnEnemy(data);

            float interval = data.BaseInterval +
                             Random.Range(-data.IntervalRandomOffset,
                                           data.IntervalRandomOffset);

            yield return new WaitForSeconds(Mathf.Max(0.05f, interval));
        }
    }

    private void SpawnEnemy(EnemySpawnData data)
    {
        int rand = Random.Range(0, _enemySpawnPoints.Length);
        Transform point = _enemySpawnPoints[rand];

        UnitStateMachine enemyInstance;

        if (data.UnitConfig != null)
        {
            var unitObj = data.UnitConfig.SpawnUnit(point.position, UnitAffiliation.Enemy);
            if (unitObj != null)
            {
                unitObj.transform.rotation = point.rotation;
                enemyInstance = unitObj.GetComponent<UnitStateMachine>();
            }
            else
            {
                Debug.LogError("[BattleSpawner] Failed to spawn unit from UnitConfig!");
                return;
            }
        }

        else if (data.Prefab != null)
        {
            enemyInstance = Instantiate(data.Prefab, point.position, point.rotation);
        }
        else
        {
            Debug.LogError("[BattleSpawner] EnemySpawnData has no UnitConfig or Prefab!");
            return;
        }

        _mainBattleManager.RegisterEnemy(enemyInstance);
    }

    private void SpawnPartyMember(PartySpawnData data, int index)
    {
        Transform spawnPoint;

        if (index < _vanguardCount)
        {

            if (_vanguardSpawnPoints != null && _vanguardSpawnPoints.Length > 0)
            {
                int pointIndex = index % _vanguardSpawnPoints.Length;
                spawnPoint = _vanguardSpawnPoints[pointIndex];
            }
            else
            {
                Debug.LogWarning("BattleSpawner: Vanguard spawn points not set!");
                spawnPoint = transform;
            }
        }
        else
        {

            if (_rearguardSpawnPoints != null && _rearguardSpawnPoints.Length > 0)
            {
                int rearIndex = index - _vanguardCount;
                int pointIndex = rearIndex % _rearguardSpawnPoints.Length;
                spawnPoint = _rearguardSpawnPoints[pointIndex];
            }
            else
            {
                Debug.LogWarning("BattleSpawner: Rearguard spawn points not set!");
                spawnPoint = transform;
            }
        }

        UnitStateMachine partyMember;

        if (data.UnitConfig != null)
        {
            var unitObj = data.UnitConfig.SpawnUnit(spawnPoint.position, UnitAffiliation.Ally);
            if (unitObj != null)
            {
                unitObj.transform.rotation = spawnPoint.rotation;
                partyMember = unitObj.GetComponent<UnitStateMachine>();
            }
            else
            {
                Debug.LogError("[BattleSpawner] Failed to spawn party member from UnitConfig!");
                return;
            }

        }

        else if (data.Prefab != null)
        {
            partyMember = Instantiate(data.Prefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("[BattleSpawner] PartySpawnData has no UnitConfig or Prefab!");
            return;
        }

        RegisterAlly(partyMember);

        string unitName = data.UnitConfig != null ? data.UnitConfig.UnitName : data.Prefab.name;
        Debug.Log($"[Spawner] Spawned party member '{unitName}' at {(index < _vanguardCount ? "VANGUARD" : "REARGUARD")} position {index}");
    }

    private void AddPersonalCardToDeck(UnitConfig config, bool shuffle = true)
    {
        if (config == null || config.PersonalCard == null) return;

        if (DeckController.Instance != null)
        {
            if (shuffle)
            {
                DeckController.Instance.AddCardAndShuffle(config.PersonalCard, config.PersonalCardCopies);
            }
            else
            {
                DeckController.Instance.AddCardToDeck(config.PersonalCard, config.PersonalCardCopies);
            }
            Debug.Log($"[BattleSpawner] Added {config.PersonalCardCopies}x '{config.PersonalCard.CardName}' from {config.UnitName}");
        }
        else
        {
            Debug.LogWarning("[BattleSpawner] DeckController.Instance is null, can't add personal card!");
        }
    }

    private void RegisterAlly(UnitStateMachine unit)
    {
        if (_mainBattleManager == null) return;
        _mainBattleManager.RegisterAlly(unit);
    }

    public Vector3 GetFirstSpawnPointPosition()
    {
        if (_enemySpawnPoints != null && _enemySpawnPoints.Length > 0 && _enemySpawnPoints[0] != null)
            return _enemySpawnPoints[0].position;
        return transform.position;
    }
}
