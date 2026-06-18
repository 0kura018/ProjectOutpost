using UnityEngine;
using System.Collections.Generic;
using BattleSystem.Units;
using BattleSystem.Units.Config;

[CreateAssetMenu(menuName = "Battle/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    public List<WaveData> Waves;
}

[System.Serializable]
public class WaveData
{
    [Tooltip("�������� ����� ��������� �����")]
    public float DelayAfterWave = 5f;

    public List<EnemySpawnData> Enemies;
}

[System.Serializable]
public class EnemySpawnData
{
    [Tooltip("������ ����� (����� �������)")]
    public UnitConfig UnitConfig;

    [Tooltip("������ ����� (������ �������, ������������ ���� UnitConfig �� �����)")]
    public UnitStateMachine Prefab;

    public int Count = 3;

    [Tooltip("������� �������� ����� ��������")]
    public float BaseInterval = 1f;

    [Tooltip("������ +- � ���������")]
    public float IntervalRandomOffset = 0.5f;
}
