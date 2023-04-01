
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.SO;
using ArcaneRealms.Scripts.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ArcaneRealms.Scripts.Managers {

	//notes: all the players SHOULD only contact the GameManager using ServerRPC, and the GameManager SHOULD contact all the players using ClientRPC
	/// <summary>
	/// This class is the core engine for the game, it will handle most of the logic the game has. Only the server will controll most of the ingame logic <br/>
	/// the two clients will only ask for server permission throught RPC
	/// </summary>
	public class GameManager : NetworkBehaviour {

		public static GameManager Instance { get; private set; }

		[Header("Events")]
		[SerializeField] GameEventSO player_ManaChangeEventSO;
		[SerializeField] GameEventSO player_DrawCardEventSO;
		[SerializeField] GameEventSO player_DeclareAttackEventSO;
		[SerializeField] GameEventSO enemyPlayer_PlayCardEventSO;
		[SerializeField] GameEventSO enemyPlayer_HoverCardInHandEventSO;

		[Header("Game Database and collection")]
		[SerializeField] CardInfoDataBase cardDatabaseSO;
		[SerializeField] CardEffectDataBaseSO cardEffectDatabaseSO;

		private PlayerInGame localPlayer = new(0); //0
		private PlayerInGame remotePlayer = new(1); //1

		[HideInInspector]
		public NetworkVariable<int> playerTurn = new NetworkVariable<int>(0);

		private GameState gameState;

		private void Awake() {
			if(Instance != null) {
				Destroy(gameObject);
				return;
			}
			Instance = this;
		}


		private void Start() {
			if(IsServer) {
				playerTurn.Value = Random.Range(0.0f, 1f) > 0.5 ? 1 : 0;
				StartCoroutine(HandleGameStateCoroutine());
			}
		}


		IEnumerator HandleGameStateCoroutine() {
			while(true) {
				yield return new WaitForSeconds(.5f);

				switch(gameState) {
					case GameState.TurnEnd:
						//TODO - run effects that run at end turn
						playerTurn.Value = (playerTurn.Value + 1) % 2;
						EndTurnEventClientRPC();
						gameState = GameState.TurnStart;
						break;
					case GameState.TurnStart:
						//TODO - run effect at turn start
						PlayerAddManaStartingTurn();
						StartTurnEventClientRPC();
						gameState = GameState.PlayerDraw;
						break;
					case GameState.PlayerDraw:
						PlayerDrawCardFromDeckAtStartingTurn();
						//TODO - run effect at card draw
						gameState = GameState.PlayerHandleTurn;
						break;
					case GameState.PlayerHandleTurn:
						//The Coroutine has nothing to do here, the player will swap to TurnEnd when he is done or the Game will end if a player Lose
						break;

					case GameState.GameEnd:
						Debug.Log("Game Ended!");
						break;

				}

			}
			//Debug.Log("HandleGameStateCoroutine Finished!");
		}

		/*[ServerRpc(RequireOwnership = false)]
		public void SpawnCommandServerRPC(ServerRpcParams serverRpcParams = default) {
			Debug.Log("Local Client ID: " + NetworkManager.Singleton.LocalClientId);
			Debug.Log("Running on server RPC: " + serverRpcParams.Receive.SenderClientId);
			SpawnOnRandomPositionClientRPC();
		}

		[ClientRpc]
		public void SpawnOnRandomPositionClientRPC() {
			Vector3 position = startingPosition.position + new Vector3(0, 0, Random.Range(0, 10));
			Instantiate(gameObjectPrefab, position, Quaternion.identity);
		}*/

		public PlayerInGame GetPlayerFromID(ulong clientID) {
			if(localPlayer.ID == clientID)
				return localPlayer;
			if(remotePlayer.ID == clientID)
				return remotePlayer;
			return null;
		}


		public PlayerInGame GetLocalPlayer() {
			return GetPlayerFromID(NetworkManager.Singleton.LocalClientId);
		}

		internal PlayerInGame GetEnemyPlayer(PlayerInGame owner) {
			if(owner.ID == localPlayer.ID) {
				return remotePlayer;
			}
			return localPlayer;
		}

		private void PlayerAddManaStartingTurn() {
			if(playerTurn.Value == 0) {
				localPlayer.AddMana(1);
			} else {
				remotePlayer.AddMana(1);
			}
		}

		private void PlayerDrawCardFromDeckAtStartingTurn() {
			if(playerTurn.Value == 0) {
				localPlayer.DrawCards(1);
			} else {
				remotePlayer.DrawCards(1);
			}
		}


		//----------------------Server RPCs-------------------------------

		[ServerRpc(RequireOwnership = false)]
		public void PlayerHoverOnCardInHandServerRPC(int cardInHandIndex, ServerRpcParams parameters = default) {
			PlayerHoverOnCardInHandClientRPC(parameters.Receive.SenderClientId, cardInHandIndex);
		}


		[ServerRpc(RequireOwnership = false)]
		public void PlayerPlayCardFromHandServerRPC(int cardFromHandIndex, int cardOnFloorDestinationIndex = 0, ServerRpcParams parameters = default) {
			//get the card from the player (and remove from hand)
			//get the current cost
			//subtract the mana from the player (and send them to players)
			//play the card on the field and then activate his effect
			ulong clientID = parameters.Receive.SenderClientId;
			PlayerInGame player = GetPlayerFromID(clientID);
			if(player == null) {
				Debug.LogError("Player Null when Playing a Card From Hand? clientID: " + clientID);
				return;
			}
			CardInGame cardInPlay = cardDatabaseSO.GetCardFromID("1").BuildCardInGame(); //TODO change this
			player.handCards.Remove(cardInPlay);
			int manaCost = cardInPlay.GetManaCost();
			player.usableMana -= manaCost;
			player.PlayCard(cardInPlay, cardOnFloorDestinationIndex);

			cardInPlay.RunOnActivationEffect();

			string cardID = cardInPlay.cardInfoSO.ID;
			string cardStatJson = cardInPlay.GetJsonStatHandler();
			PlayerPlayCardFromHandClientRPC(clientID, cardFromHandIndex, cardID, cardStatJson, cardOnFloorDestinationIndex);

		}

		[ServerRpc(RequireOwnership = false)]
		public void DeclareMonsterAttackServerRPC(int attackerIndex, int defenderIndex, ServerRpcParams parameters = default) {
			//TODO - run all effects and checks.

			DeclareMonsterAttackClientRPC(parameters.Receive.SenderClientId, attackerIndex, defenderIndex);

		}


		// -------------------Client RPCs--------------------------------

		[ClientRpc]
		public void PlayerHoverOnCardInHandClientRPC(ulong clientID, int cardInHandIndex) {
			if(clientID == NetworkManager.LocalClientId) {
				return;
			}
			object[] parameters = { clientID, cardInHandIndex };
			enemyPlayer_HoverCardInHandEventSO.Raise(this, parameters);
		}

		[ClientRpc]
		public void PlayerManaChangedClientRPC(ulong clientID, int newManaPool, int newManaUsable) {
			if(!IsServer) {
				if(clientID == localPlayer.ID) {
					localPlayer.usableMana = newManaUsable;
					localPlayer.currentManaPool = newManaPool;
				} else {
					remotePlayer.usableMana = newManaUsable;
					remotePlayer.currentManaPool = newManaPool;
				}
			}

			object[] parameters = { clientID, newManaPool, newManaUsable };
			player_ManaChangeEventSO.Raise(this, parameters);
		}

		[ClientRpc]
		public void PlayerDrawCardClientRPC(ulong clientID, string cardID, string cardJsonInfo) {
			object[] parameters;
			if(clientID == localPlayer.ID) {
				CardInfoSO cardSO = cardDatabaseSO.GetCardFromID(cardID);
				CardInGame cardInGame = cardSO.BuildCardInGame(cardJsonInfo, clientID);
				if(!IsServer) {
					localPlayer.AddCardToHand(cardInGame);
					localPlayer.RemoveCardFromDeck(cardID);
				}
				object[] objs = { clientID, cardInGame };
				parameters = objs;
			} else {
				localPlayer.AddCardInHandCount(); //no reason to keep the other infos
				object[] objs = { clientID };
				parameters = objs;
			}

			player_DrawCardEventSO.Raise(this, parameters);
		}

		[ClientRpc]
		public void PlayerPlayCardFromHandClientRPC(ulong clientID, int cardFromHandIndex, string cardID, string cardJsonInfo, int cardOnFloorDestinationIndex = 0) {
			PlayerInGame player = GetPlayerFromID(clientID);
			if(player == null) {
				Debug.LogError("Player == null with client ID: " + clientID);
				return;
			}

			CardInfoSO cardSO = cardDatabaseSO.GetCardFromID(cardID);
			CardInGame cardInGame = cardSO.BuildCardInGame(cardJsonInfo, clientID);
			if(!IsServer) {
				if(clientID == NetworkManager.Singleton.LocalClientId) {
					player.RemoveCardFromHand(cardFromHandIndex);
				} else {
					player.RemoveCardInHandCount();
				}
				player.PlayCard(cardInGame, cardOnFloorDestinationIndex);
			}

			if(player.ID == NetworkManager.Singleton.LocalClientId) {
				return;
			}
			object[] parameters = { clientID, cardInGame, cardOnFloorDestinationIndex };
			enemyPlayer_PlayCardEventSO.Raise(this, parameters);
		}

		[ClientRpc]
		private void DeclareMonsterAttackClientRPC(ulong senderClientId, int attackerIndex, int defenderIndex) {
			//do this need to do anything? we need to update stats and have the effect run as the server

			//TODO- run event!
			object[] parameters = { senderClientId, attackerIndex, defenderIndex, false, false };
			player_DeclareAttackEventSO.Raise(this, parameters);
		}

		[ClientRpc]
		public void EndTurnEventClientRPC() {

		}

		[ClientRpc]
		public void StartTurnEventClientRPC() {

		}






		public int GetPlayerMonsterCount() {
			PlayerInGame playerInGame = GetLocalPlayer();
			return playerInGame.monsterCardOnField.Count;
		}



		public new bool IsServer { get { return base.IsServer; } }
		public new bool IsHost { get { return base.IsHost; } }
		public new bool IsClient { get { return base.IsClient; } }


	}

	public enum GameState {
		Waiting,
		GameStart,
		TurnStart,
		PlayerDraw,
		PlayerHandleTurn,
		TurnEnd,
		GameEnd
	}
}
