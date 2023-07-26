using System.Collections.Generic;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils {
	[CreateAssetMenu(fileName = "New Event", menuName = "Event/New Game Event", order = 0)]
	public class GameEventSO : ScriptableObject {
		private readonly List<GameEventListener> _EventListeners = new();


		public virtual void Raise(Component component, object data) {
			for(int i = _EventListeners.Count - 1; i >= 0; i--) {
				_EventListeners[i].OnEventRaised(component, data);
			}
		}

		[ContextMenu("Raise")]
		public virtual void Raise() {
			for(int i = _EventListeners.Count - 1; i >= 0; i--) {
				_EventListeners[i].OnEventRaised(null, null);
			}
		}

		public void RegisterListener(GameEventListener listener) {
			_EventListeners.Add(listener);
		}

		public void UnregisterListener(GameEventListener listener) {
			_EventListeners.Remove(listener);
		}
	}
}