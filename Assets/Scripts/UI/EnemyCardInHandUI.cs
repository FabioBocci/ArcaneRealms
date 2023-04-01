using ArcaneRealms.Scripts.Managers;
using UnityEngine;

namespace ArcaneRealms.Scripts.UI {
	public class EnemyCardInHandUI : MonoBehaviour {

		[SerializeField] private UIOutline outline;

		private void Update() {
			if(outline.enabled && HandUIManager.Instance.enemyCardInHandHighlight != this) {
				outline.enabled = false;
			}
		}

		public void ActiveOutline() {
			outline.enabled = true;
		}


	}
}