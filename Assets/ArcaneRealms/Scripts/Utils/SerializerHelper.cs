using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.Systems;
using Unity.Netcode;

namespace ArcaneRealms.Scripts.Utils
{
    public static class SerializerHelper
    {

        #region GuidSerialization
        public static void ReadValueSafe(this FastBufferReader reader, out Guid guid)
        {
            reader.ReadValueSafe(out string val);
            guid = Guid.Parse(val);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in Guid guid)
        {
            writer.WriteValueSafe(guid.ToString());
        }
        
        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref Guid guid) where TReaderWriter: IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out guid);
            }

            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(guid);
            }
        }
        
        #endregion
        
        #region CardInGameSerialization
        public static void ReadValueSafe(this FastBufferReader reader, out CardInGame card)
        {
            reader.ReadValueSafe(out Guid cardGuid);
            card = GameManager.Instance.GetCardFromGuid(cardGuid);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in CardInGame card)
        {
            writer.WriteValueSafe(card.CardGuid);
        }
        
        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref CardInGame card) where TReaderWriter: IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out card);
            }

            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(card);
            }
        }
        
        public static void ReadValueSafe(this FastBufferReader reader, out List<CardInGame> cards)
        {
            cards = new List<CardInGame>();
            reader.ReadValueSafe(out int count);
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out Guid cardGuid);
                var card = GameManager.Instance.GetCardFromGuid(cardGuid);
                cards.Add(card);
            }
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in List<CardInGame> cards)
        {
            writer.WriteValueSafe(cards.Count);
            foreach (CardInGame card in cards)
            {
                writer.WriteValueSafe(card.CardGuid);
            }
        }
        
        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref List<CardInGame> cards) where TReaderWriter: IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out cards);
            }

            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(cards);
            }
        }
        
        
        #endregion

        #region CardInfoSoSerialization

        public static void ReadValueSafe(this FastBufferReader reader, out CardInfoSO card)
        {
            reader.ReadValueSafe(out string cardId);
            card = Database.Instance.GetCardFromId(cardId);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in CardInfoSO card)
        {
            writer.WriteValueSafe(card.ID);
        }
        
        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref CardInfoSO card) where TReaderWriter: IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out card);
            }

            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(card);
            }
        }
        
        #endregion
        
        #region CardInDeckSerialization

        public static void ReadValueSafe(this FastBufferReader reader, out CardInDeck card)
        {
            reader.ReadValueSafe(out CardInfoSO cardSo);
            reader.ReadValueSafe(out int count);
            card = new CardInDeck()
            {
                card = cardSo,
                count = count
            };
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in CardInDeck card)
        {
            writer.WriteValueSafe(card.card);
            writer.WriteValueSafe(card.count);
        }
        
        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref CardInDeck card) where TReaderWriter: IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out card);
            }

            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(card);
            }
        }
        
        public static void ReadValueSafe(this FastBufferReader reader, out List<CardInDeck> cards)
        {
            cards = new();
            reader.ReadValueSafe(out int size);
            for (int i = 0; i < size; i++)
            {
                reader.ReadValueSafe(out CardInDeck card);
                cards.Add(card);
            }
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in List<CardInDeck> cards)
        {
            writer.WriteValueSafe(cards.Count);
            foreach (var card in cards)
            {
                writer.WriteValueSafe(card);
            }
        }
        
        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref List<CardInDeck> cards) where TReaderWriter: IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out cards);
            }

            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(in cards);
            }
        }

        #endregion

        #region PlayerInGameSerialization
        
        public static void ReadValueSafe(this FastBufferReader reader, out PlayerInGame player)
        {
            reader.ReadValueSafe(out Guid playerGuid);
            player = GameManager.Instance.GetPlayerFromID(playerGuid);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in PlayerInGame player)
        {
            writer.WriteValueSafe(player.ID);
        }
        
        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref PlayerInGame player) where TReaderWriter: IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out player);
            }

            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(player);
            }
        }


        #endregion
        
    }
}