using System;
using ArcaneRealms.Scripts.Effects;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Interfaces;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.SO;
using Assets.Scripts.Cards;
using System.Collections.Generic;

namespace ArcaneRealms.Scripts.Cards {
	public abstract class CardInGame : ITargetable {
		public CardInfoSO cardInfoSO;

		public StatHandler statHandler;

		public ulong cardTeam;
		
		public Guid CardGuid { private set; get; }

		private List<CardEffect> OnActivationCardEffects = new(); //TODO - add other effects list

		protected CardInGame(CardInfoSO cardInfo, ulong team) {
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

		public ulong GetTeam() {
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

	}

}
