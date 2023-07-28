using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.Utils;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.GameCards;
using UnityEngine;

namespace ArcaneRealms.Scripts.Effects.OnActivation {

	[CreateAssetMenu(fileName = "Gain Empty Mana Effect", menuName = "card effect/Gain Empty Mana")]
	public class GainEmptyManaEffect : OnActivationEffect {

		private static readonly string AMOUNT_KEY = "Amount";
		private static readonly string AMOUNT_VALUE = "0";

		private static readonly string PERMANENT_KEY = "permanent";
		private static readonly string PERMANENT_VALUE = true.ToString();

		private static readonly string EMPTY_KEY = "empty";
		private static readonly string EMPTY_VALUE = true.ToString();

		private static readonly string TARGET_DEFAULT = TargetsEnum.ALLY_PLAYERS.name;

		public override void OnActivation(PlayerInGame owner, CardInGame card, EffectParameters parameters) {
			TargetsEnum target = parameters.GetValueOrDefault(TARGET_PARAM_NAME, TargetsEnum.ALLY_PLAYERS);

			PlayerInGame targetPlayer = target == TargetsEnum.ALLY_PLAYERS ? owner : GameManager.Instance.GetEnemyPlayer(owner);

			int amount = parameters.GetValueOrDefault(AMOUNT_KEY, 0);
			bool permanent = parameters.GetValueOrDefault(PERMANENT_KEY, true);
			bool empty = parameters.GetValueOrDefault(EMPTY_KEY, true);


			if(GameManager.Instance.IsClient) {
				//run animation


			} else {
				//server only, just update values.
				targetPlayer.AddMana(amount, permanent, empty);
			}


		}


		public override EffectParameters GetDefaultValueDictionary() {
			return new EffectParameters(new List<Parameter>() {
				new Parameter() { Key = AMOUNT_KEY, Value = AMOUNT_VALUE, Type = INT },
				new Parameter() { Key = PERMANENT_KEY, Value = PERMANENT_VALUE, Type = BOOL },
				new Parameter() { Key = EMPTY_KEY, Value = EMPTY_VALUE, Type = BOOL },
				new Parameter() { Key = TARGET_PARAM_NAME, Value = TARGET_DEFAULT, Type = TARGET_TYPE }
			});
		}

		public override string GetDesc(EffectParameters parameters) {


			return "Gain mana";
		}


	}
}