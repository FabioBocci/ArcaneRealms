using ArcaneRealms.Scripts.SO;
using ArcaneRealms.Scripts.Utils;
using UnityEngine;

namespace ArcaneRealms.Scripts.Systems
{
    public class Database : Singleton<Database>
    {
        public CardInfoDataBase cardsDatabase;
        public CardEffectDataBaseSO cardEffectDatabase;

    }
}