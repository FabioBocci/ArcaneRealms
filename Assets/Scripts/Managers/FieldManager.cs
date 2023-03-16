using ArcaneRealms.Scripts.Cards;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneRealms.Scripts.Managers {
	public class FieldManager : MonoBehaviour {

		public static FieldManager Instance { get; private set; }

		[Header("Positions based on monster count")]

		[SerializeField] private Transform oneMonsterPosition;
		[SerializeField] private Transform[] twoMonsterPositions;
		[SerializeField] private Transform[] threeMonsterPositions;
		[SerializeField] private Transform[] fourMonsterPositions;
		[SerializeField] private Transform[] fiveMonsterPositions;




		private List<GameObject> summonedMonsterGameObjectList = new();
		private List<Vector3> summonedMonsterOriginalPositionList = new();



		private void Awake() {
			if(Instance != null) {
				Destroy(gameObject);
				Debug.LogError("Double FieldManager Instace WTF!!!");
				return;
			}
			Instance = this;
		}


		public void TrySummonMonsterOnLocation(MonsterCard monsterCard, Vector3 location, out int index) {
			index = 0;
			if(summonedMonsterGameObjectList.Count > 4) {
				Debug.LogError("Tryied to summon a monster with full board WTF??");
				return;
			}

			if(summonedMonsterGameObjectList.Count == 0) {
				//middle of line
				GameObject go = Instantiate(monsterCard.cardInfoSO.MonsterPrefab, oneMonsterPosition.position, Quaternion.identity);
				go.transform.localScale = monsterCard.cardInfoSO.MonsterPrefabScale;
				summonedMonsterGameObjectList.Add(go);
			} else {

				index = 0;

				for(int i = 0; i < summonedMonsterGameObjectList.Count; i++) {
					Vector3 monsterWorldPosition = summonedMonsterGameObjectList[i].transform.position;
					if(monsterWorldPosition.x < location.x) {
						index++;
					} else {
						break;
					}
				}

				Transform[] newTransformLocations = GetNextTransformLocations();

				MoveMonsterSkippingIndex(index, newTransformLocations);

				GameObject go = Instantiate(monsterCard.cardInfoSO.MonsterPrefab, newTransformLocations[index].position, Quaternion.identity);
				go.transform.localScale = monsterCard.cardInfoSO.MonsterPrefabScale;
				summonedMonsterGameObjectList.Insert(index, go);

			}
			summonedMonsterOriginalPositionList.Clear();
			foreach(GameObject gameObject in summonedMonsterGameObjectList) {
				summonedMonsterOriginalPositionList.Add(gameObject.transform.position);
			}

		}


		public void MoveMonsterSkippingIndex(int indexPosition, Transform[] newTransformLocations) {
			bool found = false;
			for(int i = 0; i < summonedMonsterGameObjectList.Count + 1; i++) {
				if(i == indexPosition) {
					found = true;
					continue;
				} // Skip the index where the new monster will be placed

				GameObject gameObject = summonedMonsterGameObjectList[found ? i - 1 : i];
				Vector3 endPosition = newTransformLocations[i].position;
				LeanTween.move(gameObject, endPosition, 0.3f).setEase(LeanTweenType.easeOutQuad);
			}
		}

		public void ResetMonsterPosition() {
			for(int i = 0; i < summonedMonsterGameObjectList.Count; i++) {
				GameObject gameObject = summonedMonsterGameObjectList[i];
				Vector3 endPosition = summonedMonsterOriginalPositionList[i];
				LeanTween.move(gameObject, endPosition, 0.3f).setEase(LeanTweenType.easeOutQuad);
			}
		}

		private Transform[] GetNextTransformLocations() {

			switch(summonedMonsterGameObjectList.Count) {
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

		public void MouseOnLocationHit(Vector3 location) {

			int index = 0;

			for(int i = 0; i < summonedMonsterGameObjectList.Count; i++) {
				Vector3 monsterWorldPosition = summonedMonsterGameObjectList[i].transform.position;
				if(monsterWorldPosition.x < location.x) {
					index++;
				} else {
					break;
				}
			}
			MoveMonsterSkippingIndex(index, GetNextTransformLocations());


		}
	}
}