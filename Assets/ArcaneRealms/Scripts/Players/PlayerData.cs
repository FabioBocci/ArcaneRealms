using System;
using System.Collections.Generic;
using System.Linq;
using ArcaneRealms.Scripts.SO;
using UnityEngine;

namespace ArcaneRealms.Scripts.Players
{
    [Serializable]
    public class PlayerData
    {
        public Guid deckSelected;

        public HashSet<int> sawCards = new();
        public List<CardCollection> collectionList = new();
        
        public List<DeckOfCards> decks = new();


        public DeckOfCards SelectedDeck => decks.Find(deck => deck.id == deckSelected);

        public void SetSelectedDeck(Guid newDeckSelected)
        {
            if (decks.All(deck => deck.id != newDeckSelected))
            {
                Debug.LogError("Didn't find deck for Guid: " + newDeckSelected);
                return;
            }

            deckSelected = newDeckSelected;
        }

        public void SawCard(int cardId)
        {
            sawCards.Add(cardId);
        }
        
        public void ObtainCard(int cardId, bool isSpecial = false)
        {
            SawCard(cardId);
            //make sure that we saw the card.

            CardCollection cc = collectionList.Find(cc => cc.cardId == cardId && cc.special == isSpecial);
            if (cc == null)
            {
                cc = new();
                cc.cardId = cardId;
                cc.special = isSpecial;
                collectionList.Add(cc);
            }

            cc.count++;
        }
        
    }

    [Serializable]
    public class DeckOfCards
    {
        public Guid id;
        public string name;
        public List<CardInDeck> cards = new();
    }

    [Serializable]
    public class CardInDeck
    {
        public CardInfoSO card;
        public int count;
    }

    [Serializable]
    public class CardCollection
    {
        public int cardId;
        public int count = 0;
        public bool special = false;
    }
}