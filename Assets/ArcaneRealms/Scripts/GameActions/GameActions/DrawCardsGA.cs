namespace ArcaneRealms.Scripts.GameActions.GameActions
{
    public class DrawCardsGA : GameAction
    {
        public int numberOfCards;

        public DrawCardsGA(string playerId, int numberOfCards)
        {
            this.playerId = playerId;
            this.numberOfCards = numberOfCards;
            readableActionName = "Draw " + numberOfCards + " cards";
        }
    }
}
