using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.SO;
using ArcaneRealms.Scripts.UI;
using ArcaneRealms.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetCardVisualMonster : MonoBehaviour, ICardBuilderUI {
	[SerializeField] Image artwork;
	[SerializeField] Image border;
	[SerializeField] TextMeshProUGUI manaCost;
	[SerializeField] TextMeshProUGUI nameCard;
	[SerializeField] TextMeshProUGUI race;
	[SerializeField] TextMeshProUGUI description;
	[SerializeField] TextMeshProUGUI atk;
	[SerializeField] TextMeshProUGUI health;
	[SerializeField] TextMeshProUGUI id;

	[SerializeField]
	MonsterInfoSO monsterInfo;

	public void BuildCardUI(CardInGame card, Sprite borderSprite) {
		if(card == null || !card.cardInfoSO.IsMonster(out var monsterInfoSO)) {
			return;
		}
		monsterInfo = monsterInfoSO;

		artwork.sprite = monsterInfo.Artwork;
		if(artwork.color == Color.black) {
			artwork.color = Color.white;
		}
		border.sprite = borderSprite;
		manaCost.text = monsterInfo.ManaCost.ToString();
		nameCard.text = monsterInfo.Name;
		race.text = monsterInfo.Race.GetName();
		description.text = monsterInfo.Description;
		atk.text = monsterInfo.Atk.ToString();
		health.text = monsterInfo.Health.ToString();
		id.text = monsterInfo.ID.ToString();
	}

	// Start is called before the first frame update
	void Start() {
		if(monsterInfo != null) {
			artwork.sprite = monsterInfo.Artwork;
			if(artwork.color == Color.black) {
				artwork.color = Color.white;
			}
			if(CardBorderSpriteManager.Instance != null) {
				border.sprite = CardBorderSpriteManager.Instance.BuildSprite(monsterInfo);
			}
			manaCost.text = monsterInfo.ManaCost.ToString();
			nameCard.text = monsterInfo.Name;
			race.text = monsterInfo.Race.GetName();
			description.text = monsterInfo.Description;
			atk.text = monsterInfo.Atk.ToString();
			health.text = monsterInfo.Health.ToString();
			id.text = monsterInfo.ID.ToString();
		}
	}

}
