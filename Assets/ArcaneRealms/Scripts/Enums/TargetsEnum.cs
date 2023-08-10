using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.Effects;

namespace ArcaneRealms.Scripts.Enums {
	public class TargetsEnum {


		private static Parameter COUNT_ONE = new() { Key = "Targent-Count", Value = "1", Type = typeof(int).AssemblyQualifiedName };
		private static Parameter[] COUNT_ONE_ARR = { COUNT_ONE };

		public static readonly TargetsEnum NONE = new("none");

		public static readonly TargetsEnum SELF = new("Self");

		public static readonly TargetsEnum ALL = new("All");


		//random targets
		public static readonly TargetsEnum RANDOM_PLAYERS = new("Random Players", COUNT_ONE_ARR);
		public static readonly TargetsEnum RANDOM_MONSTERS = new("Random Monsters", COUNT_ONE_ARR);
		public static readonly TargetsEnum RANDOM_SPELLS = new("Random Spell", COUNT_ONE_ARR);
		public static readonly TargetsEnum RANDOM_CARDS = new("Random Cards", COUNT_ONE_ARR);

		//Enemey targets
		public static readonly TargetsEnum ENEMY_PLAYERS = new("Enemy Players", COUNT_ONE_ARR);
		public static readonly TargetsEnum ENEMY_MONSTERS = new("Enemy Monsters", COUNT_ONE_ARR);
		public static readonly TargetsEnum ENEMY_SPELLS = new("Enemy Spells", COUNT_ONE_ARR);
		public static readonly TargetsEnum ENEMY_CARDS = new("Enemy Cards", COUNT_ONE_ARR);

		//All types targets
		public static readonly TargetsEnum ALL_PLAYERS = new("All Players");
		public static readonly TargetsEnum ALL_MONSTERS = new("All Monsters");
		public static readonly TargetsEnum ALL_SPELLS = new("All Spells");
		public static readonly TargetsEnum ALL_CARDS = new("All Cards");

		//Ally Targets
		public static readonly TargetsEnum ALLY_PLAYERS = new("Ally Players", COUNT_ONE_ARR);
		public static readonly TargetsEnum ALLY_MONSTERS = new("Ally Monsters", COUNT_ONE_ARR);
		public static readonly TargetsEnum ALLY_SPELLS = new("Ally Spells", COUNT_ONE_ARR);
		public static readonly TargetsEnum ALLY_CARDS = new("Ally cards", COUNT_ONE_ARR);


		private TargetsEnum(string nameString) {
			name = nameString;
			parameters = new();
		}

		public TargetsEnum(string nameString, Parameter[] param) {
			name = nameString;
			parameters = new EffectParameters(param);
		}


		public string name;
		public EffectParameters parameters;








		public static List<TargetsEnum> GetTargetTypes() {
			return new List<TargetsEnum>() {
				NONE,
				SELF,
				ALL,
				RANDOM_PLAYERS,
				RANDOM_MONSTERS,
				RANDOM_SPELLS,
				RANDOM_CARDS,

				ENEMY_PLAYERS, ENEMY_MONSTERS, ENEMY_SPELLS, ENEMY_CARDS,
				ALL_PLAYERS, ALL_MONSTERS, ALL_SPELLS, ALL_CARDS,
				ALLY_PLAYERS, ALLY_MONSTERS, ALLY_SPELLS, ALLY_CARDS
			};
		}

		public static List<string> GetTargetTypeNames() {
			List<string> names = new List<string>();
			GetTargetTypes().ForEach(target => names.Add(target.name));
			return names;
		}

		public static TargetsEnum GetTargetType(string name) {
			return GetTargetTypes().Find(target => target.name == name);
		}

		public static int GetIndexOf(TargetsEnum type) {
			return GetTargetTypes().IndexOf(type);
		}

		public static bool TryParse(string value, out TargetsEnum newTargetTypeValue) {
			newTargetTypeValue = null;
			foreach(TargetsEnum targetType in GetTargetTypes()) {
				if(targetType.name == value) {
					newTargetTypeValue = targetType;
					return true;
				}
			}
			return false;
		}

	}


	[Serializable]
	public enum TargetType {
		MonsterCard, SpellNormalCard, SpellContinueCard, SpellDelayedCard, Player
	}
}