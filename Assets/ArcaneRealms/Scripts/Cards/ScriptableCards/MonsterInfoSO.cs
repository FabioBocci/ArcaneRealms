using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Enums;
using UnityEngine;

namespace ArcaneRealms.Scripts.SO {

	[CreateAssetMenu(fileName = "New Monster", menuName = "Cards/New Monster", order = 0)]
	public class MonsterInfoSO : CardInfoSO {
		[Header("Monster Stats")]
		public int Atk;
		public int Health;
		public Race Race;

		public GameObject MonsterPrefab;

		public Vector3 MonsterPrefabScale;

		public GameObject MonsterSummonParticlePrefab;

	}
}