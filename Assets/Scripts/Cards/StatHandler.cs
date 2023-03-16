using ArcaneRealms.Scripts.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneRealms.Scripts.Cards {
	public class StatHandler {
		private Dictionary<StatType, int> baseStats;
		private Dictionary<StatType, List<StatChanger>> modifiedStats;

		public StatHandler(Dictionary<StatType, int> baseStats) {
			this.baseStats = baseStats;
			this.modifiedStats = new Dictionary<StatType, List<StatChanger>>();
		}

		public int GetBaseStat(StatType statType) {
			if(baseStats.ContainsKey(statType)) {
				return baseStats[statType];
			} else {
				Debug.LogError("StatHandler: Base stat not found for " + statType);
				return 0;
			}
		}

		public int GetModifiedStat(StatType statType) {
			int modifiedValue = GetBaseStat(statType);

			if(modifiedStats.ContainsKey(statType)) {
				foreach(var modifier in modifiedStats[statType]) {
					modifiedValue += modifier.amount;
				}
			}

			return modifiedValue;
		}

		public void ModifyStat(StatType statType, int amount, string effectID) {
			if(!modifiedStats.ContainsKey(statType)) {
				modifiedStats.Add(statType, new List<StatChanger>());
			}

			StatChanger statChanger = modifiedStats[statType].Find(x => x.effectID == effectID);
			if(statChanger != null) {
				statChanger.amount += amount;
			} else {
				modifiedStats[statType].Add(new StatChanger { amount = amount, effectID = effectID });
			}
		}

		public void AddStatModification(StatType statType, int amount, string effectID) {
			if(!modifiedStats.ContainsKey(statType)) {
				modifiedStats.Add(statType, new List<StatChanger>());
			}
			modifiedStats[statType].Add(new StatChanger { amount = amount, effectID = effectID });
		}

		public void RemoveStatModifier(StatType statType, string effectID) {
			if(modifiedStats.ContainsKey(statType)) {
				modifiedStats[statType].RemoveAll(x => x.effectID == effectID);

				if(modifiedStats[statType].Count == 0) {
					modifiedStats.Remove(statType);
				}
			}
		}

		public void ClearStatModifiers() {
			modifiedStats.Clear();
		}

		public string ToJson() {
			Dictionary<string, object> dict = new Dictionary<string, object>();

			foreach(var baseStat in baseStats) {
				dict.Add(baseStat.Key.ToString(), baseStat.Value);
			}

			Dictionary<string, object> modifiedStatsDict = new Dictionary<string, object>();

			foreach(var modifiedStat in modifiedStats) {
				List<object> statChangersList = new List<object>();

				foreach(var statChanger in modifiedStat.Value) {
					Dictionary<string, object> statChangerDict = new Dictionary<string, object> {
						{ "amount", statChanger.amount },
						{ "effectID", statChanger.effectID }
					};

					statChangersList.Add(statChangerDict);
				}

				modifiedStatsDict.Add(modifiedStat.Key.ToString(), statChangersList);
			}

			dict.Add("modifiedStats", modifiedStatsDict);

			return JsonUtility.ToJson(dict);
		}
		public void FromJson(string json) {
			Dictionary<string, object> dict = JsonUtility.FromJson<Dictionary<string, object>>(json);

			baseStats = new Dictionary<StatType, int>();
			modifiedStats = new Dictionary<StatType, List<StatChanger>>();

			foreach(var key in dict.Keys) {
				if(key == "modifiedStats") {
					Dictionary<string, object> modifiedStatsDict = (Dictionary<string, object>) dict[key];

					foreach(var modifiedStatKey in modifiedStatsDict.Keys) {
						StatType statType = (StatType) Enum.Parse(typeof(StatType), modifiedStatKey);
						List<object> statChangersList = (List<object>) modifiedStatsDict[modifiedStatKey];
						List<StatChanger> statChangers = new List<StatChanger>();

						foreach(var statChangerObj in statChangersList) {
							Dictionary<string, object> statChangerDict = (Dictionary<string, object>) statChangerObj;
							StatChanger statChanger = new StatChanger();
							statChanger.amount = Convert.ToInt32(statChangerDict["amount"]);
							statChanger.effectID = (string) statChangerDict["effectID"];

							statChangers.Add(statChanger);
						}

						modifiedStats.Add(statType, statChangers);
					}
				} else {
					StatType statType = (StatType) Enum.Parse(typeof(StatType), key);
					int value = Convert.ToInt32(dict[key]);

					baseStats.Add(statType, value);
				}
			}
		}
	}


	public class StatChanger {
		public int amount;
		public string effectID;
	}
}