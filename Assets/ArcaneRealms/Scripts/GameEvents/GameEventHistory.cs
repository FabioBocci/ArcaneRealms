using System.Collections.Generic;

namespace ArcaneRealms.Scripts.GameEvents
{
    public class GameEventHistory
    {
        private readonly List<GameEvent> eventsList = new();

        private int index = 0;

        public void AddEvent(GameEvent gameEvent)
        {
            eventsList.Add(gameEvent);
        }

        public void SetNextEvent(GameEvent gameEvent)
        {
            eventsList.Insert(index, gameEvent);
        }

        public void RemoveNextEvent()
        {
            eventsList.RemoveAt(index);
        }

        public void TriggerEvent()
        {
            if (eventsList.Count > index)
            {
                eventsList[index].Do();
                index++;
            }
        }

        public void UndoEvent()
        {
            if (index > 0)
            {
                index--;
                eventsList[index].Undo();
            }
        }

        public void Reset()
        {
            for (int i = index - 1; i > 0; i--)
            {
                UndoEvent();
            }
        }

    }
}