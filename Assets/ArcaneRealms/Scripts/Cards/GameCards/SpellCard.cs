using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.ScriptableCards;

namespace ArcaneRealms.Scripts.Cards.GameCards {
	public class SpellCard : CardInGame {

		public new SpellInfoSO cardInfoSO;
		public SpellCard(SpellInfoSO infoSO, Guid cardGuid, Guid team) : base(infoSO, cardGuid, team) {
			cardInfoSO = infoSO;
		}

		public override void Start() {
			Dictionary<StatType, int> map = new() {
				{ StatType.ManaCost, cardInfoSO.ManaCost }
			};
			statHandler = new StatHandler(map);
		}

	}
}