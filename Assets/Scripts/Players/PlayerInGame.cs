﻿using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Managers;
using Assets.Scripts.Cards;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneRealms.Scripts.Players {

	//this class should only be used and edit by the game manager, other class should only be able to see what is inside but not chainging anything
	public class PlayerInGame {

		private const int MAX_MANA = 10;
		private const int MAX_CARDS_IN_HAND = 7;

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

		public readonly ulong ID;

		public PlayerInGame(ulong iD) {
			ID = iD;
		}

		//these function will call the GameManager to sync the players to this state!

		public void AddMana(int amount) {
			AddMana(amount, true);
		}

		public void AddMana(int mana, bool permanent) {
			if(permanent) {
				currentManaPool = (currentManaPool + mana) % MAX_MANA;
			}
			usableMana = (usableMana + mana) % MAX_MANA;

			GameManager.Instance.PlayerManaChangedClientRPC(ID, currentManaPool, usableMana);
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

				GameManager.Instance.PlayerDrawCardClientRPC(ID, card.cardInfoSO.ID, "TODO - handle json of card");
			}

		}


		//these functions will be called from the GameManager for helping keep the game state

		//when you drow a card
		public void AddCardToHand(CardInGame card) {
			handCards.Add(card);
		}

		//when the enemy draw a card
		public void AddCardInHandCount() {
			handCardsCounter++;
		}

		public void RemoveCardInHandCount() {
			handCardsCounter--;
		}

		public void PlayCard(CardInGame cardInPlay, int index = 0) {
			if(cardInPlay.IsMonsterCard(out MonsterCard monster)) {
				if(index >= monsterCardOnField.Count) {
					monsterCardOnField.Add(monster);
				} else {
					monsterCardOnField.Insert(index, monster);
				}
				return;
			}

			if(cardInPlay.IsSpellCard(out SpellCard spell)) {
				if(spell.cardInfoSO.SpellType == SpellType.Continue) {
					continueSpellCards.RemoveAll(card => card.cardInfoSO.ID == spell.cardInfoSO.ID);
					continueSpellCards.Add(spell);
				}
				if(spell.cardInfoSO.SpellType == SpellType.Delayed) {
					delayedSpellCards.RemoveAll(card => card.cardInfoSO.ID == spell.cardInfoSO.ID);
					delayedSpellCards.Add(spell);
				}
				return;
			}

			Debug.LogError("Tryied to play a card that is not a Monster or a Spell?");

		}
	}
}