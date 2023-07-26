using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.UI;
using Assets.Scripts.UI;
using Unity.Netcode;
using UnityEngine;

namespace ArcaneRealms.Scripts.Managers {

	/// <summary>
	/// This class is for client only, it will manage the physic implementation of the card in the ui space (hands) <br></br>
	/// It will also handle these event called from the local GameManager:
	/// <list type="bullet">
	/// <item> <see cref="OnManaChangeEvent"> OnTurnChange </see> <description> ...... </description></item>
	/// <item> <see cref="OnPlayerDrawCard"> OnCardDraw </see> <description>....... </description></item>
	/// <item> <see cref="OnManaChangeEvent"> OnManaChange </see> <description> ....... </description></item>
	/// <item> <see cref="..."> OnPlayCard </see> <description> ....... </description></item>
	/// <item> <see cref="OnEnemyHoverEvent"> OnEnemyHovering </see> <description> ....... </description></item>
	/// </list>
	/// </summary>
	public class HandUIManager : MonoBehaviour {

		[SerializeField] RectTransform playerHandTransform;
		[SerializeField] RectTransform enemyHandTransform;

		[Header("Card prefab")]
		[SerializeField] RectTransform playerCardPrefab_Monster;
		[SerializeField] RectTransform playerCardPrefab_Spell;
		[SerializeField] RectTransform emptyCardPrefab_back;


		public static HandUIManager Instance { get; private set; }

		[HideInInspector]
		public bool PlayerIsDraggingCard = false;
		[HideInInspector]
		public CardInHandHandlerUI CardInHandHandler = null;
		[HideInInspector]
		public EnemyCardInHandUI enemyCardInHandHighlight = null; // keep reference of which EnemyCard is highlighting now

		private void Awake() {
			Instance = this;
		}


		public void OnPlayerDrawCard(Component component, object parameters) {
			if(parameters == null) {
				return;
			}

			object[] objects = (object[]) parameters;
			ulong clientID = (ulong) objects[0];

			if(NetworkManager.Singleton.LocalClientId == clientID) {
				CardInGame cardInGame = (CardInGame) objects[1];
				//TODO - run animation to card draw
			} else {
				//TODO - run animation for enemy card draw
			}

		}

		public void OnEnemyHoverEvent(Component component, object parameters) {
			if(parameters == null) {
				return;
			}
			object[] objects = (object[]) parameters;
			int cardFromHandIndex = (int) objects[1];

			if(cardFromHandIndex == -1) {
				enemyCardInHandHighlight = null;
				return;
			}

			Transform transform = enemyHandTransform.GetChild(cardFromHandIndex);
			if(transform == null) {
				Debug.LogError("transform == null");
				return;
			}

			EnemyCardInHandUI enemy = transform.GetComponent<EnemyCardInHandUI>();
			enemyCardInHandHighlight = enemy;
			enemy.ActiveOutline();
		}


		public void OnManaChangeEvent(Component component, object parameters) {
			if(parameters == null) { return; }
		}
	}
}