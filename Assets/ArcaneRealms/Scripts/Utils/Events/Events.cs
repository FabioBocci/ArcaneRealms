using System;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils.Events
{
    [Serializable]
    public class CancellableEventData
    {
        [field: SerializeField] public bool IsCancelled { set; get; } = false;
    }

    public class EntityEventData<T> : CancellableEventData
    {
        public T Entity { set; get; }

        public EntityEventData(T entity)
        {
            Entity = entity;
        }

        private int usedCount = 0;
        private Action OnCompleted;
        public void RegisterWaiter() => usedCount++;

        public void UnregisterWaiter()
        {
            usedCount--;
            if (usedCount <= 0)
            {
                OnCompleted?.Invoke();
            }
        }

        public void OnComplete(Action callback)
        {
            if (usedCount <= 0)
            {
                callback.Invoke();
            }
            else
            {
                OnCompleted += callback;
            }
        }
        
    }

    public delegate void CancellableEvent(ref CancellableEventData cancellableEventData);

    public delegate void CancellableEvent<in T>(T cancellableEventData) where T : CancellableEventData;
    
    public delegate void EntityEvent<T>(ref EntityEventData<T> entityEventData);

    public delegate void EntityEvent<in T, TD>(T entityEventData) where T : EntityEventData<TD>;
}