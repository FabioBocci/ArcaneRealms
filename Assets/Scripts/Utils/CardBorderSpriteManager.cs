using ArcaneRealms.Scripts.SO;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils {
	public class CardBorderSpriteManager : MonoBehaviour {
		public static CardBorderSpriteManager Instance { get; private set; }

		[Header("Monsters")]
		[SerializeField] private Sprite commonMonsterSprite;
		[SerializeField] private Sprite rareMonsterSprite;
		[SerializeField] private Sprite epicMonsterSprite;
		[SerializeField] private Sprite legendaryMonsterSprite;

		[Header("Spells")]
		[SerializeField] private Sprite commonSpellSprite;
		[SerializeField] private Sprite rareSpellSprite;
		[SerializeField] private Sprite epicSpellSprite;
		[SerializeField] private Sprite legendarySpellSprite;

		private void Awake() {
			if(Instance != null) {
				Debug.LogWarning("Duplicate CardBorderSpriteManager in Scene");
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);
		}


		public Sprite BuildSprite(CardInfoSO infoSO) {
			if(infoSO.IsMonster(out var monster)) {
				return BuildMonsterSprite(infoSO);
			} else if(infoSO.IsSpell(out var spell)) {
				return BuildSpellSprite(infoSO);
			}

			return null;
		}

		private Sprite BuildMonsterSprite(CardInfoSO infoSO) {
			switch(infoSO.Rarity) {
				default:
				case Enums.CardRarity.Common:
					return commonMonsterSprite;
				case Enums.CardRarity.Rare:
					return rareMonsterSprite;
				case Enums.CardRarity.Epic:
					return epicMonsterSprite;
				case Enums.CardRarity.Legendary:
					return legendaryMonsterSprite;
			}
		}

		private Sprite BuildSpellSprite(CardInfoSO infoSO) {
			switch(infoSO.Rarity) {
				default:
				case Enums.CardRarity.Common:
					return commonSpellSprite;
				case Enums.CardRarity.Rare:
					return rareSpellSprite;
				case Enums.CardRarity.Epic:
					return epicSpellSprite;
				case Enums.CardRarity.Legendary:
					return legendarySpellSprite;
			}
		}
	}


}

