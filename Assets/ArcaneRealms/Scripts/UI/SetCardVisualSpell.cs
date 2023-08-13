using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcaneRealms.Scripts.UI {
	public class SetCardVisualSpell : MonoBehaviour, ICardBuilderUI {
		[SerializeField] Image artwork;
		[SerializeField] Image border;
		[SerializeField] Image type;
		[SerializeField] TextMeshProUGUI manaCost;
		[SerializeField] TextMeshProUGUI nameCard;
		[SerializeField] TextMeshProUGUI description;
		[SerializeField] TextMeshProUGUI id;

		[SerializeField]
		SpellInfoSO spellInfo;

		public void BuildCardUI(CardInGame card, Sprite borderSprite) {
			if(card == null || !card.cardInfoSO.IsSpell(out var spellInfoSO)) {
				return;
			}
			spellInfo = spellInfoSO;

			artwork.sprite = spellInfoSO.Artwork;
			if(artwork.color == Color.black) {
				artwork.color = Color.white;
			}
			border.sprite = borderSprite;
			manaCost.text = spellInfo.ManaCost.ToString();
			nameCard.text = spellInfo.Name;

			description.text = spellInfo.Description;
			id.text = spellInfo.ID.ToString();
		}



	}
}