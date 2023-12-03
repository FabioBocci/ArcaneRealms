using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.Players;

namespace ArcaneRealms.Scripts.GameEvents
{
    public class PlayCardEvent : GameEvent
    {

        private readonly PlayerInGame player;
        private readonly CardInGame card;
        private readonly int destPos;

        private string jsonState;
        
        public PlayCardEvent(PlayerInGame playerInGame, CardInGame cardInGame, int posDest = -1)
        {
            player = playerInGame;
            card = cardInGame;
            destPos = posDest;
        }
        
        public override void Undo()
        {
            GameManager.Instance.SetJsonCurrentState(jsonState);
        }

        public override void Do()
        {
            jsonState = GameManager.Instance.GetJsonCurrentState();
            GameManager.Instance.HandlePlayerPlayCardLocally(player, card, destPos);
        }
    }
}