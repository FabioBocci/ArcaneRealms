using System;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using System.Collections.Generic;
using System.Linq;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace ArcaneRealms.Scripts.Players {

	//this class should only be used and edit by the game manager, other class should only be able to see what is inside but not chainging anything
	public class PlayerInGame : ITargetable {

		private const int MAX_MANA = 10;
		private const int MAX_CARDS_IN_HAND = 7;

		public Dictionary<Guid, CardInGame> allCardInGameDicionary = new();
		
		//immutable list of the card in the deck at the start of the game
		public List<CardInGame> startingDeck = new();

		//list of all the current cards in the deck
		public List<CardInGame> currentDeck = new();

		//list of all the card that has been played and gone to the graveyard //TODO - maybe keep the turn a card goes to the graveyard?
		public List<CardInGame> graveyardList = new();


		//list of all the card in the player hand
		public List<CardInGame> handCards = new();
		public int handCardsCounter = 0; //only used for remote player to know how many card it has

		//all the monster on the field of this player
		public List<MonsterCard> monsterCardOnField = new();
		//spell of type continue
		public List<SpellCard> continueSpellCards = new();
		//spell of type delayed
		public List<SpellCard> delayedSpellCards = new();



		public int currentManaPool = 0;
		public int usableMana = 0;

		public Guid ID { private set; get; }

		public ulong playerUlong { private set; get; }
		//also for teams
		
		public ClientRpcParams thisClientRpcTarget { get; }

		public PlayerInGame(Guid iD, ulong player) {
			ID = iD;
			playerUlong = player;
			thisClientRpcTarget = new ClientRpcParams()
			{
				Send = new ClientRpcSendParams()
				{
					TargetClientIds = new[] { player }
				}
			};
		}
		

		public void AddMana(int mana, bool permanent = true, bool onlyEmpty = false) {
			if(permanent) {
				currentManaPool = (currentManaPool + mana) % MAX_MANA;
			}
			if(!onlyEmpty) {
				usableMana = (usableMana + mana) % MAX_MANA;
			}
		}

		public void PayMana(int mana)
		{
			usableMana -= mana;
			usableMana = Mathf.Max(usableMana, 0);
		}
		
		

		public void DrawCards(int count) {

			for(int i = 0; i < count; i++) {
				CardInGame card = currentDeck.First();
				if(card == null) {
					//deck empty
					//TODO - run fatigue!
					continue;
				}
				if(handCards.Count >= MAX_CARDS_IN_HAND) {
					//Hand full of cards
					//TODO - burn this card!
					continue;
				}

				handCards.Add(card);

				//GameManager.Instance.PlayerDrawCardClientRPC(ID, card.cardInfoSO.ID, "TODO - handle json of card");
			}

		}


		//these functions will be called from the GameManager for helping keep the game state

		//when you drow a card
		public void AddCardToHand(CardInGame card) {
			handCards.Add(card);
		}

		public void RemoveCardFromHand(int index) {
			handCards.RemoveAt(index);
		}

		internal void RemoveCardFromDeck(string cardID) {
			if(currentDeck == null || currentDeck.Count == 0) {
				return;
			}
			CardInGame cardFromDeck = currentDeck.Find(card => card.cardInfoSO.ID == cardID);
			if(cardFromDeck == null) {
				return;
			}
			currentDeck.Remove(cardFromDeck);
		}



		public void PlayCard(CardInGame cardInPlay, int index = 0) {
			if(cardInPlay.IsMonsterCard(out MonsterCard monster)) {
				if(index >= monsterCardOnField.Count) {
					monsterCardOnField.Add(monster);
				} else {
					monsterCardOnField.Insert(index, monster);
				}
				cardInPlay.position = CardPosition.Field;
				return;
			}

			if(cardInPlay.IsSpellCard(out SpellCard spell)) {
				if(spell.cardInfoSO.SpellType == SpellType.Continue) {
					continueSpellCards.RemoveAll(card => card.cardInfoSO.ID == spell.cardInfoSO.ID);
					continueSpellCards.Add(spell);
					cardInPlay.position = CardPosition.Field;
				}
				if(spell.cardInfoSO.SpellType == SpellType.Delayed) {
					delayedSpellCards.RemoveAll(card => card.cardInfoSO.ID == spell.cardInfoSO.ID);
					delayedSpellCards.Add(spell);
					cardInPlay.position = CardPosition.Field;
				}
				return;
			}

			Debug.LogError("Tried to play a card that is not a Monster or a Spell?");

		}

		public CardInGame GetCardInGameFromGuid(Guid guid)
		{
			return allCardInGameDicionary[guid];
		}

		public Guid GetTeam() => ID;

		public TargetType GetTargetType() => TargetType.Player;

		public Guid GetUnique() => ID;
	}
}