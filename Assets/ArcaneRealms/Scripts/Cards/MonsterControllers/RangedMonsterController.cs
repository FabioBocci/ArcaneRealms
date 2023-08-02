using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace ArcaneRealms.Scripts.Cards.MonsterControllers {

	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(Animator))]
	public class RangedMonsterController : MeleeMonsterController {

		public float rangeForDistanceCheck = 10;

		public new void MoveTo(Vector3 targetPosition, GameObject target, Action endMoveAction = null) {
			spawnPoint = transform.position;
			spawnRotation = transform.rotation;
			agent.destination = targetPosition;
			StartCoroutine(RunToTargetAndPerformAction(target, endMoveAction));
		}

		public new void PerformAttack(Transform target) {
			throw new System.NotImplementedException();
		}

		public new bool IsRanged() {
			return true;
		}


		protected new IEnumerator RunToTargetAndPerformAction(GameObject target, Action action = null) {
			while(true) {
				float distance = Vector3.Distance(transform.position, target.transform.position);
				if(agent.destination != target.transform.position) {
					//agent.destination = target.transform.position;
				}

				if(distance <= rangeForDistanceCheck) {
					action?.Invoke();
					break;
				}

				yield return new WaitForSeconds(0.05f);
			}
			agent.isStopped = false;
		}




	}
}