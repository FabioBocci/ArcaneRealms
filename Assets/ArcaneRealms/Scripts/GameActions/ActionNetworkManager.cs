#region

using System.Collections.Generic;
using ArcaneRealms.Scripts.GameActions.GameActions;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;

#endregion

namespace ArcaneRealms.Scripts.GameActions
{
    public class ActionNetworkManager : MonoBehaviour
    {
        private readonly HashSet<string> receivedActionIds = new(); // per evitare doppioni

        public void SendAction(GameAction action)
        {
            var json = JsonConvert.SerializeObject(action,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            //usiamo netcode per inviare l'azione al server
            //NetworkTransport.SendToServer(json);
            Debug.Log("Sending action: " + json);
            ReceiveAction(json);
        }

        public void ReceiveAction(string json)
        {
            var action = JsonConvert.DeserializeObject<GameAction>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            if (receivedActionIds.Contains(action.actionId)) return;

            receivedActionIds.Add(action.actionId);

            Debug.Log("Received action: " + action.readableActionName + " from player: " + action.playerId);

            //ActionSystem.Instance.Perform(action); // esegui localmente
        }

        [Button("Test ActionNetworkManager")]
        public void TestFunction()
        {
            // Test function to check if the ActionNetworkManager is working
            Debug.Log("ActionNetworkManager is working!");

            var action = new DrawCardsGA("Player1", 3);
            SendAction(action);
        }
    }
}