using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.Effects;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Utils.ScriptableDatabase;
using UnityEngine;

namespace ArcaneRealms.Scripts.Cards.ScriptableCards {
	public class CardInfoSO : ScriptableObject {
		private static CardInfoDataBase DatabaseStatic;

		[SerializeField]
		[HideInInspector]
		private CardInfoDataBase database;

		public string ID = "I-1";
		public CardRarity Rarity;
		public int ManaCost;
		public Sprite Artwork;

		[HideInInspector]
		public List<CardEffect> Effects = new();

		public string Name;
		public string Description;


		private void OnValidate() {
			if(database != null && DatabaseStatic == null) {
				DatabaseStatic = database;
			}

			if(database == null && DatabaseStatic != null) {
				database = DatabaseStatic;
			}

			if(DatabaseStatic != null) {
				if(!DatabaseStatic.Cards.Contains(this)) {
					DatabaseStatic.Cards.Add(this);
				}
			}
		}


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

		public CardInGame BuildCardInGame(Guid team, Guid cardGuid, string statHandlerJson = null) {
			CardInGame cardInGame = null;
			if(IsMonster(out var monster)) {
				cardInGame = new MonsterCard(monster, cardGuid, team);
			}

			if(IsSpell(out var spell)) {
				cardInGame = new SpellCard(spell, cardGuid, team);
			}

			cardInGame.Start();

			if(cardInGame != null && statHandlerJson != null) {
				cardInGame.SetStatHandlerFromJson(statHandlerJson);
			}

			return cardInGame;
		}

	}

}

