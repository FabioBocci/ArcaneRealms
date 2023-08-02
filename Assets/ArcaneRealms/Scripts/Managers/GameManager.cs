
using System;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Systems;
using ArcaneRealms.Scripts.Utils.Events;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace ArcaneRealms.Scripts.Managers {

	//notes: all the players SHOULD only contact the GameManager using ServerRPC, and the GameManager SHOULD contact all the players using ClientRPC
	/// <summary>
	/// This class is the core engine for the game, it will handle most of the logic the game has. Only the server will controll most of the ingame logic <br/>
	/// the two clients will only ask for server permission throught RPC
	/// </summary>
	public class GameManager : NetworkBehaviour {

		[Header("Game Infos")]
		[SerializeField] private int startingHandSize = 3;


		#region Events
		
		public event EntityEvent<List<CardInGame>> OnStartingCardsReceived;
		
		
		#endregion
		
		public static GameManager Instance { get; private set; }
		
		private PlayerInGame localPlayer; //0
		private PlayerInGame remotePlayer; //1

		[HideInInspector]
		public NetworkVariable<Guid> playerTurn = new();

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
				gameState = GameState.PlayersHandShake;
				StartHandlePlayersHandShake();
				StartCoroutine(HandleGameStateCoroutine());
			}
			
		}

		private void StartHandlePlayersHandShake()
		{
			localPlayer = new PlayerInGame(Guid.NewGuid(), NetworkManager.ConnectedClients.Keys.First());
			ulong remotePlayerUlong = 1;
			foreach (var playerUlong in NetworkManager.ConnectedClients.Keys)
			{
				if (playerUlong != localPlayer.playerUlong)
				{
					remotePlayerUlong = playerUlong;
					break;
				}
			}

			remotePlayer = new PlayerInGame(Guid.NewGuid(), remotePlayerUlong);
			playerTurn.Value = Random.Range(0.0f, 1f) > 0.5 ? localPlayer.ID : remotePlayer.ID;
			SendSignalStartHandShakeClientRPC(localPlayer.playerUlong, localPlayer.ID, remotePlayer.playerUlong,
				remotePlayer.ID);
		}
		
		private void HandlePlayerChooseCards()
		{
			PlayerInGame player = GetPlayerTurn();
			//chose 3 card from the player and 3 for the other
			List<CardInGame> cardsInHand = player.currentDeck.GetRange(0, startingHandSize);
			
			SendPlayersStartingHandsClientRPC(player, cardsInHand, player.thisClientRpcTarget);
			
			player = GetEnemyPlayer(player);
			cardsInHand = player.currentDeck.GetRange(0, startingHandSize);
			SendPlayersStartingHandsClientRPC(player, cardsInHand, player.thisClientRpcTarget);
		}
		
		IEnumerator HandleGameStateCoroutine() {
			while(true) {
				yield return new WaitForSeconds(.5f);

				switch(gameState) {
					case GameState.PlayersChooseHandCards:
						//after hand shake, so all player has loaded and we can start using Guid
						HandlePlayerChooseCards();
						break;
					case GameState.TurnEnd:
						//TODO - run effects that run at end turn
						SetPlayerTurn(GetEnemyPlayer(GetPlayerTurn()));
						EndTurnEventClientRPC();
						gameState = GameState.TurnStart;
						break;
					case GameState.TurnStart:
						//TODO - run effect at turn start
						StartTurnEventClientRPC();
						gameState = GameState.PlayerDraw;
						break;
					case GameState.PlayerDraw:
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


		public PlayerInGame GetPlayerTurn()
		{
			return GetPlayerFromID(playerTurn.Value);
		}

		public void SetPlayerTurn(PlayerInGame player)
		{
			if (NetworkManagerHelper.Instance.IsServer)
			{
				playerTurn.Value = player.ID;
			}
		}

		public PlayerInGame GetPlayerFromID(Guid clientID) {
			if(localPlayer.ID == clientID)
				return localPlayer;
			if(remotePlayer.ID == clientID)
				return remotePlayer;
			return null;
		}

		public PlayerInGame GetPlayerFromUlong(ulong clientUlong)
		{
			return localPlayer.playerUlong == clientUlong ? localPlayer : remotePlayer;
		}


		internal PlayerInGame GetEnemyPlayer(PlayerInGame owner)
		{
			return owner.ID == localPlayer.ID ? remotePlayer : localPlayer;
		}

		
		
		//----------------------Server RPCs-------------------------------

		private Dictionary<PlayerInGame, List<DeckCardSerializer>> handShake;

		[ServerRpc(RequireOwnership = false)]
		public void PlayerSendHandShakeServerRPC(Guid playerGuid, List<CardInDeck> deck, string avatarId, ServerRpcParams parameters = default)
		{
			Debug.Log($"DeckSize: {deck.Count} cardsName: {deck.Select(card => card.card.name).ToArray()} ");
			if (handShake == null)
			{
				handShake = new Dictionary<PlayerInGame, List<DeckCardSerializer>>
				{
					[localPlayer] = null,
					[remotePlayer] = null
				};
			}

			PlayerInGame player = GetPlayerFromID(playerGuid);
			handShake[player] = new();
			foreach (CardInDeck cardInDeck in deck)
			{
				for (int i = 0; i < cardInDeck.count; i++)
				{
					CardInGame card = cardInDeck.card.BuildCardInGame(player.ID, Guid.NewGuid());
					handShake[player].Add(new DeckCardSerializer()
					{
						cardInfo = card.cardInfoSO,
						guid = card.CardGuid
					});
					player.startingDeck.Add(card);
					player.allCardInGameDicionary.Add(card.CardGuid, card);
					player.currentDeck.AddRange(player.startingDeck);
					player.currentDeck.Shuffle();
				}
			}

			if (!handShake.Any(pair => pair.Value == null || pair.Value.Count == 0))
			{
				SendPlayersDataAfterHandShakeClientRPC(localPlayer.ID, handShake[localPlayer].ToArray(), remotePlayer.ID, handShake[remotePlayer].ToArray());
				handShake = null;
				gameState = GameState.PlayersChooseHandCards;
			}

		}



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
			PlayerInGame player = null; // GetPlayerFromID(clientID);
			if(player == null) {
				Debug.LogError("Player Null when Playing a Card From Hand? clientID: " + clientID);
				return;
			}
			return;
			CardInGame cardInPlay = null; // cardDatabaseSO.GetCardFromID("1").BuildCardInGame(); //TODO change this
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
		private void SendSignalStartHandShakeClientRPC(ulong localPlayerPlayerUlong, Guid localPlayerID, ulong remotePlayerPlayerUlong, Guid remotePlayerID)
		{
			if (!NetworkManagerHelper.Instance.IsServer)
			{
				localPlayer = new PlayerInGame(NetworkManager.LocalClientId == localPlayerPlayerUlong ? localPlayerID : remotePlayerID, NetworkManager.LocalClientId);
				remotePlayer = new PlayerInGame(NetworkManager.LocalClientId != localPlayerPlayerUlong ? localPlayerID : remotePlayerID, remotePlayerPlayerUlong);
			}
			
			DeckOfCards deck = PlayerDataManager.Instance.playerData.SelectedDeck;
			if (deck == null)
			{
				Debug.LogError("[GameManager] null deck selected! this is a bug!");
				return;
			}


			PlayerSendHandShakeServerRPC(localPlayer.ID, deck.cards, "empty");
		}

		[ClientRpc]
		private void SendPlayersDataAfterHandShakeClientRPC(Guid player1, DeckCardSerializer[] player1Deck, Guid player2, DeckCardSerializer[] player2Deck)
		{
			if (!NetworkManagerHelper.Instance.IsServer)
			{
				PlayerInGame player = GetPlayerFromID(player1);
				foreach (DeckCardSerializer card in player1Deck)
				{
					CardInGame cardInGame = card.cardInfo.BuildCardInGame(player.ID, card.guid);
					player.startingDeck.Add(cardInGame);
					player.allCardInGameDicionary[cardInGame.CardGuid] = cardInGame;
				}
				player.currentDeck.AddRange(player.startingDeck);
				player.currentDeck.Shuffle();

				player = GetPlayerFromID(player2);
				foreach (DeckCardSerializer card in player2Deck)
				{
					CardInGame cardInGame = card.cardInfo.BuildCardInGame(player.ID, card.guid);
					player.startingDeck.Add(cardInGame);
					player.allCardInGameDicionary[cardInGame.CardGuid] = cardInGame;
				}
				player.currentDeck.AddRange(player.startingDeck);
				player.currentDeck.Shuffle();
			}
		}

		[ClientRpc]
		private void SendPlayersStartingHandsClientRPC(PlayerInGame thisPlayer, List<CardInGame> startingHand, ClientRpcParams clientRpcParam = default)
		{
			Debug.LogError($"Recived SendPlayersStartingHandsClientRPC");
			if (thisPlayer != localPlayer)
			{
				Debug.LogError($" Received a ClientRPC for a different client! Aborting RPC! this client {localPlayer.playerUlong} received {thisPlayer.playerUlong}");
				return;
			}
			
			OnStartingCardsReceived?.Invoke(new EntityEventData<List<CardInGame>>(startingHand));
			
		}
		
		[ClientRpc]
		public void PlayerHoverOnCardInHandClientRPC(ulong clientID, int cardInHandIndex) {
			if(clientID == NetworkManager.LocalClientId) {
				return;
			}
			object[] parameters = { clientID, cardInHandIndex };
		}

		[ClientRpc]
		public void PlayerManaChangedClientRPC(ulong clientID, int newManaPool, int newManaUsable) {
			if(!IsServer) {
				if(false) {
					localPlayer.usableMana = newManaUsable;
					localPlayer.currentManaPool = newManaPool;
				} else {
					remotePlayer.usableMana = newManaUsable;
					remotePlayer.currentManaPool = newManaPool;
				}
			}

			object[] parameters = { clientID, newManaPool, newManaUsable };
		}

		[ClientRpc]
		public void PlayerDrawCardClientRPC(ulong clientID, string cardID, string cardJsonInfo) {
			object[] parameters;
			if(false) {
				CardInGame cardInGame = null; // cardSO.BuildCardInGame(cardJsonInfo, clientID);
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

		}

		[ClientRpc]
		public void PlayerPlayCardFromHandClientRPC(ulong clientID, int cardFromHandIndex, string cardID, string cardJsonInfo, int cardOnFloorDestinationIndex = 0)
		{
			PlayerInGame player = null; // GetPlayerFromID(clientID);
			if(player == null) {
				Debug.LogError("Player == null with client ID: " + clientID);
				return;
			}

			CardInGame cardInGame = null; // cardSO.BuildCardInGame(cardJsonInfo, clientID);
			if(!IsServer) {
				if(clientID == NetworkManager.Singleton.LocalClientId) {
					player.RemoveCardFromHand(cardFromHandIndex);
				} else {
					player.RemoveCardInHandCount();
				}
				player.PlayCard(cardInGame, cardOnFloorDestinationIndex);
			}
			
			object[] parameters = { clientID, cardInGame, cardOnFloorDestinationIndex };
		}

		[ClientRpc]
		private void DeclareMonsterAttackClientRPC(ulong senderClientId, int attackerIndex, int defenderIndex) {
			//do this need to do anything? we need to update stats and have the effect run as the server
			
		}

		[ClientRpc]
		public void EndTurnEventClientRPC() {

		}

		[ClientRpc]
		public void StartTurnEventClientRPC() {

		}






		public int GetPlayerMonsterCount() {
			return localPlayer.monsterCardOnField.Count;
		}



		public new bool IsServer { get { return base.IsServer; } }
		public new bool IsHost { get { return base.IsHost; } }
		public new bool IsClient { get { return base.IsClient; } }


		public CardInGame GetCardFromGuid(Guid cardGuid)
		{
			CardInGame card = localPlayer.GetCardInGameFromGuid(cardGuid);
			if (card != null)
			{
				return card;
			}

			return remotePlayer.GetCardInGameFromGuid(cardGuid);
		}
	}

	public enum GameState {
		PlayersHandShake,
		PlayersChooseHandCards,
		GameStart,
		TurnStart,
		PlayerDraw,
		PlayerHandleTurn,
		TurnEnd,
		GameEnd
	}

	#region DeckHandShakeSerialization
	
	public struct DeckCardSerializer : INetworkSerializable
	{
		public Guid guid;
		public CardInfoSO cardInfo;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref guid);
			serializer.SerializeValue(ref cardInfo);
		}
	}

	#endregion
}
