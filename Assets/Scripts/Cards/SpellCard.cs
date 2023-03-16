using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.SO;
using System.Collections.Generic;

namespace Assets.Scripts.Cards {
	public class SpellCard : CardInGame {

		public new SpellInfoSO cardInfoSO;
		public SpellCard(SpellInfoSO infoSO) : base(infoSO) {
			cardInfoSO = infoSO;

			Dictionary<StatType, int> map = new() {
				{ StatType.ManaCost, cardInfoSO.ManaCost }
			};
			statHandler = new StatHandler(map);
		}

		protected override void Start() {
			Dictionary<StatType, int> map = new() {
				{ StatType.ManaCost, cardInfoSO.ManaCost }
			};
			statHandler = new StatHandler(map);
		}

	}
}