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


        [ContextMenu("Save")]
        public void Save()
        {
            string jsonString = JsonUtility.ToJson(playerData);
            
            Debug.Log($"[PlayerData] json: {jsonString}");
        }
    }
}