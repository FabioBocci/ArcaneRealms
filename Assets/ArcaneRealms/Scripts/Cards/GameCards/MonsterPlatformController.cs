using System;
using System.Collections;
using ArcaneRealms.Scripts.Cards.MonsterControllers;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Interfaces;
using UnityEngine;

namespace ArcaneRealms.Scripts.Cards.GameCards {
	public class MonsterPlatformController : MonoBehaviour, ITargetable {
		[SerializeField] private Transform basePlatform;
		[SerializeField] private Transform monsterPositionOnPlate;


		public LayerMask terreinLayerMask;
		public MonsterCard MonsterCard { set; get; }

		private IMonsterController monsterController;

		private void Awake() {
			monsterController = monsterPositionOnPlate.GetComponent<IMonsterController>();
		}


		public Transform GetMonsterPosition() {
			return monsterPositionOnPlate;
		}

		public Guid GetTeam() => MonsterCard.GetTeam();

		public TargetType GetTargetType() => MonsterCard.GetTargetType();
		
		public Guid GetUnique() => MonsterCard.CardGuid;
		
		private void Update() {
			if(monsterController != null) {
				/*
				if(Input.GetMouseButtonDown(0)) {

					// Get the mouse position in world space
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;

					Debug.Log("Raycast result = " + Physics.Raycast(ray, out hit, Mathf.Infinity, terreinLayerMask));

					if(Physics.Raycast(ray, out hit, Mathf.Infinity, terreinLayerMask)) {
						//hit.transform.position;
						Debug.Log("raycast move to! YES");
						monsterController.MoveTo(hit.point, hit.transform.gameObject);

					}
				}*/
			}

			if(MonsterCard != null && MonsterCard.CanAttack(null)) {

			}
		}


		internal void Attack(MonsterPlatformController defender, Action callback) {
			//TODO - there are too many magic number in this code, we need to change these to event or some sort of inside logic to know how long the attack last ec...

			if(!monsterController.IsRanged() && !defender.monsterController.IsRanged()) {
				monsterController.MoveTo(defender.monsterPositionOnPlate.position, defender.monsterPositionOnPlate.gameObject, () => {


					monsterController.PerformAttack(defender.monsterPositionOnPlate);
					defender.monsterController.PerformAttack(monsterPositionOnPlate);

					//se i mostri non muoiono riportarli alla base
					StartCoroutine(RunActionAfterSeconds(1.2f, () => {
						monsterController.ResetState();
						defender.monsterController.ResetState();
						monsterController.MoveToBase();
					}));
				});
			}

			if(monsterController.IsRanged() ^ defender.monsterController.IsRanged()) {
				RangedMonsterController ranged = (monsterController.IsRanged() ? monsterController : defender.monsterController) as RangedMonsterController;
				MeleeMonsterController melee = (monsterController.IsRanged() ? defender.monsterController : monsterController) as MeleeMonsterController;

				ranged.MoveTo(melee.gameObject.transform.position, melee.gameObject, () => {
					//ranged monster moved in range of enemy
					//cast spell and make the other move
					ranged.PerformAttack(melee.gameObject.transform);
					melee.MoveTo(ranged.gameObject.transform.position, ranged.gameObject, () => {
						melee.PerformAttack(ranged.gameObject.transform);

						StartCoroutine(RunActionAfterSeconds(1.2f, () => {
							monsterController.ResetState();
							defender.monsterController.ResetState();
							monsterController.MoveToBase();
						}));
					});
				});
			}

			if(monsterController.IsRanged() && defender.monsterController.IsRanged()) {
				monsterController.MoveTo(defender.transform.position, defender.gameObject, () => {

				});

			}
			//TODO - other case

		}

		public void UpdateMonsterLocation() => monsterController.UpdateAgentLocation();

		IEnumerator RunActionAfterSeconds(float time, Action action) {
			yield return new WaitForSeconds(time);
			action?.Invoke();
		}

		
	}
}