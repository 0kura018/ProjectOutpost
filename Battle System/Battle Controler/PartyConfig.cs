using UnityEngine;
using System.Collections.Generic;
using BattleSystem.Units;
using BattleSystem.Units.Config;

[CreateAssetMenu(menuName = "Battle/PartyConfig")]
public class PartyConfig : ScriptableObject
{
    public List<PartySpawnData> Party;
}

[System.Serializable]
public class PartySpawnData
{
    [Tooltip("������ ����� (����� �������)")]
    public UnitConfig UnitConfig;

    [Tooltip("������ ����� (������ �������, ������������ ���� UnitConfig �� �����)")]
    public UnitStateMachine Prefab;

    [Tooltip("������� �������� ����� ��������")]
    public float BaseInterval = 0.5f;

    [Tooltip("������ +- � ���������")]
    public float IntervalRandomOffset = 0.25f;
}
