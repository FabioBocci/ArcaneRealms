
using System;
using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Events;
using ArcaneRealms.Scripts.Interfaces;
using ArcaneRealms.Scripts.Systems;
using ArcaneRealms.Scripts.Utils.Events;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArcaneRealms.Scripts.Managers {
	
	public class GameManager : NetworkBehaviour {

		[Header("Game Infos")]
		[SerializeField] private int startingHandSize = 3;


		#region Events
		
		public event EntityEvent<List<CardInGame>> OnStartingCardsReceived;
		public event EntityEvent<PlayerInGame> OnPlayerDeath;
		public event EntityEvent<CardInGame> OnBeforeCardDestroy;
		public event EntityEvent<CardInGame> OnAfterCardDestroy;
		public event AttackEntityEvent OnAttackDeclaration;	 //on attack declaration
		public event AttackEntityEvent OnAttackResult; //after damage calculation
		public event EntityEvent<MonsterCard> OnMonsterSummon;			//monster who is summoned
		public event EntityEvent<MonsterCard> OnMonsterDying;			//monster who is dying
		public event EntityEvent<CardInGame> OnCardDraw;				//playerWhoDraw cardThatHasBeenDraw
		public event EntityEvent<CardInGame> OnCardPlayed;
		//public event EntityEvent<EntityEventData<CardInGame>> OnCardPlayed;						//playerWhoDraw cardThatHasBeenDraw

		
		#endregion
		
		public static GameManager Instance { get; private set; }
		
		private PlayerInGame localPlayer;
		private PlayerInGame remotePlayer;

		[HideInInspector]
		public NetworkVariable<Guid> playerTurn = new();

		[SerializeField] private GameState gameState;

		private void Awake() {
			if(Instance != null) {
				Destroy(gameObject);
				return;
			}
			Instance = this;
			UserNetworkVariableSerialization<Guid>.WriteValue = SerializerHelper.WriteValueSafe;
			UserNetworkVariableSerialization<Guid>.ReadValue = SerializerHelper.ReadValueSafe;
			UserNetworkVariableSerialization<Guid>.DuplicateValue = (in Guid value, ref Guid duplicatedValue) =>
			{
				duplicatedValue = value;
			};

		}

		public override void OnNetworkSpawn()
		{
			if(IsServer) {
				gameState = GameState.PlayersHandShake;
				StartHandlePlayersHandShake();
				StartCoroutine(HandleGameStateCoroutine());
			}
		}

		#region RegionHandlers
		
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
			gameState = GameState.PlayersWaitChooseHandCards;
		}

		private void HandleGameEnded()
		{
			//TODO - run client rpc.
		}

		#region LocallyHandlers

		private async Task HandlePlayerPlayCardLocally(PlayerInGame player, CardInGame card, int position = -1)
		{
			if (card.IsMonsterCard(out var monster))
			{
				await HandlePlayerSummonLocally(player, monster, position);
				return;
			}
			
			if (card.GetManaCost() > 0)
			{
				player.PayMana(card.GetManaCost());
			}
			
			//not a monster card? use generic events
			player.PlayCard(card, position);
			
			if (NetworkManagerHelper.Instance.IsClient)
			{
				await FieldManager.Instance.PlayCardAnimation(player, card);
			}

			if (OnCardPlayed != null)
			{
				EntityEventData<CardInGame> data = new EntityEventData<CardInGame>(card);
				await OnCardPlayed.Invoke(ref data);
				if (!data.IsCancelled)
				{
					await card.OnCardActivation();
					if (card.IsSpellCard(out var spell))
					{
						if (spell.cardInfoSO.SpellType == SpellType.Normal)
						{
							spell.position = CardPosition.Graveyard;
							player.graveyardList.Add(card);
						}
					}
				}
				else
				{
					card.position = CardPosition.Graveyard;
					player.graveyardList.Add(card);
				}
				
				if (NetworkManagerHelper.Instance.IsClient)
				{
					await FieldManager.Instance.CloseCardAnimation(player, card);
				}
				
				await ResolveFinalCallbacks();
			}
		}
		
		private async Task HandlePlayerSummonLocally(PlayerInGame playerWhoSummon, MonsterCard card, int position)
		{
			if (card.GetManaCost() > 0)
			{
				playerWhoSummon.PayMana(card.GetManaCost());
			}
			
			playerWhoSummon.PlayCard(card, position);
			
			if (NetworkManagerHelper.Instance.IsClient)
			{
				//Client or Host
				await FieldManager.Instance.SummonMonsterAtPlayer(playerWhoSummon, card, position);
				
			}
			
			EntityEventData<MonsterCard> data = new EntityEventData<MonsterCard>(card);
			if (OnMonsterSummon != null)
			{
				await OnMonsterSummon.Invoke(ref data);
			}
			//TODO battle cry
			await ResolveFinalCallbacks();
		}
		
		private async Task HandleAttackLocally(PlayerInGame player, MonsterCard card, IDamageable target)
		{
			if (NetworkManagerHelper.Instance.IsClient)
			{
				//visual and other things...
				await FieldManager.Instance.DeclareAttack(card, target);
			}
			
			AttackEntityEventData eventData = 
				new AttackEntityEventData(
					player, card, target, card.GetAttack(), target.GetTargetType() == TargetType.MonsterCard ? ((MonsterCard) target).GetAttack() : 0);
			if (OnAttackDeclaration != null)
			{
				await OnAttackDeclaration.Invoke(ref eventData);
			}

			if (eventData.IsCancelled && NetworkManagerHelper.Instance.IsClient)
			{
				await FieldManager.Instance.ResetMonsters();
				return;
			}

			#region DebugLog

			if (Debug.isDebugBuild || Application.isEditor)
			{
				string targetName = target.GetTargetType() == TargetType.Player
					? "OtherPlayer"
					: GetCardFromGuid(target.GetUnique()).cardInfoSO.name;
				Debug.Log(
					$"Attack Declaration successful | Attacker {card.cardInfoSO.Name} | Target {targetName}");
			}

			#endregion
			
			if (NetworkManagerHelper.Instance.IsClient)
			{
				await FieldManager.Instance.HandleVisualAttack(eventData.Attacker, eventData.Defender, eventData.AttackerAttack,
					eventData.DefenderAttack);
			}
			
			//do damage and run event after attack then alive checks
			eventData.Attacker.Damage(eventData.AttackerAttack);
			eventData.Defender.Damage(eventData.DefenderAttack);
			
			eventData = new AttackEntityEventData(player, eventData.Attacker, eventData.Defender, eventData.AttackerAttack, eventData.DefenderAttack);
			if (OnAttackResult != null)
			{
				await OnAttackResult.Invoke(ref eventData);
			}
			
			if (!AliveCheck(localPlayer) || !AliveCheck(remotePlayer))
			{
				gameState = GameState.GameEnd;
				return;
			}
			
			if (AliveCheck(eventData.Defender))
			{
				//we know for sure that this is a card since we are checking the players before this
				PlayerInGame playerD = GetPlayerFromID(eventData.Defender.GetTeam());
				CardInGame cardToRemove = playerD.GetCardInGameFromGuid(eventData.Defender.GetUnique());
				RegisterFinalCallback(async () =>
				{
					playerD.RemoveCard(cardToRemove);

					if (NetworkManagerHelper.Instance.IsClient)
					{
						await FieldManager.Instance.DestroyMonster(playerD, cardToRemove);
					}
					var entityEventData = new EntityEventData<CardInGame>(cardToRemove);
					if (OnAfterCardDestroy != null)
					{
						await OnAfterCardDestroy.Invoke(ref entityEventData);
					}
					
					
				});
			}

			if (AliveCheck(eventData.Attacker))
			{
				//we know for sure that this is a card since we are checking the players before this
				PlayerInGame playerD = GetPlayerFromID(eventData.Defender.GetTeam());
				CardInGame cardToRemove = playerD.GetCardInGameFromGuid(eventData.Defender.GetUnique());
				RegisterFinalCallback(async () =>
				{
					playerD.RemoveCard(cardToRemove);

					if (NetworkManagerHelper.Instance.IsClient)
					{
						await FieldManager.Instance.DestroyMonster(playerD, cardToRemove);
					}
					var entityEventData = new EntityEventData<CardInGame>(cardToRemove);
					if (OnAfterCardDestroy != null)
					{
						await OnAfterCardDestroy.Invoke(ref entityEventData);
					}
					
				});
			}
					
			await ResolveFinalCallbacks();
		}
		
		

		#endregion
		
		#endregion

		#region RegionCoroutineHandleGameStates
		
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
						HandleGameEnded();
						break;
					case GameState.GameEnded:
						break;

				}

			}
			//Debug.Log("HandleGameStateCoroutine Finished!");
		}


		#endregion


		//----------------------Server RPCs-------------------------------

		#region RegionServerRPCs

		private Dictionary<PlayerInGame, List<DeckCardSerializer>> handShake;
		private Dictionary<PlayerInGame, bool> handShakeSecond;

		[ServerRpc(RequireOwnership = false)]
		private void PlayerSendHandShakeServerRPC(Guid playerGuid, List<CardInDeck> deck, string avatarId, ServerRpcParams parameters = default)
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
				Debug.Log("HandShake ended!");
				SendPlayersDataAfterHandShakeClientRPC(localPlayer.ID, handShake[localPlayer].ToArray(), remotePlayer.ID, handShake[remotePlayer].ToArray());
				handShake = null;
				gameState = GameState.PlayersChooseHandCards;
			}
			
		}

		[ServerRpc(RequireOwnership = false)]
		private void PlayerChooseCardsServerRPC(PlayerInGame playerInGame, List<CardInGame> cardsChosen, List<CardInGame> cardNotChosen)
		{
			if (gameState != GameState.PlayersWaitChooseHandCards)
			{
				Debug.LogError($"[ServerRpc] Received PlayerChooseCardsServerRPC when state was: {gameState} from player: {playerInGame.playerUlong}");
				return;
			}

			if (handShakeSecond == null)
			{
				handShakeSecond = new Dictionary<PlayerInGame, bool>
				{
					[localPlayer] = false,
					[remotePlayer] = false
				};
			}

			handShakeSecond[playerInGame] = true;
			
			
			foreach (var cardInGame in cardsChosen)
			{
				playerInGame.currentDeck.Remove(cardInGame);
				playerInGame.handCards.Add(cardInGame);
				cardInGame.position = CardPosition.Hand;
			}

			List<CardInGame> newCards = new();
			foreach (var cardInGame in cardNotChosen)
			{
				CardInGame card = playerInGame.currentDeck[Random.Range(0, playerInGame.currentDeck.Count - 1)];
				playerInGame.currentDeck.Remove(card);
				playerInGame.handCards.Add(card);
				card.position = CardPosition.Hand;
				newCards.Add(card);
			}


			SendFinalPlayersStartingHandsClientRPC(playerInGame, cardsChosen, newCards); 
			//sending this to both player so they can keep the track

			if (handShakeSecond[localPlayer] && handShakeSecond[remotePlayer])
			{
				gameState = GameState.TurnStart;
				handShakeSecond = null;
				SetPlayerTurn(localPlayer);
			}

		}
		
		[ServerRpc(RequireOwnership = false)]
		private void HandlePlayerPlayCardRemoteServerRPC(PlayerInGame playerWhoSummon, CardInGame card, int position = -1)
		{
			if (!CanPlay(playerWhoSummon, card))
			{
				Debug.LogError($"[ServerRPC] - a player tried to play a card when he couldn't! card name: {card.cardInfoSO.Name}");
				return;
			}

			if (!NetworkManagerHelper.Instance.IsHost || playerWhoSummon.ID != localPlayer.ID)
			{
				HandlePlayerPlayCardLocally(playerWhoSummon, card, position);
			}
			
			PlayerInGame enemy = GetEnemyPlayer(playerWhoSummon);
			HandlePlayerPlayCardRemoteClientRPC(playerWhoSummon, card, position, enemy.thisClientRpcTarget);
			
		}
		
		[ServerRpc(RequireOwnership = false)]
		private void HandleAttackRemoteServerRPC(PlayerInGame playerWhoAttack, CardInGame card, IDamageable target)
		{
			if (!card.IsMonsterCard(out var monster))
			{
				return;
			}
			
			if (!IsPlayerTurn(playerWhoAttack))
			{
				Debug.LogError($"[ServerRPC] Somehow a player card is attacking not in player turn? card name: {card.cardInfoSO.Name}");
				return;
			}
			//TODO - CanAttack(monster, target);

			if (!NetworkManagerHelper.Instance.IsHost && playerWhoAttack.ID != localPlayer.ID)
			{
				//don't run the Attack locally if we are an host and this rpc is called from this client!
				//aka don't run HandleAttackLocally(...) twice in the same client if he is the host
				HandleAttackLocally(playerWhoAttack, monster, target);
			}

			PlayerInGame enemy = GetEnemyPlayer(playerWhoAttack);
			HandleAttackRemoteClientRPC(playerWhoAttack, monster, target, enemy.thisClientRpcTarget);
		}



		#endregion
		
		// -------------------Client RPCs--------------------------------

		#region RegionClientRPCs
		
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
			if (thisPlayer != localPlayer)
			{
				Debug.LogError($" Received a ClientRPC for a different client! Aborting RPC! this client {localPlayer.playerUlong} received {thisPlayer.playerUlong}");
				return;
			}
			Debug.Log("Cards received: ");
			foreach (var card in startingHand)
			{
				Debug.Log(card.cardInfoSO.Name);
			}

			HandUIManager.Instance.OnStartingCardsReceived(startingHand);
		}
		
		[ClientRpc]
		private void SendFinalPlayersStartingHandsClientRPC(PlayerInGame playerInGame, List<CardInGame> cardsChosen, List<CardInGame> newCards)
		{
			
			foreach (var cardInGame in cardsChosen.Concat(newCards))
			{
				playerInGame.currentDeck.Remove(cardInGame);
				playerInGame.handCards.Add(cardInGame);
				cardInGame.position = CardPosition.Hand;
			}

			if (localPlayer == playerInGame)
			{
				//Update visual
				HandUIManager.Instance.FinalStartingCardsReceived(cardsChosen, newCards);
			}
			
		}

		[ClientRpc]
		private void HandlePlayerPlayCardRemoteClientRPC(PlayerInGame playerWhoSummon, CardInGame card,
			int position = -1,  ClientRpcParams clientRpcParams = default)
		{
			if (playerWhoSummon.ID == localPlayer.ID)
			{
				Debug.LogError($"[ClientRpc] Why did i received this client rpc when it was for the enemy? ");
				return;
			}

			if (NetworkManagerHelper.Instance.IsServer)
			{
				return;
			}
			
			HandlePlayerPlayCardLocally(playerWhoSummon, card, position);
		}
		
		[ClientRpc]
		private void HandleAttackRemoteClientRPC(PlayerInGame playerWhoAttack, CardInGame card, IDamageable target, ClientRpcParams clientRpcParams = default)
		{
			if (!card.IsMonsterCard(out var monster))
			{
				return;
			}
			
			if (playerWhoAttack.ID == localPlayer.ID)
			{
				Debug.LogError($"[ClientRpc] Why did i received this client rpc when it was for the enemy? ");
				return;
			}
			
			if (NetworkManagerHelper.Instance.IsServer)
			{
				return;
			}
			
			HandleAttackLocally(playerWhoAttack, monster, target);
		}
		

		[ClientRpc]
		public void EndTurnEventClientRPC() {

		}

		[ClientRpc]
		public void StartTurnEventClientRPC() {
			
		}


		#endregion

		// ----------------------Client Actions--------------------------------------

		#region RegionClientActions

		public void PlayerChooseCards(List<CardInGame> cardsChosen, List<CardInGame> cardNotChosen)
		{
			//send server rpc
			PlayerChooseCardsServerRPC(localPlayer, cardsChosen, cardNotChosen);
		}

		public void PlayCard(CardInGame card, int destPos = -1)
		{
			if (!CanPlay(localPlayer, card))
			{
				Debug.LogError("Trying to summon a monster when is not possible!");
				return;
			}

			HandlePlayerPlayCardLocally(localPlayer, card, destPos);
			HandlePlayerPlayCardRemoteServerRPC(localPlayer, card, destPos);
		}

		public void DeclareAttack(PlayerInGame player, MonsterCard card, IDamageable target)
		{
			if (!IsPlayerTurn(player)) return;
			//if (!CanAttack(card, target)) Debug.Log("...."); return;

			HandleAttackLocally(player, card, target);
			HandleAttackRemoteServerRPC(player, card, target);
		}

		
		#endregion

		// -------------------------Utils-------------------------------------
		
		#region RegionUtils

		public bool IsMyTurn() => localPlayer.ID == playerTurn.Value;

		public bool IsLocal(PlayerInGame playerInGame) => localPlayer.ID == playerInGame.ID;
		
		public bool IsPlayerTurn(PlayerInGame player) => player.ID == playerTurn.Value;

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


		public PlayerInGame GetEnemyPlayer(PlayerInGame owner)
		{
			return owner.ID == localPlayer.ID ? remotePlayer : localPlayer;
		}
		
		public int GetPlayerMonsterCount() {
			return localPlayer.monsterCardOnField.Count;
		}

		public CardInGame GetCardFromGuid(Guid cardGuid)
		{
			CardInGame card = localPlayer.GetCardInGameFromGuid(cardGuid);
			if (card != null)
			{
				return card;
			}

			return remotePlayer.GetCardInGameFromGuid(cardGuid);
		}
		
		public ITargetable GetTarget(TargetType targetType, Guid uniqueId, Guid team)
		{
			if (targetType == TargetType.Player)
			{
				return GetPlayerFromID(uniqueId);
			}

			PlayerInGame player = GetPlayerFromID(team);

			return player.GetCardInGameFromGuid(uniqueId);

		}

		private bool AliveCheck(IDamageable damageable)
		{
			if (damageable.IsAlive())
			{
				return true;
			}
			
			if (damageable.GetTargetType() == TargetType.Player)
			{
				var entityEventData = new EntityEventData<PlayerInGame>(GetPlayerFromID(damageable.GetUnique()));
				OnPlayerDeath?.Invoke(ref entityEventData);
				
			}

			if (damageable.GetTargetType() == TargetType.MonsterCard)
			{
				var entityEventData = new EntityEventData<CardInGame>(GetPlayerFromID(damageable.GetTeam()).GetCardInGameFromGuid(damageable.GetUnique()));
				OnBeforeCardDestroy?.Invoke(ref entityEventData);
			}

			return false;
		}
		
		private bool CanPlay(PlayerInGame playerInGame, CardInGame card)
		{
			return true;
			return IsPlayerTurn(playerInGame) && playerInGame.CurrentUsableMana >= card.GetManaCost() &&
			       (!card.IsMonsterCard(out var monster) || playerInGame.monsterCardOnField.Count < 5);
		}


		private Func<Task> finalCallback;

		public void RegisterFinalCallback(Func<Task> callback)
		{
			finalCallback += callback;
		}

		public async Task ResolveFinalCallbacks()
		{
			Func<Task> finalCallbackClone = finalCallback;
			finalCallback = null;
			if (finalCallbackClone != null)
			{
				await finalCallbackClone.Invoke();
			}
		}

		#endregion


	}

	public enum GameState {
		PlayersHandShake,
		PlayersChooseHandCards,
		PlayersWaitChooseHandCards,
		TurnStart,
		PlayerDraw,
		PlayerHandleTurn,
		TurnEnd,
		GameEnd,
		GameEnded
	}

	public enum ClientGameState
	{
		Initializing,
		PlayerTurn,
		Resolving,
		GameEnded
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
