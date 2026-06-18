using UnityEngine;

namespace BattleSystem.CardSystem
{

    public class CardInstance
    {
        public CardConfig Config { get; }

        public int CurrentEnergyCost { get; set; }

        public CardInstance(CardConfig config)
        {
            Config = config;
            CurrentEnergyCost = config.EnergyCost;
        }

        public void ResetModifiers()
        {
            CurrentEnergyCost = Config.EnergyCost;
        }
    }
}