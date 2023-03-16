using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Effects;
using ArcaneRealms.Scripts.Enums;
using Assets.Scripts.Cards;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneRealms.Scripts.SO {
	public class CardInfoSO : ScriptableObject {
		public string ID = "I-1";
		public CardRarity Rarity;
		public int ManaCost;
		public Sprite Artwork;

		public List<CardEffect> Effects;

		public string Name;
		public string Description;



		public bool IsSpell(out SpellInfoSO card) {
			if(this is SpellInfoSO sO)
				card = sO;
			else
				card = null;
			return this is SpellInfoSO;
		}

		public bool IsMonster(out MonsterInfoSO card) {
			if(this is MonsterInfoSO sO)
				card = sO;
			else
				card = null;
			return this is MonsterInfoSO;
		}

		public CardInGame BuildCardInGame(string statHandlerJson = null) {
			CardInGame cardInGame = null;
			if(IsMonster(out var monster)) {
				cardInGame = new MonsterCard(monster);
			}

			if(IsSpell(out var spell)) {
				cardInGame = new SpellCard(spell);
			}

			if(cardInGame != null && statHandlerJson != null) {
				cardInGame.SetStatHandlerFromJson(statHandlerJson);
			}

			return cardInGame;
		}

	}

}

