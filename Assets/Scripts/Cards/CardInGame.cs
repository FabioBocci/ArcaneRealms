using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.SO;
using Assets.Scripts.Cards;

namespace ArcaneRealms.Scripts.Cards {
	public abstract class CardInGame {
		public CardInfoSO cardInfoSO;

		public StatHandler statHandler;


		protected CardInGame(CardInfoSO cardInfo) {
			cardInfoSO = cardInfo;
		}

		protected virtual void Start() {

		}


		public bool IsMonsterCard(out MonsterCard monster) {
			if(this is MonsterCard monsterCard) {
				monster = monsterCard;
				return true;
			}
			monster = null;
			return false;
		}

		public bool IsSpellCard(out SpellCard spell) {
			if(this is SpellCard spellCard) {
				spell = spellCard;
				return true;
			}
			spell = null;
			return false;
		}

		public int GetManaCost() {
			return statHandler.GetModifiedStat(StatType.ManaCost);
		}

		public string GetJsonStatHandler() {
			return statHandler.ToJson();
		}

		public void SetStatHandlerFromJson(string jsonStatHandler) {
			statHandler.FromJson(jsonStatHandler);
		}
	}

}
