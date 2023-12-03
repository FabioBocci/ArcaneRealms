using System;
using ArcaneRealms.Scripts.Players;

namespace ArcaneRealms.Scripts.Utils
{
    [Serializable]
    public class GameStateSerializable
    {
        public PlayerState[] players;
    }
    
    [Serializable]
    public class PlayerState
    {
        public string id;
        public string[] currentDeck;
        public string[] graveyard;
        public string[] handCards;
        public string[] monsterCardOnField;
        public string[] continueSpellCards;
        public string[] delayedSpellCards;

        public int currentManaPool;
        public int usableMana;
        public int damageReceived;

    }
}