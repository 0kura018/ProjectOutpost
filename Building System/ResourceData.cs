using UnityEngine;

namespace BuildingSystem
{

    [CreateAssetMenu(menuName = "Resources/Resource Data", fileName = "New Resource")]
    public class ResourceData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("�������� �������")]
        public string ResourceName;

        [Tooltip("������ ������� ��� UI")]
        public Sprite Icon;

        [Tooltip("�������� �������")]
        [TextArea(2, 4)]
        public string Description;

        [Header("Type")]
        [Tooltip("��� �������")]
        public ResourceType Type;

        [Header("Visual")]
        [Tooltip("���� ��� UI ���������")]
        public Color ResourceColor = Color.white;

        [Header("Gameplay")]
        [Tooltip("��������� ���������� �������")]
        public int StartingAmount = 0;

        [Tooltip("������������ ���������� (0 = ��� ������)")]
        public int MaxAmount = 0;
    }
}
