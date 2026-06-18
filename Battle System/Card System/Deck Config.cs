using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.CardSystem
{
    [CreateAssetMenu(menuName = "Battle/Cards/Deck Config")]
    public class DeckConfig : ScriptableObject
    {
        [Header("Deck Info")]
        public string DeckName;
        [TextArea] public string Description;

        [Header("Cards")]
        [Tooltip("����� � ������ (����� ��������� ���������)")]
        public List<CardEntry> Cards = new();

        [Header("Deck Settings")]
        [Tooltip("������������ ���������� ���� � ������")]
        public int MaxDeckSize = 30;

        [Tooltip("������������ ���������� ����� ����� �����")]
        public int MaxCopiesPerCard = 3;

        public int TotalCardCount
        {
            get
            {
                int count = 0;
                foreach (var entry in Cards)
                    count += entry.Count;
                return count;
            }
        }

        public List<CardConfig> GetAllCards()
        {
            var result = new List<CardConfig>();
            foreach (var entry in Cards)
            {
                for (int i = 0; i < entry.Count; i++)
                {
                    if (entry.Card != null)
                        result.Add(entry.Card);
                }
            }
            return result;
        }
    }

    [System.Serializable]
    public class CardEntry
    {
        public CardConfig Card;
        [Range(1, 10)] public int Count = 1;
    }
}