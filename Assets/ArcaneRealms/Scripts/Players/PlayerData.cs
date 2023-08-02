using System;
using System.Collections.Generic;
using System.Linq;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Systems;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Object = System.Object;

namespace ArcaneRealms.Scripts.Players
{
    [Serializable]
    public class PlayerData
    {
        [JsonIgnore]
        public Guid deckSelected;

        public List<int> sawCards = new();
        public List<CardCollection> collectionList = new();
        
        public List<DeckOfCards> decks = new();


        [JsonIgnore]
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
            if (!sawCards.Contains(cardId))
            {
                sawCards.Add(cardId);
            }
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
        public Guid id = Guid.NewGuid();
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

    public class CardInDeckConverter : JsonConverter<CardInDeck>
    {
        public override void WriteJson(JsonWriter writer, CardInDeck value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            // Write card ID and count
            writer.WritePropertyName("cardId");
            writer.WriteValue(value.card.ID);
        
            writer.WritePropertyName("count");
            writer.WriteValue(value.count);

            writer.WriteEndObject();
        }

        public override CardInDeck ReadJson(JsonReader reader, Type objectType, CardInDeck existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            // Read card ID and count from JSON
            string cardId = jsonObject["cardId"].Value<string>();
            int count = jsonObject["count"].Value<int>();

            // Construct and return a new CardInDeck instance
            CardInfoSO card = Database.Instance.GetCardFromId(cardId);
            return new CardInDeck { card = card, count = count };
        }
    }
}