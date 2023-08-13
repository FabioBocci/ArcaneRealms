using System;
using ArcaneRealms.Scripts.Cards;
using System.Collections;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Interfaces;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.Utils.ArrowPointer;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

namespace ArcaneRealms.Scripts.Managers {

	/// <summary>
	/// This class is for client only, it will manage the physic implementation of the card in the 3D space. <br></br>
	/// It will also handle these event called from the local GameManager:
	/// <list type="bullet">
	/// <item> <see cref="OnMonsterSummon"> OnMonsterSummon </see> <description> ...... </description></item>
	/// <item> <see cref="OnAttackDeclaretion"> OnAttackDeclaretion </see> <description>....... </description></item>
	/// <item> <see cref="OnMonsterDie"> OnMonsterDie </see> <description> ....... </description></item>
	/// <item> <see cref="OnSpellDestroyed"> OnSpellDestroyed </see> <description> ....... </description></item>
	/// <item> <see cref="OnCardPlayed"> OnCardPlayed </see> <description> ....... </description></item>
	/// <item> <see cref="OnCardActivate"> OnCardActivate </see> <description> ....... </description></item>
	/// <item> <see cref="OnTurnChange"> OnTurnChange </see> <description> ....... </description></item>
	/// </list>
	/// </summary>
	public class FieldManager : MonoBehaviour {

		public static FieldManager Instance { get; private set; }

		[Header("Ally Positions based on monster count")]

		[SerializeField] private Transform oneMonsterPosition;
		[SerializeField] private Transform[] twoMonsterPositions;
		[SerializeField] private Transform[] threeMonsterPositions;
		[SerializeField] private Transform[] fourMonsterPositions;
		[SerializeField] private Transform[] fiveMonsterPositions;

		[Header("Enemy Positions based on monster count")]

		[SerializeField] private Transform oneMonsterPositionEnemy;
		[SerializeField] private Transform[] twoMonsterPositionsEnemy;
		[SerializeField] private Transform[] threeMonsterPositionsEnemy;
		[SerializeField] private Transform[] fourMonsterPositionsEnemy;
		[SerializeField] private Transform[] fiveMonsterPositionsEnemy;

		[Header("Other things")]
		[SerializeField] private LayerMask monsterLayerMask;
		[SerializeField] private Transform allyFatherTransform;
		[SerializeField] private Transform enemyFatherTransform;

		private List<Vector3> AllyMonsterOriginalLocationList = new();
		private List<MonsterPlatformController> AllyMonsterGameObjectList = new();

		private List<MonsterPlatformController> EnemyMonsterGameObjectList = new();

		private NavMeshSurface navMesh; //TODO - change this to A*

		private Dictionary<Guid, MonsterPlatformController> allPlatformOnField = new();
		//TODO - add PlayerPlatformController : IPlatformController

		private void Awake() {
			if(Instance != null) {
				Destroy(gameObject);
				Debug.LogError("Double FieldManager Instace WTF!!!");
				return;
			}
			Instance = this;
			navMesh = GetComponent<NavMeshSurface>();
		}



