using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Utils;
using NaughtyAttributes;
using Newtonsoft.Json;
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

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string>() {"PlayerData" });
                Debug.Log($"PlayerData: {savedData.GetValueOrDefault("PlayerData", "EMPTY")}");
                if (!string.IsNullOrEmpty(savedData["PlayerData"]))
                {
                    LoadJsonData(savedData["PlayerData"]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        [Button("Save")]
        public void Save()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new CardInDeckConverter());
            string jsonString = JsonConvert.SerializeObject(playerData, settings);
            
            Debug.Log($"[PlayerData] json: {jsonString}");
            SaveAsync(jsonString);
        }

        [Button("Load TEMP data")]
        public void Load()
        {
            LoadJsonData(loadJsonInfo);
        }

        private void LoadJsonData(string jsonData)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new CardInDeckConverter());
            playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData, settings);
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