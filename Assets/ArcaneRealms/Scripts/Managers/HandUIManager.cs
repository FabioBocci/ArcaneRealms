using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.UI;
using ArcaneRealms.Scripts.Utils;
using ArcaneRealms.Scripts.Utils.Events;
using Unity.Netcode;
using UnityEngine;

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

		[SerializeField] RectTransform playerHandTransform;
		[SerializeField] RectTransform enemyHandTransform;

		[Header("Card prefab")]
		[SerializeField] RectTransform playerCardPrefab_Monster;
		[SerializeField] RectTransform playerCardPrefab_Spell;
		[SerializeField] RectTransform emptyCardPrefab_back;
		
		[Header("Screen Position")]
		[SerializeField] RectTransform outOfScreenRight;
		[SerializeField] RectTransform[] centerPositions;
		

		[HideInInspector]
		public bool PlayerIsDraggingCard = false;
		[HideInInspector]
		public CardInHandHandlerUI CardInHandHandler = null;
		[HideInInspector]
		public EnemyCardInHandUI enemyCardInHandHighlight = null; // keep reference of which EnemyCard is highlighting now


		private void Start()
		{
			GameManager.Instance.OnStartingCardsReceived += OnStartingCardsReceived;
		}

		private void OnDisable()
		{
			GameManager.Instance.OnStartingCardsReceived -= OnStartingCardsReceived;
		}

		private void OnStartingCardsReceived(ref EntityEventData<List<CardInGame>> entityeventdata)
		{
			List<CardInGame> cards = entityeventdata.Entity;

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

				cardInHand.rectTransform.position = outOfScreenRight.position;
				LeanTween.delayedCall(i * 0.1f + 0.05f, () =>
				{
					LeanTween.move(cardInHand.rectTransform, centerPositions[i++].position, 0.25f + 0.05f * i).setOnComplete(
						() =>
						{
							Debug.Log($"Distance: {Vector3.Distance(cardInHand.rectTransform.position, centerPositions[i++].position)}");
						});
				});
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
	}
}