		public void Update() {
			if(Input.GetMouseButtonDown(0) && !ArrowPointerBuilder.HasRunningArrowPointer()) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit, Mathf.Infinity, monsterLayerMask)) {
					MonsterPlatformController AllyMonsterHit = AllyMonsterGameObjectList.Find(monster => monster.gameObject == hit.transform.gameObject);
					ArrowPointerBuilder.CreateBuilder()
						.SetActionCallback(monster => {
							MonsterPlatformController controller = monster as MonsterPlatformController;
							//GameManager.Instance.DeclareMonsterAttackServerRPC(AllyMonsterGameObjectList.IndexOf(AllyMonsterHit), EnemyMonsterGameObjectList.FindIndex((monsterController) => monsterController.MonsterCard == controller.MonsterCard));

						})
						.SetStartingPosition(AllyMonsterHit.GetMonsterPosition())
						.SetPredicateFilter(monster => {
							MonsterPlatformController controller = monster as MonsterPlatformController;
							return monster != null && controller.MonsterCard != null && AllyMonsterHit.MonsterCard.CanAttack(controller.MonsterCard);
						})
						.BuildArrowPointer();

				}

			}
		}

		
		public void ResetMonsters()
		{
			//TODO - implement
		}
		
		public void SummonMonsterAtPlayer(PlayerInGame playerWhoSummon, MonsterCard card, int position, Action callback)
		{
			bool allay = GameManager.Instance.IsLocal(playerWhoSummon);
			if (allay)
			{
				StartCoroutine(SummonAllayMonster(card, position, callback));
			}
			else
			{
				StartCoroutine(SummonEnemyMonster(card, position, callback));
			}
		}
		
		public void DeclareAttack(MonsterCard card, IDamageable target, Action afterAttackDeclarationCallback)
		{
			
		}
		
		public void HandleVisualAttack(IDamageable dataAttacker, IDamageable dataDefender, int dataAttackerAttack, int dataDefenderAttack, Action afterAttackVisual)
		{
			//Debug.Log("Client ID: " + clientID + " attacker ID: " + attackerIndex + " defender ID: " + defenderIndex);

			//MonsterPlatformController attacker = GetMonsterPlatformControllerFromIndex(clientID == NetworkManager.Singleton.LocalClientId, attackerIndex);
			//MonsterPlatformController defender = GetMonsterPlatformControllerFromIndex(clientID != NetworkManager.Singleton.LocalClientId, defenderIndex);
			MonsterPlatformController attacker = allPlatformOnField[dataAttacker.GetUnique()];
			MonsterPlatformController defender = allPlatformOnField[dataAttacker.GetUnique()];
			attacker.Attack(defender, afterAttackVisual);
			//attacker.Attack(defender);
		}
		
		
		public void DestroyMonster(PlayerInGame playerD, CardInGame cardToRemove, Action animationEnded)
		{
			
		}
		
		public void PlayCardAnimation(PlayerInGame player, CardInGame card, Action action)
		{
			
		}
		
		
		public void CloseCardAnimation(PlayerInGame player, CardInGame card)
		{
			
		}
		

		#region RegionSummonUtils
		
		public void MoveMonsterSkippingIndex(List<MonsterPlatformController> monsters, int indexPosition, Transform[] newTransformLocations) {
			bool found = false;
			for(int i = 0; i < monsters.Count + 1; i++) {
				if(i == indexPosition) {
					found = true;
					continue;
				} // Skip the index where the new monster will be placed

				MonsterPlatformController monsterPlatformController = monsters[found ? i - 1 : i];
				GameObject gameObject = monsterPlatformController.gameObject;
				Vector3 endPosition = newTransformLocations[i].position;
				LeanTween.move(gameObject, endPosition, 0.3f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
					monsterPlatformController.UpdateMonsterLocation();
				});
				
			}
		}

		public void ResetMonsterPosition() {
			for(int i = 0; i < AllyMonsterGameObjectList.Count; i++) {
				GameObject gameObject = AllyMonsterGameObjectList[i].gameObject;
				Vector3 endPosition = AllyMonsterOriginalLocationList[i];
				LeanTween.move(gameObject, endPosition, 0.3f).setEase(LeanTweenType.easeOutQuad);
			}
		}
		
		public void MouseOnLocationHit(Vector3 location) {

			int index = 0;

			for(int i = 0; i < AllyMonsterGameObjectList.Count; i++) {
				Vector3 monsterWorldPosition = AllyMonsterGameObjectList[i].transform.position;
				if(monsterWorldPosition.x < location.x) {
					index++;
				} else {
					break;
				}
			}
			MoveMonsterSkippingIndex(AllyMonsterGameObjectList, index, GetNextAllyTransformLocations());
		}



		private Transform[] GetNextAllyTransformLocations() {

			switch(AllyMonsterGameObjectList.Count) {
				case 1:
					return twoMonsterPositions;
				case 2:
					return threeMonsterPositions;
				case 3:
					return fourMonsterPositions;
				case 4:
					return fiveMonsterPositions;

			}

			return null;
		}

		private Transform[] GetNextEnemyTransformLocations() {

			switch(EnemyMonsterGameObjectList.Count) {
				case 0:
					return new[] { oneMonsterPositionEnemy };
				case 1:
					return twoMonsterPositionsEnemy;
				case 2:
					return threeMonsterPositionsEnemy;
				case 3:
					return fourMonsterPositionsEnemy;
				case 4:
					return fiveMonsterPositionsEnemy;
			}
			return null;
		}

		private MonsterPlatformController InstanziateMonsterCard(MonsterCard monsterCard, Transform location, bool isEnemy) {
			GameObject go = Instantiate(monsterCard.cardInfoSO.MonsterPrefab, location.position, Quaternion.identity);
			go.transform.localScale = monsterCard.cardInfoSO.MonsterPrefabScale;
			go.transform.rotation = new Quaternion(0, isEnemy ? 180 : 0, 0, 0);
			go.transform.SetParent(isEnemy ? enemyFatherTransform : allyFatherTransform);
			MonsterPlatformController monsterInGame = go.GetComponent<MonsterPlatformController>();
			monsterInGame.MonsterCard = monsterCard;
			return monsterInGame;
		}

		IEnumerator SummonAllayMonster(MonsterCard card, int index, Action callback) {
			Transform[] allayTransforms = GetNextAllyTransformLocations();
			MoveMonsterSkippingIndex(AllyMonsterGameObjectList, index, allayTransforms);

			yield return new WaitForSeconds(0.3f);
			SummonMonsterOnAllayLocation(card, index);
			
			AllyMonsterOriginalLocationList.Clear();
			foreach(MonsterPlatformController allyMonsterPlatformInfo in AllyMonsterGameObjectList) {
				AllyMonsterOriginalLocationList.Add(allyMonsterPlatformInfo.transform.position);
			}
			
			callback?.Invoke();
		}
		
		IEnumerator SummonEnemyMonster(MonsterCard card, int index, Action callback) {
			Transform[] enemyTransform = GetNextEnemyTransformLocations();
			MoveMonsterSkippingIndex(EnemyMonsterGameObjectList, index, enemyTransform);

			yield return new WaitForSeconds(0.3f);
			SummonMonsterOnEnemyLocation(card, index);
			callback?.Invoke();
		}

		public void SummonMonsterOnEnemyLocation(MonsterCard monsterCard, int index) {
			Transform[] enemyTransform = GetNextEnemyTransformLocations();
			MonsterPlatformController go = InstanziateMonsterCard(monsterCard, enemyTransform[index], true);
			if(EnemyMonsterGameObjectList.Count >= index) {
				EnemyMonsterGameObjectList.Add(go);
			} else {
				EnemyMonsterGameObjectList.Insert(index, go);
			}

			navMesh.BuildNavMesh();
		}

		public void SummonMonsterOnAllayLocation(MonsterCard monsterCard, int index) {
			Transform[] allayTransforms = GetNextAllyTransformLocations();
			MonsterPlatformController go = InstanziateMonsterCard(monsterCard, allayTransforms[index], false);
			if(AllyMonsterGameObjectList.Count >= index) {
				AllyMonsterGameObjectList.Add(go);
			} else {
				AllyMonsterGameObjectList.Insert(index, go);
			}

			navMesh.BuildNavMesh();
		}
		
		#endregion


	}
}