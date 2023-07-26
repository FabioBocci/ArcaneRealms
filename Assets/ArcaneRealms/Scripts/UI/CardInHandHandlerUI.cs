using ArcaneRealms.Scripts.ArrowPointer;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.SO;
using ArcaneRealms.Scripts.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI {
	public class CardInHandHandlerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {

		[Header("Monsters border Sprite", order = 0)]
		[SerializeField] private Sprite commonMonsterSprite;
		[SerializeField] private Sprite rareMonsterSprite;
		[SerializeField] private Sprite epicMonsterSprite;
		[SerializeField] private Sprite legendaryMonsterSprite;

		[Header("Spells border Sprite", order = 1)]
		[SerializeField] private Sprite commonSpellSprite;
		[SerializeField] private Sprite rareSpellSprite;
		[SerializeField] private Sprite epicSpellSprite;
		[SerializeField] private Sprite legendarySpellSprite;

		[Header("Hovering and Dragging params")]
		[SerializeField] private float TIMER = 0.5f;
		[SerializeField] private float SCALE_MULTIPLIER = 0.5f;
		[SerializeField] private float MOVE_UP_MULTIPLIER = 200;
		[SerializeField] private float ROTATION_Y_ANGLE = 30;
		//[SerializeField] private float ROTATION_X_ANGLE = 20;
		[SerializeField] private float ROTATION_SPEED = 20;

		[Header("field params")]
		[SerializeField] private MonsterInfoSO TMP_Monster; //TODO - remove this line


		public bool IsDragging { get; private set; } = false;


		private MonsterCard cardInGame;
		private ICardBuilderUI cardBuilderUI;
		private bool hoveringThisCard = false;
		private bool isShowingThisCard = false;
		private float interlTimer = 0;
		private GameObject cardCloneShowing = null;
		private RectTransform rectTransform = null;
		private CanvasGroup canvasGroup = null;
		private UIOutline uiOutline = null;
		private Transform originalParent = null;
		private int parentChildIndexForThisCard = -1;

		private void Awake() {
			cardBuilderUI = GetComponent<ICardBuilderUI>();
			rectTransform = GetComponent<RectTransform>();
			canvasGroup = GetComponent<CanvasGroup>();
			uiOutline = GetComponentInChildren<UIOutline>();
			BuildCard(new MonsterCard(TMP_Monster, 0)); //TODO - remove this line
		}

		private void Update() {
			if(hoveringThisCard && !isShowingThisCard && !IsDragging && !HandUIManager.Instance.PlayerIsDraggingCard) {
				interlTimer += Time.deltaTime;
				if(interlTimer >= TIMER) {
					interlTimer = 0;
					ShowCard();
				}
			}

			if(HandUIManager.Instance.PlayerIsDraggingCard && !IsDragging) {
				uiOutline.enabled = false;
			} else {
				uiOutline.enabled = true;
			}
		}

		public void BuildCard(MonsterCard card) {
			cardInGame = card;
			cardBuilderUI.BuildCardUI(card, BuildSprite(card.cardInfoSO));
		}

		private void ShowCard() {
			if(NetworkManager.Singleton != null) {
				GameManager.Instance.PlayerHoverOnCardInHandServerRPC(parentChildIndexForThisCard);
			}
			isShowingThisCard = true;
			cardCloneShowing = Instantiate(gameObject, gameObject.transform.position, Quaternion.identity);
			cardCloneShowing.name = "CCShowing_" + cardInGame.cardInfoSO.Name;
			cardCloneShowing.transform.SetParent(transform.parent.parent);
			cardCloneShowing.GetComponent<CanvasGroup>().blocksRaycasts = false;
			cardCloneShowing.GetComponent<CardInHandHandlerUI>().enabled = false;
			cardCloneShowing.transform.localScale += (Vector3.one * SCALE_MULTIPLIER);
			cardCloneShowing.transform.position += (Vector3.up * MOVE_UP_MULTIPLIER);
			canvasGroup.alpha = 0;
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
				case CardRarity.Common:
					return commonMonsterSprite;
				case CardRarity.Rare:
					return rareMonsterSprite;
				case CardRarity.Epic:
					return epicMonsterSprite;
				case CardRarity.Legendary:
					return legendaryMonsterSprite;
			}
		}

		private Sprite BuildSpellSprite(CardInfoSO infoSO) {
			switch(infoSO.Rarity) {
				default:
				case CardRarity.Common:
					return commonSpellSprite;
				case CardRarity.Rare:
					return rareSpellSprite;
				case CardRarity.Epic:
					return epicSpellSprite;
				case CardRarity.Legendary:
					return legendarySpellSprite;
			}
		}

		private int GetParentChildIndexForThisCard() {
			for(int i = 0; i < transform.parent.childCount; i++) {
				if(transform.parent.GetChild(i) == transform) {
					return i;
				}
			}

			return -1;
		}

		private void DestroyCardCloneShowing() {
			if(isShowingThisCard) {
				isShowingThisCard = false;
				Destroy(cardCloneShowing);
				cardCloneShowing = null;
				canvasGroup.alpha = 1;
			}
		}


		public void OnPointerEnter(PointerEventData eventData) {
			hoveringThisCard = true;
			parentChildIndexForThisCard = GetParentChildIndexForThisCard();
		}

		public void OnPointerExit(PointerEventData eventData) {
			hoveringThisCard = false;
			if(isShowingThisCard && NetworkManager.Singleton != null) {
				GameManager.Instance.PlayerHoverOnCardInHandServerRPC(-1);
			}
			DestroyCardCloneShowing();
		}
		public void OnBeginDrag(PointerEventData eventData) {

			if(cardInGame != null && !cardInGame.IsMonsterCard(out var monster) && cardInGame.HasTargetingEffects()) {
				//this only works for Spells not for monster since for monster we need before to check the final position
				ArrowPointerBuilder.CreateBuilder()
					.SetActionCallback((cardTarget) => {
						//we didn't choose a right target, reset the card in hand
						if(cardTarget == null) {
							OnEndDrag(null);
							return;
						}


					}).SetPredicateFilter((cardToFilter) => {
						return false;
					}).SetStartingPosition(transform) //todo set position from GameManager.Instance.localPlayerGameObject
					.BuildArrowPointer();
			}

			if(cardInGame.IsMonsterCard(out var monstreCard) && GameManager.Instance.GetPlayerMonsterCount() >= 5) {
				return;
			}

			IsDragging = true;
			HandUIManager.Instance.PlayerIsDraggingCard = true;

			originalParent = transform.parent;
			transform.SetParent(transform.parent.parent);
			rectTransform.rotation = Quaternion.identity;

			DestroyCardCloneShowing();
			canvasGroup.blocksRaycasts = false;
			Cursor.visible = false;
			FieldPannelManagerUI.Instance.ActiveGameObject();

		}

		public void OnDrag(PointerEventData eventData) {
			if(cardInGame.IsMonsterCard(out var monstreCard) && GameManager.Instance.GetPlayerMonsterCount() >= 5) {
				return;
			}

			if(ArrowPointerBuilder.HasRunningArrowPointer()) {
				//we are choosing a target
				return;
			}

			float horizontalInput = Input.GetAxis("Mouse X");
			//float verticalInput = Input.GetAxis("Mouse Y");


			// Calculate the angle of rotation based on the mouse movement
			float rotationAngleY = -horizontalInput * ROTATION_Y_ANGLE;

			// Calculate the new rotation of the card
			Quaternion newRotation = Quaternion.Euler(0, rotationAngleY, 0f);

			// Smoothly interpolate between the current rotation and the new rotation
			float rotationSpeed = ROTATION_SPEED; // adjust this value to control the rotation speed
			rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, newRotation, Time.deltaTime * rotationSpeed);

			transform.position = new Vector3(eventData.position.x, eventData.position.y - rectTransform.sizeDelta.y * rectTransform.localScale.y / 2, 0);

			if(FieldPannelManagerUI.Instance.IsInsideFieldPannel) {
				FieldPannelManagerUI.Instance.PointerAt(eventData);
			}

		}

		public void OnEndDrag(PointerEventData eventData) {
			Cursor.visible = true;
			IsDragging = false;
			hoveringThisCard = false;
			transform.SetParent(originalParent);
			transform.SetSiblingIndex(parentChildIndexForThisCard);

			canvasGroup.blocksRaycasts = true;
			FieldPannelManagerUI.Instance.DeactivateGameObject();
			HandUIManager.Instance.PlayerIsDraggingCard = false;
		}

		internal CardInGame GetCardInGame() {
			return cardInGame;
		}

		public void DestroyAndResetState() {
			Destroy(gameObject);
			HandUIManager.Instance.PlayerIsDraggingCard = false;
			Cursor.visible = true;
		}

		public int GetCardInGameIndex() {
			return parentChildIndexForThisCard;
		}
	}
}