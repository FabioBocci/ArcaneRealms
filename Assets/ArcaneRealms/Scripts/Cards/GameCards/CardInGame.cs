using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.Effects;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Interfaces;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.Players;
using Unity.VisualScripting;

namespace ArcaneRealms.Scripts.Cards.GameCards {
	public abstract class CardInGame : ITargetable, IEquatable<CardInGame> {
		public CardInfoSO cardInfoSO;

		public StatHandler statHandler;

		public Guid cardTeam;
		
		public Guid CardGuid { private set; get; }

		private List<CardEffect> OnActivationCardEffects = new(); //TODO - add other effects list

		protected CardInGame(CardInfoSO cardInfo, Guid cardGuid, Guid team)
		{
			CardGuid = cardGuid;
			cardInfoSO = cardInfo;
			cardTeam = team;

			foreach(CardEffect effect in cardInfoSO.Effects) {
				if(effect.HasActivationEffect()) {
					OnActivationCardEffects.Add(effect);
				}
			}
		}

		public virtual void Start() {

		}


		public bool IsMonsterCard(out MonsterCard monster) {
			if(this is MonsterCard monsterCard) {
				monster = monsterCard;
				return true;
			}
			monster = null;
			return false;
		}

		public bool IsSpellCard(out SpellCard spell) {
			if(this is SpellCard spellCard) {
				spell = spellCard;
				return true;
			}
			spell = null;
			return false;
		}

		public int GetManaCost() {
			return statHandler.GetModifiedStat(StatType.ManaCost);
		}

		public string GetJsonStatHandler() {
			return statHandler.ToJson();
		}

		public void SetStatHandlerFromJson(string jsonStatHandler) {
			statHandler.FromJson(jsonStatHandler);
		}

		public Guid GetTeam() {
			return cardTeam;
		}

		public TargetType GetTargetType() {
			if(IsMonsterCard(out var monster)) {
				return TargetType.Monster_card;
			}

			if(IsSpellCard(out var spellCard)) {
				switch(spellCard.cardInfoSO.SpellType) {
					case SpellType.Normal:
						return TargetType.Spell_normal_card;
					case SpellType.Continue:
						return TargetType.Spell_continue_card;
					case SpellType.Delayed:
						return TargetType.Spell_delayed_card;
				}
			}

			return TargetType.Player;
		}


		public bool HasTargetingEffects() {
			foreach(CardEffect effect in cardInfoSO.Effects) {
				if(effect.RequireTargetToRun()) {
					return true;
				}
			}
			return false;
		}

		public void RunOnActivationEffect() {
			if(!GameManager.Instance.IsServer) {
				return;
			}

			PlayerInGame owner = GameManager.Instance.GetPlayerFromID(GetTeam());

			foreach(CardEffect effect in OnActivationCardEffects) {
				effect.OnActivation(owner, this);
			}

		}


		public static bool operator ==(CardInGame card1, CardInGame card2)
		{
			if (ReferenceEquals(card1, card2)) 
				return true;
			if (ReferenceEquals(card1, null)) 
				return false;
			if (ReferenceEquals(card2, null))
				return false;
			return card1.Equals(card2);
		}

		public static bool operator !=(CardInGame card1, CardInGame card2) => !(card1 == card2);

		public bool Equals(CardInGame other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return CardGuid.Equals(other.CardGuid);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((CardInGame)obj);
		}

		public override int GetHashCode()
		{
			return CardGuid.GetHashCode();
		}
	}

}
