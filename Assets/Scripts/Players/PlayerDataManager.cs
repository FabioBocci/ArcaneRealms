using ArcaneRealms.Scripts.Utils;
using UnityEngine;

namespace ArcaneRealms.Scripts.Players
{
    public class PlayerDataManager : Singleton<PlayerDataManager>
    {
        public PlayerData playerData = new();
        
        protected override void Awake()
        {
            if (!EnsureInstance())
            {
                return;
            }
            
            //load playerdata from remote data.
        }
    }
}