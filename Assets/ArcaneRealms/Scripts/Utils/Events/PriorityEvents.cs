using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace ArcaneRealms.Scripts.Utils.Events
{
    [Serializable]
    public class PriorityEvent<T>
    {
        public List<PriorityCallback<T>> list = new();

        public virtual void Add(int priority, UnityAction<T> action)
        {
            list.Add(new PriorityCallback<T>()
            {
                priority = priority,
                callBack = action
            });
            list.Sort();
        }

        public virtual void Remove(UnityAction<T> action)
        {
            list.RemoveAll(pc => pc.callBack.Equals(action));
        }

        public virtual void Trigger(ref T param)
        {
            foreach (var priorityCallback in list)
            {
                priorityCallback.callBack?.Invoke(param);
            }
            
        }
        
        public static PriorityEvent<T> operator +(PriorityEvent<T> prioEvent, ( int prio, UnityAction<T> callback) prioCallback)
        {
            prioEvent.Add(prioCallback.prio, prioCallback.callback);
            return prioEvent;
        }

        public static PriorityEvent<T> operator -(PriorityEvent<T> prioEvent,  UnityAction<T> callback)
        {
            prioEvent.Remove(callback);
            return prioEvent;
        }
        
    }

    public class PriorityCancellableEvent<T> : PriorityEvent<T> where T : CancellableEventData
    {
        public void Trigger(ref T param, bool stopIfCancelled)
        {
            foreach (var priorityCallback in list)
            {
                priorityCallback.callBack?.Invoke(param);
                if (stopIfCancelled && param.IsCancelled)
                {
                    return;
                }
            }

        }
    }

    [Serializable]
    public class PriorityCallback<T> : IComparable<PriorityCallback<T>>
    {
        public int priority;

        public UnityAction<T> callBack;

        public int CompareTo(PriorityCallback<T> other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return priority.CompareTo(other.priority);
        }
        
    }
}