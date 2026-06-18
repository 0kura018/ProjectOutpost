using UnityEngine;

namespace BuildingSystem
{

    [CreateAssetMenu(menuName = "Building/Room Config")]
    public class RoomConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public string RoomName;
        public Sprite Icon;
        [TextArea] public string Description;

        [Header("Prefab")]
        [Tooltip("������ ������� ��� ������")]
        public GameObject RoomPrefab;

        [Header("Size (in grid cells)")]
        [Tooltip("������ ������� � ������� �����")]
        public int Width = 1;

        [Tooltip("������ ������� � ������� �����")]
        public int Height = 1;

        [Header("Cost")]
        [Tooltip("��������� ���������")]
        public ResourceCost[] BuildCost;

        [Header("Build Time")]
        [Tooltip("����� ��������� � ������� ����� (0 = ���������)")]
        public float BuildTime = 0f;

        [Header("Connection Points")]
        [Tooltip("����� ����� ����� (������������� ������� �� Y � �������)")]
        public int[] LeftConnections = { 0 };

        [Tooltip("����� ����� ������")]
        public int[] RightConnections = { 0 };

        [Tooltip("����� ����� ������")]
        public int[] TopConnections = { 0 };

        [Tooltip("����� ����� �����")]
        public int[] BottomConnections = { 0 };

        [Header("Requirements")]
        [Tooltip("��������� �� ���������� � ������ ��������")]
        public bool RequiresConnection = true;

        [Tooltip("����� �� ���� ��������� �� ����� (������ ����)")]
        public bool CanBuildOnGround = true;

        [Header("Build Limit")]
        [Tooltip("������������ ���������� ������ ����� ����")]
        public bool HasBuildLimit = false;

        [Tooltip("������������ ���������� (���� HasBuildLimit = true)")]
        public int MaxCount = 1;

        [Header("Category")]
        public RoomCategory Category;

        [Header("Work Settings")]
        [Tooltip("Название работы (для UI)")]
        public string WorkName;

        [Tooltip("Может ли комната предоставлять работу?")]
        public bool CanProvideWork = false;

        [Tooltip("Время в игровых часах для выполнения одного цикла работы")]
        public float WorkTimeHours = 2f;

        [Tooltip("Ресурс, выдаваемый за работу")]
        public ResourceType ProducedResource = ResourceType.Gold;

        [Tooltip("Количество ресурса за один цикл")]
        public int ProducedAmount = 10;
    }

    [System.Serializable]
    public class ResourceCost
    {
        public ResourceType Type;
        public int Amount;
    }

    public enum ResourceType
    {
        Gold,
        Wood,
        Stone,
        Iron,
        Energy
    }

    public enum RoomCategory
    {
        Production,
        Storage,
        Living,
        Utility,
        Decoration
    }
}
