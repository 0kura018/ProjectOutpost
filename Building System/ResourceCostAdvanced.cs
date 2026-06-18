using UnityEngine;

namespace BuildingSystem
{

    [System.Serializable]
    public class ResourceCostAdvanced
    {
        [Tooltip("������ �� ������ ������� (�����������)")]
        public ResourceData ResourceData;

        [Tooltip("��� ������� (������������ ���� ResourceData �� �����)")]
        public ResourceType Type;

        [Tooltip("����������")]
        public int Amount;

        public ResourceType GetResourceType()
        {
            return ResourceData != null ? ResourceData.Type : Type;
        }

        public Sprite GetIcon()
        {
            return ResourceData?.Icon;
        }
    }
}
