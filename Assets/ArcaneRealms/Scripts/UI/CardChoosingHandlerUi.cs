using System;
using ArcaneRealms.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcaneRealms.Scripts.UI
{
    public class CardChoosingHandlerUi : MonoBehaviour, IPointerClickHandler
    {
        private CardInHandHandlerUI cardInHand;
        
        private void Awake()
        {
            cardInHand ??= GetComponent<CardInHandHandlerUI>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            cardInHand.Outline.enabled = !cardInHand.Outline.enabled;
            HandUIManager.Instance.CardChoosingTriggerClick(cardInHand);
        }
    }
}