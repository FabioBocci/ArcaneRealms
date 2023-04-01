using ArcaneRealms.Scripts.Interfaces;
using System;
using UnityEngine;

namespace ArcaneRealms.Scripts.ArrowPointer {
	public class ArrowPointerBuilder : MonoBehaviour {

		[SerializeField] private GameObject arrowPointerPrefab;
		[SerializeField] private GameObject arrowTailPrefab;
		[SerializeField] private LayerMask terreinLayerMask;
		[SerializeField] private LayerMask monsterLayerMask;

		private Transform startingPosition = null;
		private Action<ITargetable> actionCallback = null;
		private Predicate<ITargetable> predicateFilter = null;

		private static ArrowPointerBuilder Instance;

		private static ArrowPointer arrowPointerStatic = null;

		private void Awake() {
			Instance = this;
		}

		public ArrowPointerBuilder SetStartingPosition(Transform startingPoint) {
			startingPosition = startingPoint;
			return this;
		}

		public ArrowPointerBuilder SetActionCallback(Action<ITargetable> action) {
			actionCallback = action;
			return this;
		}

		public ArrowPointerBuilder SetPredicateFilter(Predicate<ITargetable> action) {
			predicateFilter = action;
			return this;
		}

		public void BuildArrowPointer() {
			ArrowPointer arrowPointer = Instantiate(arrowPointerPrefab, transform).AddComponent<ArrowPointer>();
			arrowPointer.name = "ArrowPointer";
			arrowPointer.tailSegmentPrefab = arrowTailPrefab;
			arrowPointer.terreinLayerMask = terreinLayerMask;
			arrowPointer.monsterLayerMask = monsterLayerMask;
			arrowPointer.predicate = predicateFilter;
			arrowPointer.action = actionCallback;
			arrowPointer.arrowStartingPosition = startingPosition;
			arrowPointerStatic = arrowPointer;
		}

		public static ArrowPointerBuilder CreateBuilder() {
			return Instance;
		}

		public static bool HasRunningArrowPointer() {
			return arrowPointerStatic != null && arrowPointerStatic.gameObject != null && arrowPointerStatic.gameObject.activeSelf;
		}

	}
}