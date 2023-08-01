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
    }

    public delegate void SimpleEmptyEvent();

    public delegate void CancellableEvent(CancellableEventData cancellableEventData);

    public delegate void CancellableEvent<in T>(T cancellableEventData) where T : CancellableEventData;
    
    public delegate void EntityEvent<T>(EntityEventData<T> entityEventData);

    public delegate void EntityEvent<in T, TD>(T entityEventData) where T : EntityEventData<TD>;
}