using System;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Managers;
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
        
        #endregion
        
        
    }
}