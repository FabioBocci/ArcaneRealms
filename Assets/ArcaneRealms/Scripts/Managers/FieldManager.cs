using ArcaneRealms.Scripts.Cards;
using System.Collections;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.GameCards;
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

		private NavMeshSurface navMesh;


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
							GameManager.Instance.DeclareMonsterAttackServerRPC(AllyMonsterGameObjectList.IndexOf(AllyMonsterHit), EnemyMonsterGameObjectList.FindIndex((monsterController) => monsterController.MonsterCard == controller.MonsterCard));

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



		public void TrySummonMonsterOnLocation(MonsterCard monsterCard, Vector3 location, out int index, out Transform monsterTransform) {
			monsterTransform = null;
			index = 0;
			if(AllyMonsterGameObjectList.Count > 4) {
				Debug.LogError("Tryied to summon a monster with full board WTF??");
				return;
			}

			if(AllyMonsterGameObjectList.Count == 0) {
				//middle of line
				MonsterPlatformController go = InstanziateMonsterCard(monsterCard, oneMonsterPosition, false);
				AllyMonsterGameObjectList.Add(go);
				monsterTransform = go.transform;
			} else {

				index = 0;

				for(int i = 0; i < AllyMonsterGameObjectList.Count; i++) {
					Vector3 monsterWorldPosition = AllyMonsterGameObjectList[i].transform.position;
					if(monsterWorldPosition.x < location.x) {
						index++;
					} else {
						break;
					}
				}

				Transform[] newTransformLocations = GetNextAllyTransformLocations();

				MoveMonsterSkippingIndex(AllyMonsterGameObjectList, index, newTransformLocations);

				MonsterPlatformController go = InstanziateMonsterCard(monsterCard, newTransformLocations[index], false);
				AllyMonsterGameObjectList.Insert(index, go);
				monsterTransform = go.transform;

			}
			AllyMonsterOriginalLocationList.Clear();
			foreach(MonsterPlatformController allyMonsterPlatformInfo in AllyMonsterGameObjectList) {
				AllyMonsterOriginalLocationList.Add(allyMonsterPlatformInfo.transform.position);
			}

		}


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
					return new Transform[1] { oneMonsterPositionEnemy };
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

		private MonsterPlatformController GetMonsterPlatformControllerFromIndex(bool allyMonster, int index) {
			if(allyMonster) {
				return AllyMonsterGameObjectList[index];
			} else {
				return EnemyMonsterGameObjectList[index];
			}

		}


		public void OnMonsterAttackDeclared(Component sender, object objects) {
			if(objects == null) {
				return;
			}

			object[] paramsArr = objects as object[];

			ulong clientID = (ulong) paramsArr[0];
			int attackerIndex = (int) paramsArr[1];
			int defenderIndex = (int) paramsArr[2];
			bool attackerIsDead = (bool) paramsArr[3];
			bool defenderIsDead = (bool) paramsArr[4];

			//Debug.Log("Client ID: " + clientID + " attacker ID: " + attackerIndex + " defender ID: " + defenderIndex);

			MonsterPlatformController attacker = GetMonsterPlatformControllerFromIndex(clientID == NetworkManager.Singleton.LocalClientId, attackerIndex);
			MonsterPlatformController defender = GetMonsterPlatformControllerFromIndex(clientID != NetworkManager.Singleton.LocalClientId, defenderIndex);


			attacker.Attack(defender);
		}

		public void OnEnemyPlaycardEvent(Component component, object parameters) {
			object[] parametersArray = parameters as object[];
			MonsterCard card = (MonsterCard) parametersArray[1];
			int index = (int) parametersArray[2];

			StartCoroutine(SummonEnemyMonster(card, index));
		}

		IEnumerator SummonEnemyMonster(MonsterCard card, int index) {
			Transform[] enemyTransform = GetNextEnemyTransformLocations();
			MoveMonsterSkippingIndex(EnemyMonsterGameObjectList, index, enemyTransform);

			yield return new WaitForSeconds(0.3f);
			SummonMonsterOnEnemyLocation(card, index);
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

	}
}