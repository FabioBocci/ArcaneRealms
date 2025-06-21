using System;
using System.Threading.Tasks;
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

    public delegate Task CancellableEvent(ref CancellableEventData cancellableEventData);

    public delegate Task CancellableEvent<in T>(T cancellableEventData) where T : CancellableEventData;
    
    public delegate Task EntityEvent<T>(ref EntityEventData<T> entityEventData);

    public delegate Task EntityEvent<in T, TD>(T entityEventData) where T : EntityEventData<TD>;
}