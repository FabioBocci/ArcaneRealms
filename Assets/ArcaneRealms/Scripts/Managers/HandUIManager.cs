using System;
using System.Collections.Generic;
using System.Linq;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.UI;
using ArcaneRealms.Scripts.Utils;
using ArcaneRealms.Scripts.Utils.Events;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace ArcaneRealms.Scripts.Managers {

	/// <summary>
	/// This class is for client only, it will manage the physic implementation of the card in the ui space (hands) <br></br>
	/// It will also handle these event called from the local GameManager:
	/// <list type="bullet">
	/// <item> <see cref="OnManaChangeEvent"> OnTurnChange </see> <description> ...... </description></item>
	/// <item> <see cref="OnPlayerDrawCard"> OnCardDraw </see> <description>....... </description></item>
	/// <item> <see cref="OnManaChangeEvent"> OnManaChange </see> <description> ....... </description></item>
	/// <item> <see cref="..."> OnPlayCard </see> <description> ....... </description></item>
	/// <item> <see cref="OnEnemyHoverEvent"> OnEnemyHovering </see> <description> ....... </description></item>
	/// </list>
	/// </summary>
	public class HandUIManager : Singleton<HandUIManager> {

		[Header("Parents")]
		[SerializeField] RectTransform chooseCardTransform;
		[SerializeField] RectTransform playerHandTransform;
		[SerializeField] RectTransform enemyHandTransform;

		[Header("Card prefab")]
		[SerializeField] RectTransform playerCardPrefab_Monster;
		[SerializeField] RectTransform playerCardPrefab_Spell;
		[SerializeField] RectTransform emptyCardPrefab_back;
		
		[Header("Screen Position")]
		[SerializeField] RectTransform outOfScreenRight;
		[SerializeField] RectTransform[] centerPositions;

		[Header("Buttons")]
		[SerializeField] Button confirmStartingHand;
		
		[HideInInspector]
		public bool PlayerIsDraggingCard = false;
		[HideInInspector]
		public CardInHandHandlerUI CardInHandHandler = null;
		[HideInInspector]
		public EnemyCardInHandUI enemyCardInHandHighlight = null; // keep reference of which EnemyCard is highlighting now


		private List<CardInHandHandlerUI> choosedCards = new();
		private List<CardInHandHandlerUI> removedChoosedCards = new();


		private void Start()
		{
			confirmStartingHand.onClick.AddListener(() =>
			{
				GameManager.Instance.PlayerChooseCards(choosedCards.Select(c => c.GetCardInGame()).ToList(),
					removedChoosedCards.Select(c => c.GetCardInGame()).ToList());
				confirmStartingHand.gameObject.SetActive(false);
			});
		}

		public void OnStartingCardsReceived(List<CardInGame> cards)
		{

			int i = 0;
			for (int j = 0; j < centerPositions.Length; j++)
			{
				centerPositions[j].gameObject.SetActive(j < cards.Count);
			}
			
			
			foreach (var card in cards)
			{
				CardInHandHandlerUI cardInHand = SpawnNewCard(card);
				if (cardInHand == null)
				{
					return;
				}
				cardInHand.transform.SetParent(chooseCardTransform);
				cardInHand.gameObject.AddComponent<CardChoosingHandlerUi>();
				cardInHand.rectTransform.position = outOfScreenRight.position;
				choosedCards.Add(cardInHand);
				LeanTween.delayedCall(i * 0.1f + 0.05f, () =>
				{
					int d = i++;
					LeanTween.move(cardInHand.gameObject, centerPositions[d].position, 0.25f + 0.05f * d).setOnComplete(
						() =>
						{
							//Debug.LogWarning($"Distance: {Vector3.Distance(cardInHand.rectTransform.position, centerPositions[d].position)}");
						});
				});
			}
		}

		public void FinalStartingCardsReceived(List<CardInGame> oldCards, List<CardInGame> newCards)
		{
			int i = 0;
			List<CardInHandHandlerUI> cards = new();
			foreach (var card in newCards)
			{
				CardInHandHandlerUI cardInHand = removedChoosedCards[0];
				removedChoosedCards.Remove(cardInHand);
				CardChoosingHandlerUi removed = cardInHand.gameObject.GetComponent<CardChoosingHandlerUi>();
				Destroy(removed);
				cardInHand.BuildCard(card);
				cards.Add(cardInHand);
			}

			foreach (CardInHandHandlerUI card in choosedCards)
			{
				CardChoosingHandlerUi removed = card.gameObject.GetComponent<CardChoosingHandlerUi>();
				Destroy(removed);
				LeanTween.move(card.gameObject, centerPositions[i++].position, 0.25f);
			}

			foreach (var outOfScreen in cards)
			{
				LeanTween.move(outOfScreen.gameObject, centerPositions[i++].position, 0.4f);
			}

			choosedCards.AddRange(cards);
			
			LeanTween.delayedCall(0.4f, () =>
			{
				foreach (var card in choosedCards)
				{
					card.transform.SetParent(playerHandTransform);
				}
			});
			
			LeanTween.delayedCall(0.45f, () =>
			{
				Debug.LogError("ALL DONE?");
			});

		}

		public void CardChoosingTriggerClick(CardInHandHandlerUI card)
		{
			if (choosedCards.Contains(card))
			{
				choosedCards.Remove(card);
				removedChoosedCards.Add(card);
				return;
			}

			if (removedChoosedCards.Contains(card))
			{
				choosedCards.Add(card);
				removedChoosedCards.Remove(card);
			}
		}
		
		
		#region Utils

		private CardInHandHandlerUI SpawnNewCard(CardInGame card)
		{
			CardInHandHandlerUI cardInHand = null;
			if (card.IsMonsterCard(out MonsterCard monster))
			{
				cardInHand = Instantiate(playerCardPrefab_Monster, transform).GetComponent<CardInHandHandlerUI>();
			}

			if (card.IsSpellCard(out SpellCard spell))
			{
				cardInHand = Instantiate(playerCardPrefab_Spell, transform).GetComponent<CardInHandHandlerUI>();
			}

			if (cardInHand == null)
			{
				Debug.LogError($"Trying to spawn a CardInHand that is not a monster or a spell! NOT SUPPORTED TYPE inside HandUiManager.cs");
				return null;
			}
			cardInHand.BuildCard(card);
			return cardInHand;
		}
		

		#endregion

		public void UpdateCurrentStateImmediately()
		{
			//aggiorno tutte le carte in mano ed eventuali altri segnali
		}
	}
}