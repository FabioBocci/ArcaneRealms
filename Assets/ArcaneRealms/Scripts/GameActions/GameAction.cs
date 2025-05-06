#region

using System;
using System.Collections.Generic;

#endregion

namespace ArcaneRealms.Scripts.GameActions
{
    [Serializable]
    public class GameAction
    {
        public List<GameAction> preReactions = new();
        public List<GameAction> performReactions = new();
        public List<GameAction> postReactions = new();

        public string playerId;
        public string actionId = Guid.NewGuid().ToString();
        public string readableActionName;
        public long timeStamp = DateTime.UtcNow.Ticks;


        public bool isCancelled;
    }
}