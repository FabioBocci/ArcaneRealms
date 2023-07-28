using System;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.SO;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.GameCards;

namespace Assets.Scripts.Cards {
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