using UnityEngine;
using System.Collections.Generic;

namespace BuildingSystem
{

    [CreateAssetMenu(menuName = "Resources/Resources Config", fileName = "ResourcesConfig")]
    public class ResourcesConfig : ScriptableObject
    {
        [Header("All Resources")]
        [Tooltip("������ ���� ��������� �������� � ����")]
        public ResourceData[] AllResources;

        public ResourceData GetResourceData(ResourceType type)
        {
            foreach (var resource in AllResources)
            {
                if (resource.Type == type)
                    return resource;
            }

            Debug.LogWarning($"[ResourcesConfig] Resource data not found for type: {type}");
            return null;
        }

        public Dictionary<ResourceType, int> GetStartingResources()
        {
            var startingResources = new Dictionary<ResourceType, int>();

            foreach (var resource in AllResources)
            {
                if (resource.StartingAmount > 0)
                {
                    startingResources[resource.Type] = resource.StartingAmount;
                }
            }

            return startingResources;
        }

        private void OnValidate()
        {
            #if UNITY_EDITOR

            var typeSet = new HashSet<ResourceType>();
            foreach (var resource in AllResources)
            {
                if (resource == null) continue;

                if (!typeSet.Add(resource.Type))
                {
                    Debug.LogWarning($"[ResourcesConfig] Duplicate resource type found: {resource.Type}", this);
                }
            }
            #endif
        }
    }
}
