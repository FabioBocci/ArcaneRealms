using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.Utils.ArrowPointer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcaneRealms.Scripts.UI {
	public class FieldPannelManagerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {

		public static FieldPannelManagerUI Instance { get; private set; }

		[SerializeField] private LayerMask layerMask;

		public bool IsInsideFieldPannel { get; private set; } = false;

		private void Awake() {
			Instance = this;
			DeactivateGameObject();
		}

		public void ActiveGameObject() {
			gameObject.SetActive(true);
		}

		public void DeactivateGameObject() {
			gameObject.SetActive(false);
		}


		public void OnPointerEnter(PointerEventData eventData) {
			IsInsideFieldPannel = true;
		}

		public void OnPointerExit(PointerEventData eventData) {
			IsInsideFieldPannel = false;
			FieldManager.Instance.ResetMonsterPosition();
		}

		public void OnDrop(PointerEventData eventData) {
			IsInsideFieldPannel = false;

			GameObject gameObjectDropped = eventData.pointerDrag;
			CardInHandHandlerUI card = gameObjectDropped.GetComponent<CardInHandHandlerUI>();
			if(card != null) {
				CardInGame cardInGame = card.GetCardInGame();
				if(cardInGame.IsMonsterCard(out var monster)) {
					Ray ray = Camera.main.ScreenPointToRay(eventData.position);
					if(Physics.Raycast(ray, out RaycastHit hit, 100, layerMask))
					{
						FieldManager.Instance.TryGetNewMonsterIndex(hit.point, out int index);
						if (cardInGame.HasTargetingEffects()) 
						{
							FieldManager.Instance.TrySummonMonsterOnLocation(monster, hit.point, out int i, out Transform monsterTransform);
							ArrowPointerBuilder.CreateBuilder()
								.SetActionCallback((cardTarget) => {
									//we didn't choose a right target, reset the card in hand
									if(cardTarget == null) {
										//TODO - FieldManager.Instance.RemoveMonster(....)
										card.OnEndDrag(null); //TODO - maybe a nicer visual?
										return;
									}


								}).SetPredicateFilter((cardToFilter) => {
									return false;
								})//.SetStartingPosition(monsterTransform)
								.BuildArrowPointer();
						} 
						else 
						{
							card.DestroyAndResetState();
							GameManager.Instance.PlayCard(card.GetCardInGame(), index);
						}
					}
				}
			}
		}

		public void PointerAt(PointerEventData eventData) {
			Ray ray = Camera.main.ScreenPointToRay(eventData.position);
			if(Physics.Raycast(ray, out RaycastHit hit, 100, layerMask)) {
				FieldManager.Instance.MouseOnLocationHit(hit.point);
			}

		}
	}
}