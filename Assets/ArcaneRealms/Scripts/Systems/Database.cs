using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Utils;
using ArcaneRealms.Scripts.Utils.ScriptableDatabase;
using NaughtyAttributes;
using UnityEngine;

namespace ArcaneRealms.Scripts.Systems
{
    public class Database : Singleton<Database>
    {
        public CardInfoDataBase cardsDatabase;
        public CardEffectDataBaseSO cardEffectDatabase;

        private Dictionary<string, CardInfoSO> customCardsLoaded = new();
        private Dictionary<string, CardInfoSO> normalCardsLoaded = new();

        private void Start()
        {
            foreach (var card in cardsDatabase.Cards)
            {
                normalCardsLoaded[card.ID] = card;
            }
        }

        public void RegisterCustomCard(CardInfoSO customCard)
        {
            customCardsLoaded[customCard.ID] = customCard;
        }

        [Button("Clear Custom Card Data")]
        public void ClearCustomCardData()
        {
            customCardsLoaded.Clear();
        }

        public CardInfoSO GetCardFromId(string id)
        {
            if (customCardsLoaded.TryGetValue(id, out var card))
            {
                return card;
            }

            return normalCardsLoaded[id];
        }
        

    }
}