using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.Effects;

namespace ArcaneRealms.Scripts.Utils {
	public static class TypeUtils {
		public static T GetValueOrDefault<T>(this EffectParameters info, string key, T defaultValue) {
			string value = info.GetParameterStringValue(key);

			switch(defaultValue) {
				case int:
					int newValue;
					if(int.TryParse(value, out newValue)) {
						return (T) Convert.ChangeType(newValue, typeof(T));
					} else {
						return defaultValue;
					}
				case float:
					float newFloatValue;
					if(float.TryParse(value, out newFloatValue)) {
						return (T) Convert.ChangeType(newFloatValue, typeof(T));
					} else {
						return defaultValue;
					}
				case string:
					if(value == null || value.Length == 0) {
						return (T) Convert.ChangeType(value, typeof(T));
					} else {
						return defaultValue;
					}
				case bool:
					bool newBoolValue;
					if(bool.TryParse(value, out newBoolValue)) {
						return (T) Convert.ChangeType(newBoolValue, typeof(T));
					} else {
						return defaultValue;
					}
				case TargetsEnum:
					TargetsEnum newTargetTypeValue;
					if(TargetsEnum.TryParse(value, out newTargetTypeValue)) {
						return (T) Convert.ChangeType(newTargetTypeValue, typeof(T));
					} else {
						return defaultValue;
					}
				// add more cases for other types as needed
				default:
					throw new ArgumentException($"Type {typeof(T)} not supported");
			}

			throw new ArgumentException($"Type {typeof(T)} not supported");
		}

		public static bool IsTargetable<T>(this TargetsEnum type, T start, T possibleTarget) where T : ITargetable {
			if(type == TargetsEnum.NONE) {
				throw new ArgumentException("type == NONE");
			}

			if(type == TargetsEnum.ALL) {
				return true;
			}

			if(type == TargetsEnum.SELF) {
				return start.Equals(possibleTarget);
			}

			Guid teamStart = start.GetTeam();
			Guid teamTarget = possibleTarget.GetTeam();
			bool sameTeam = teamStart == teamTarget;

			switch(type.name) {
				case var value when value == TargetsEnum.ALL_PLAYERS.name || value == TargetsEnum.RANDOM_PLAYERS.name:
					return possibleTarget.GetTargetType() == TargetType.Player;
				case var value when value == TargetsEnum.ALL_CARDS.name || value == TargetsEnum.RANDOM_CARDS.name:
					return possibleTarget.GetTargetType() != TargetType.Player;
				case var value when value == TargetsEnum.ALL_MONSTERS.name || value == TargetsEnum.RANDOM_MONSTERS.name:
					return possibleTarget.GetTargetType() == TargetType.Monster_card;
				case var value when value == TargetsEnum.ALL_SPELLS.name || value == TargetsEnum.RANDOM_SPELLS.name:
					return possibleTarget.GetTargetType() == TargetType.Spell_continue_card || possibleTarget.GetTargetType() == TargetType.Spell_delayed_card || possibleTarget.GetTargetType() == TargetType.Spell_normal_card;

				case var value when value == TargetsEnum.ALLY_CARDS.name:
					return possibleTarget.GetTargetType() != TargetType.Player && sameTeam;
				case var value when value == TargetsEnum.ALLY_MONSTERS.name:
					return possibleTarget.GetTargetType() == TargetType.Monster_card && sameTeam;
				case var value when value == TargetsEnum.ALLY_PLAYERS.name:
					return possibleTarget.GetTargetType() == TargetType.Player && sameTeam;
				case var value when value == TargetsEnum.ALLY_SPELLS.name:
					return (possibleTarget.GetTargetType() == TargetType.Spell_continue_card || possibleTarget.GetTargetType() == TargetType.Spell_delayed_card || possibleTarget.GetTargetType() == TargetType.Spell_normal_card) && sameTeam;


				case var value when value == TargetsEnum.ENEMY_CARDS.name:
					return possibleTarget.GetTargetType() != TargetType.Player && !sameTeam;
				case var value when value == TargetsEnum.ENEMY_MONSTERS.name:
					return possibleTarget.GetTargetType() == TargetType.Monster_card && !sameTeam;
				case var value when value == TargetsEnum.ENEMY_PLAYERS.name:
					return possibleTarget.GetTargetType() == TargetType.Player && !sameTeam;
				case var value when value == TargetsEnum.ENEMY_SPELLS.name:
					return (possibleTarget.GetTargetType() == TargetType.Spell_continue_card || possibleTarget.GetTargetType() == TargetType.Spell_delayed_card || possibleTarget.GetTargetType() == TargetType.Spell_normal_card) && !sameTeam;

			}


			return false;
		}

		public static List<T> GetTargets<T>(this TargetsEnum type, T start, List<T> possibleTarget) where T : ITargetable {
			List<T> listOfTarget = new();
			foreach(var target in possibleTarget) {
				if(type.IsTargetable(start, target)) {
					listOfTarget.Add(target);
				}
			}

			if(type == TargetsEnum.RANDOM_CARDS || type == TargetsEnum.RANDOM_MONSTERS || type == TargetsEnum.RANDOM_PLAYERS || type == TargetsEnum.RANDOM_SPELLS) {
				listOfTarget.Shuffle();
			}

			return listOfTarget;
		}

		public static void Shuffle<T>(this List<T> list) {
			int n = list.Count;

			while(n > 1) {
				n--;
				int k = UnityEngine.Random.Range(0, n + 1);
				(list[n], list[k]) = (list[k], list[n]);
			}

		}

	}


}