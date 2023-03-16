using ArcaneRealms.Scripts.Cards;
using Unity.Netcode;
using UnityEngine;

namespace ArcaneRealms.Scripts.UI {
	public class PlayersHandHandlerUI : MonoBehaviour {

		[SerializeField] RectTransform playerHandTransform;
		[SerializeField] RectTransform enemyHandTransform;


		[SerializeField] RectTransform playerCardPrefab_Monster;
		[SerializeField] RectTransform playerCardPrefab_Spell;


		public static PlayersHandHandlerUI Instance { get; private set; }

		[HideInInspector]
		public bool PlayerIsDraggingCard = false;

		private void Awake() {
			Instance = this;
		}

		// Update is called once per frame
		void Update() {

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
	}
}