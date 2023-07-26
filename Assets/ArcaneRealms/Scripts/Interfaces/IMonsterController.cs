using System;
using UnityEngine;

namespace ArcaneRealms.Scripts.Interfaces {

	public interface IMonsterController {
		public void MoveTo(Vector3 targetPosition, GameObject target, Action endAction = null);


		public void PerformAttack(Transform target);


		public void ResetState();

		public void MoveToBase();

		public bool IsMoving();

		public bool IsRanged();

		void UpdateAgentLocation();
	}
}