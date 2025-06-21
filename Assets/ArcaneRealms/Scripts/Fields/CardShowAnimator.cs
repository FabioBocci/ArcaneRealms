#region

using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Utils;
using UnityEngine;

#endregion

namespace ArcaneRealms.Scripts.Fields
{
    public class CardShowAnimator : Singleton<CardShowAnimator>
    {
        [SerializeField] private float startingScale = 0.1f;
        [SerializeField] private float endingScale = 2f;
        [SerializeField] private Transform allyStartingPoint;
        [SerializeField] private Transform allyEndPoint;
        [SerializeField] private Transform enemyStartingPoint;
        [SerializeField] private Transform enemyEndPoint;


        [SerializeField] private Transform cardToShow;

        //[SerializeField] private <CustomVisualCard> frontCardToShow;
        //[SerializeField] private Image backCardToShow;


        public async void RunEnemyAnimation(CardInGame cardInGame)
        {
            cardToShow.gameObject.SetActive(true);
            cardToShow.position = enemyStartingPoint.position;
            cardToShow.rotation = enemyStartingPoint.rotation;
            cardToShow.localScale = Vector3.one * startingScale;

            //LeanTween.move(cardToShow,)
        }
    }
}