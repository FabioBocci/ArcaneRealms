using ArcaneRealms.Scripts.Interfaces;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Ara.Scripts.Cards.MonsterControllers {

	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(Animator))]
	public class MeleeMonsterController : MonoBehaviour, IMonsterController {

		protected NavMeshAgent agent;
		protected Animator animator;
		protected Vector3 spawnPoint;
		protected Quaternion spawnRotation;
		protected Vector2 smoothDeltaPosition = Vector2.zero;
		protected Vector2 velocity = Vector2.zero;


		private void Awake() {
			agent = GetComponent<NavMeshAgent>();
			agent.updatePosition = false; // Don’t update position automatically so we can use Animator
			animator = GetComponent<Animator>();
		}

		private void Update() {
			Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

			// Map 'worldDeltaPosition' to local space
			float dx = Vector3.Dot(transform.right, worldDeltaPosition);
			float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
			Vector2 deltaPosition = new Vector2(dx, dy);

			// Low-pass filter the deltaMove
			float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
			smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

			// Update velocity if time advances
			if(Time.deltaTime > 1e-5f)
				velocity = smoothDeltaPosition / Time.deltaTime;

			bool shouldMove = velocity.magnitude > 0.9f && agent.remainingDistance > agent.radius;

			// Update animation parameters
			animator.SetFloat("Velocity", shouldMove ? velocity.magnitude : 0f);

		}

		private void OnAnimatorMove() {
			// Update position to agent position
			if(agent.hasPath && agent.velocity.sqrMagnitude > 0f) {
				transform.position = agent.nextPosition;
			}
		}

		public bool IsMoving() {
			if(!agent.pathPending) {
				if(agent.remainingDistance <= agent.stoppingDistance) {
					if(!agent.hasPath || agent.velocity.sqrMagnitude == 0f) {
						// Done
						Debug.Log("Executes 2 times");
						return false;

					}
				}
			}
			return true;

		}

		public void MoveTo(Vector3 targetPosition, GameObject target, Action endAction = null) {
			spawnPoint = transform.position;
			spawnRotation = transform.rotation;
			agent.destination = targetPosition;
			StartCoroutine(RunToTargetAndPerformAction(target, endAction));
		}


		public void MoveToBase() {
			agent.destination = spawnPoint;
			StartCoroutine(FaceOriginalDirection());

		}

		public void PerformAttack(Transform target) {
			animator.SetBool("Attack", true);
		}

		public void ResetState() {
			animator.SetBool("Attack", false);
			animator.SetFloat("Velocity", 0f);
		}

		public bool IsRanged() {
			return false;
		}

		public void UpdateAgentLocation() {
			if(agent != null) {
				agent.Warp(transform.position);
			}
		}


		protected virtual IEnumerator RunToTargetAndPerformAction(GameObject target, Action action) {
			while(true) {
				float distance = Vector3.Distance(transform.position, target.transform.position);
				if(agent.destination != target.transform.position) {
					//agent.destination = target.transform.position;
				}


				if(distance <= agent.stoppingDistance) {
					action?.Invoke();
					break;
				}

				yield return new WaitForSeconds(0.05f);

			}

			agent.isStopped = false;
		}

		IEnumerator FaceOriginalDirection() {
			while(true) {
				float distance = Vector3.Distance(transform.position, spawnPoint);



				if(distance <= 0.1f) {
					LeanTween.rotate(gameObject, spawnRotation.eulerAngles, 0.5f);
					break;
				}

				yield return new WaitForSeconds(0.1f);

			}

			agent.isStopped = false;
		}

	}
}