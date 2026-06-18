using UnityEngine;
using BattleSystem.Units.Config;
using BuildingSystem;

namespace BaseSystem
{

    [CreateAssetMenu(menuName = "Base/Non-Combat Team Member")]
    public class NonCombatTeamMember : ScriptableObject
    {
        [Header("Basic Info")]
        public string MemberName;
        public Sprite Portrait;
        [TextArea] public string Description;

        [Header("Unit Reference")]
        [Tooltip("Конфиг боевого юнита для этого члена команды")]
        public UnitConfig UnitConfig;

        [Header("Non-Combat Prefab")]
        [Tooltip("Префаб для передвижения по базе (должен иметь компонент NonCombatUnit)")]
        public GameObject NonCombatPrefab;

        [Header("Movement")]
        [Tooltip("Скорость передвижения по базе")]
        public float MovementSpeed = 3f;

        [Header("Behavior")]
        [Tooltip("Время ожидания в комнате перед переходом в другую")]
        public float IdleTimeInRoom = 5f;

        [Header("Sleep")]
        [Tooltip("Optional room config used as sleep room for this unit.")]
        public RoomConfig SleepRoomConfig;
    }
}

