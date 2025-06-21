#region

using System;
using System.Collections;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Utils;
using UnityEngine;

#endregion

namespace ArcaneRealms.Scripts.GameActions
{
    public enum ReactionTiming
    {
        PRE,
        POST
    }


    public class ActionSystem : Singleton<ActionSystem>
    {
        private List<GameAction> reactions;

        public bool IsPerforming { get; private set; }

        private static readonly Dictionary<Type, List<Action<GameAction>>> preSubscriptions = new();
        private static readonly Dictionary<Type, List<Action<GameAction>>> postSubscriptions = new();

        private static readonly Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

        public void Perform(GameAction action, Action onPerformFinished = null)
        {
            if (IsPerforming)
            {
                Debug.LogWarning(
                    "ActionSystem is already performing an action. Cannot perform another action until the current one is finished.");
                return;
            }

            IsPerforming = true;

            StartCoroutine(Flow(action, () =>
            {
                IsPerforming = false;
                onPerformFinished?.Invoke();
            }));
        }

        public void AddReaction(GameAction action)
        {
            reactions?.Add(action);
        }

        private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
        {
            reactions = action.preReactions;
            PerformSubscribers(action, preSubscriptions);
            yield return PerformReactions();

            reactions = action.performReactions;
            yield return PerformPerformer(action);
            yield return PerformReactions();

            reactions = action.postReactions;
            PerformSubscribers(action, postSubscriptions);
            yield return PerformReactions();

            OnFlowFinished?.Invoke();
        }

        private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subscriptions)
        {
            if (!subscriptions.ContainsKey(action.GetType())) return;
            foreach (var subscription in subscriptions[action.GetType()]) subscription.Invoke(action);
        }

        private IEnumerator PerformReactions()
        {
            if (reactions == null) yield break;
            foreach (var reaction in reactions) yield return Flow(reaction);
        }

        private IEnumerator PerformPerformer(GameAction action)
        {
            if (performers.ContainsKey(action.GetType()))
                yield return performers[action.GetType()](action);
            else
                Debug.LogWarning($"No performer found for action type {action.GetType()}");
        }

        public static void AttachPerformer<T>(Func<GameAction, IEnumerator> performer) where T : GameAction
        {
            var type = typeof(T);

            IEnumerator WrappedPerformer(GameAction action)
            {
                return performer((T)action);
            }

            if (performers.ContainsKey(type))
                performers[type] = WrappedPerformer;
            else
                performers.Add(type, WrappedPerformer);
        }

        public static void DetachPerformer<T>() where T : GameAction
        {
            performers.Remove(typeof(T));
        }

        public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
        {
            if (timing == ReactionTiming.PRE)
            {
                if (!preSubscriptions.ContainsKey(typeof(T)))
                    preSubscriptions[typeof(T)] = new List<Action<GameAction>>();
                preSubscriptions[typeof(T)].Add(action => reaction((T)action));
            }
            else
            {
                if (!postSubscriptions.ContainsKey(typeof(T)))
                    postSubscriptions[typeof(T)] = new List<Action<GameAction>>();
                postSubscriptions[typeof(T)].Add(action => reaction((T)action));
            }
        }

        public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
        {
            if (timing == ReactionTiming.PRE)
            {
                if (preSubscriptions.ContainsKey(typeof(T)))
                    preSubscriptions[typeof(T)].Remove(action => reaction((T)action));
            }
            else
            {
                if (postSubscriptions.ContainsKey(typeof(T)))
                    postSubscriptions[typeof(T)].Remove(action => reaction((T)action));
            }
        }
    }
}