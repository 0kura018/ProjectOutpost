using UnityEngine;
using System.Collections.Generic;

namespace BaseSystem
{

    [CreateAssetMenu(menuName = "Base/Non-Combat Team")]
    public class NonCombatTeam : ScriptableObject
    {
        [Header("Team Info")]
        public string TeamName;
        [TextArea] public string Description;

        [Header("Team Members")]
        [Tooltip("Список членов команды")]
        public List<NonCombatTeamMember> Members = new List<NonCombatTeamMember>();
    }
}
