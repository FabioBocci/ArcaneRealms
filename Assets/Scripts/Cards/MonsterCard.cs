using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.SO;
using System.Collections.Generic;

namespace ArcaneRealms.Scripts.Cards {
	public class MonsterCard : CardInGame {


		public new MonsterInfoSO cardInfoSO;

		public int _DamageRecived = 0;

		//TODO - we could probably handle these with public StatChanger statChanger; and currentHealth = statChanger.TotalHealth - DamageRecived;
		public int _CurrentHealth;

		public MonsterCard(MonsterInfoSO info) : base(info) {
			cardInfoSO = info;
		}

		protected override void Start() {
			_CurrentHealth = cardInfoSO.Health;
			Dictionary<StatType, int> map = new() {
				{ StatType.Health, cardInfoSO.Health },
				{ StatType.Attack, cardInfoSO.Atk },
				{ StatType.ManaCost, cardInfoSO.ManaCost }
			};
			statHandler = new StatHandler(map);
		}


	}
}