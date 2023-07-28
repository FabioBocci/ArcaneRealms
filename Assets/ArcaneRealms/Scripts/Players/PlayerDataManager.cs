using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Utils;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

namespace ArcaneRealms.Scripts.Players
{
    public class PlayerDataManager : Singleton<PlayerDataManager>
    {
        [SerializeField] private string loadJsonInfo;
        
        public PlayerData playerData = new();
        
        protected override void Awake()
        {
            if (!EnsureInstance())
            {
                return;
            }

        }

        private void Start()
        {
            LoadPlayerDataAsync();
        }

        private async void LoadPlayerDataAsync()
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                Dictionary<string, string> savedData =
                    await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string>() {"PlayerData" });
                Debug.Log($"PlayerData: {savedData.GetValueOrDefault("PlayerData", "EMPTY")}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        [ContextMenu("Save")]
        public void Save()
        {
            string jsonString = JsonUtility.ToJson(playerData);
            
            Debug.Log($"[PlayerData] json: {jsonString}");
            SaveAsync(jsonString);
        }

        [ContextMenu("Load TEMP data")]
        public void Load()
        {
            LoadJsonData(loadJsonInfo);
        }

        private void LoadJsonData(string jsonData)
        {
            playerData = JsonUtility.FromJson<PlayerData>(jsonData);
        }

        private async void SaveAsync(string jsonData)
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }
                
                Dictionary<string, object> saveData = new Dictionary<string, object>();
                saveData["PlayerData"] = jsonData;
                await CloudSaveService.Instance.Data.ForceSaveAsync(saveData);
                Debug.Log($"[PlayerData] Saved");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}