using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Interfaces;
using UnityEngine;

namespace ArcaneRealms.Scripts.Cards.GameCards {
	public class MonsterCard : CardInGame , IDamageable {


		public new MonsterInfoSO cardInfoSO;

		private int damageReceived = 0;


		public MonsterCard(MonsterInfoSO info, Guid cardGuid, Guid team) : base(info, cardGuid, team) {
			cardInfoSO = info;
		}

		public override void Start() {
			Dictionary<StatType, int> map = new() {
				{ StatType.Health, cardInfoSO.Health },
				{ StatType.Attack, cardInfoSO.Atk },
				{ StatType.ManaCost, cardInfoSO.ManaCost }
			};
			statHandler = new StatHandler(map);
		}


		public int GetBaseAttack => statHandler.GetBaseStat(StatType.Attack);
		public int GetAttack() => statHandler.GetModifiedStat(StatType.Attack);

		public bool CanAttack(MonsterCard monsterCard) {
			//GameManager.Instance.CanMonsterAttack()
			return true;
		}


		public int GetHealth() => GetMaxHealth() - damageReceived;

		public int GetMaxHealth() => statHandler.GetModifiedStat(StatType.Health);

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
	}
}