using System;
using System.Collections.Generic;
using System.Text;
using ArcaneRealms.Scripts.Cards.Effects;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Utils.ScriptableDatabase;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Serialization;

namespace ArcaneRealms.Scripts.Cards.ScriptableCards {
	public class CardInfoSO : ScriptableObject {
		private static CardInfoDataBase DatabaseStatic;
		//private static StringBuilder TableKeyBuilder = new StringBuilder("C{0}-D");

		[SerializeField]
		[HideInInspector]
		private CardInfoDataBase database;

		public string ID = "I-1";
		public CardRarity rarity;
		public int manaCost;
		public Sprite artwork;

		[HideInInspector]
		public List<CardEffect> effects = new();

		[SerializeField] private string cardName;
		[SerializeField] private string description;
		
		public string Name => cardName;
		public string Description => description;


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

			/*StringTable table = LocalizationSettings.Instance.GetStringDatabase().GetTable("CardTable");
			if (table == null)
			{
				Debug.Log("Could not find table");
				return;
			}

			if (ID.StartsWith("I"))
			{
				return;
			}

			string key = TableKeyBuilder.Replace("{0}", ID).ToString();
			
			StringTableEntry entry = table.GetEntry(key);
			if (entry == null)
			{
				table.AddEntry(key, "TEMP DESC");
			}*/

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

