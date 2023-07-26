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
}