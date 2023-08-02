using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Enums;

namespace ArcaneRealms.Scripts.Cards.GameCards {
	public class MonsterCard : CardInGame {


		public new MonsterInfoSO cardInfoSO;

		public int _DamageRecived = 0;

		//TODO - we could probably handle these with public StatChanger statChanger; and currentHealth = statChanger.TotalHealth - DamageRecived;
		public int _CurrentHealth;

		public MonsterCard(MonsterInfoSO info, Guid cardGuid, Guid team) : base(info, cardGuid, team) {
			cardInfoSO = info;
		}

		public override void Start() {
			_CurrentHealth = cardInfoSO.Health;
			Dictionary<StatType, int> map = new() {
				{ StatType.Health, cardInfoSO.Health },
				{ StatType.Attack, cardInfoSO.Atk },
				{ StatType.ManaCost, cardInfoSO.ManaCost }
			};
			statHandler = new StatHandler(map);
		}


		public bool CanAttack(MonsterCard monsterCard) {
			//GameManager.Instance.CanMonsterAttack()
			return true;
		}


	}
}