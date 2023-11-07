using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Utils;
using NaughtyAttributes;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
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
                Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() {"PlayerData" });
                Debug.Log($"PlayerData: {savedData["PlayerData"].Value.GetAsString()}");
                if (savedData["PlayerData"]?.Value != null)
                {
                    LoadJsonData(savedData["PlayerData"].Value.GetAsString());
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
                await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
                Debug.Log($"[PlayerData] Saved");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}