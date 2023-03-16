using System;
using UnityEngine;
using UnityEngine.Events;

namespace ArcaneRealms.Scripts.Utils {

	[Serializable]
	public class CustomGameEvent : UnityEvent<Component, object> { };

	public class GameEventListener : MonoBehaviour {
		public GameEventSO Event;
		public CustomGameEvent Response;

		private void OnEnable() {
			Event?.RegisterListener(this);
		}

		private void OnDisable() {
			Event?.UnregisterListener(this);
		}

		public void OnEventRaised(Component sender, object data) {
			Response?.Invoke(sender, data);
		}
	}
}