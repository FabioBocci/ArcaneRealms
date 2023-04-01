using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.SO;
using System.Collections.Generic;

namespace Assets.Scripts.Cards {
	public class SpellCard : CardInGame {

		public new SpellInfoSO cardInfoSO;
		public SpellCard(SpellInfoSO infoSO, ulong team) : base(infoSO, team) {
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