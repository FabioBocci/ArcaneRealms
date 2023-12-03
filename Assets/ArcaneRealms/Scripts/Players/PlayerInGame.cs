using System;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using System.Collections.Generic;
using System.Linq;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Interfaces;
using ArcaneRealms.Scripts.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

namespace ArcaneRealms.Scripts.Players {

	//this class should only be used and edit by the game manager, other class should only be able to see what is inside but not chainging anything
	public class PlayerInGame : IDamageable {

		private const int MAX_MANA = 10;
		private const int MAX_CARDS_IN_HAND = 7;

		public Dictionary<Guid, CardInGame> allCardInGameDictionary = new();
		
		//immutable list of the card in the deck at the start of the game
		public List<CardInGame> startingDeck = new();

		//list of all the current cards in the deck
		public List<CardInGame> currentDeck = new();

		//list of all the card that has been played and gone to the graveyard //TODO - maybe keep the turn a card goes to the graveyard?
		public List<CardInGame> graveyardList = new();


		//list of all the card in the player hand
		public List<CardInGame> handCards = new();

		//all the monster on the field of this player
		public List<MonsterCard> monsterCardOnField = new();
		//spell of type continue
		public List<SpellCard> continueSpellCards = new();
		//spell of type delayed
		public List<SpellCard> delayedSpellCards = new();



		public int currentManaPool = 0;
		public int usableMana = 0;
		public int damageReceived = 0;
		
		public Guid ID { private set; get; }

		public ulong playerUlong { private set; get; }
		//also for teams
		
		public ClientRpcParams thisClientRpcTarget { get; }

		public int CurrentUsableMana => usableMana;
		public int CurrentTotalMana => currentManaPool;
		
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
		
		public void PlayCard(CardInGame cardInPlay, int index = 0) {
			if (handCards.Contains(cardInPlay))
			{
				handCards.Remove(cardInPlay);
			}
			
			if(cardInPlay.IsMonsterCard(out MonsterCard monster)) {
				if(index >= monsterCardOnField.Count - 1) {
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
			return allCardInGameDictionary.TryGetValue(guid, out var value) ? value : null;
		}

		public Guid GetTeam() => ID;
		public Guid GetUnique() => ID;
		public TargetType GetTargetType() => TargetType.Player;

		public int GetHealth() => GetMaxHealth() - damageReceived;

		public int GetMaxHealth() => 40; //TODO - change this!

		public void Damage(int damage)
		{
			damageReceived += damage;
			damageReceived = Mathf.Min(damageReceived, GetMaxHealth());
		}

		public void Heal(int amount)
		{
			damageReceived -= amount;
			damageReceived = Mathf.Max(damageReceived, 0);
		}

		public void RemoveCard(CardInGame cardToRemove)
		{
			if (cardToRemove.IsMonsterCard(out var monster))
			{
				monsterCardOnField.Remove(monster);
			}

			if (cardToRemove.IsSpellCard(out var spell))
			{
				continueSpellCards.Remove(spell);
				delayedSpellCards.Remove(spell);
			}
			
			graveyardList.Add(cardToRemove);
			cardToRemove.position = CardPosition.Graveyard;
		}


		public PlayerState ToPlayerState()
		{
			PlayerState playState = new()
			{
				id = ID.ToString(),
				currentDeck = currentDeck.Select(c => c.GetUnique().ToString()).ToArray(),
				graveyard = graveyardList.Select(c => c.GetUnique().ToString()).ToArray(),
				handCards = handCards.Select(c => c.GetUnique().ToString()).ToArray(),
				monsterCardOnField = monsterCardOnField.Select(c => c.GetUnique().ToString()).ToArray(),
				continueSpellCards = continueSpellCards.Select(c => c.GetUnique().ToString()).ToArray(),
				delayedSpellCards = delayedSpellCards.Select(c => c.GetUnique().ToString()).ToArray(),
				currentManaPool = currentManaPool,
				usableMana = usableMana,
				damageReceived = damageReceived
			};

			return playState;
		}

		public void FromPlayerState(PlayerState state)
		{
			if (!ID.ToString().Equals(state.id))
			{
				return;
			}

			currentDeck = state.currentDeck.Select(c => allCardInGameDictionary[Guid.Parse(c)]).ToList();
			graveyardList = state.graveyard.Select(c => allCardInGameDictionary[Guid.Parse(c)]).ToList();
			handCards = state.handCards.Select(c => allCardInGameDictionary[Guid.Parse(c)]).ToList();
			monsterCardOnField = state.monsterCardOnField.Select(c => allCardInGameDictionary[Guid.Parse(c)] as MonsterCard).ToList();
			continueSpellCards = state.continueSpellCards.Select(c => allCardInGameDictionary[Guid.Parse(c)] as SpellCard).ToList();
			delayedSpellCards = state.delayedSpellCards.Select(c => allCardInGameDictionary[Guid.Parse(c)] as SpellCard).ToList();
			currentManaPool = state.currentManaPool;
			usableMana = state.usableMana;
			damageReceived = state.damageReceived;


		}
		
	}
}