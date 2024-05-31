using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGame
{
    public class EventsManager : BaseManager
    {
        private Dictionary<EventsType, EventListenersData> eventListenersData = new();

        public EventsManager(Action<BaseManager> onComplete) : base(onComplete)
        {
            OnInitComplete();
        }

        public void AddListener(EventsType eventsType, Action<object> methodToInvoke)
        {
            if (eventListenersData.TryGetValue(eventsType, out var value))
            {
                value.ActionsOnInvoke.Add(methodToInvoke);
            }
            else
            {
                eventListenersData[eventsType] = new EventListenersData(methodToInvoke);
            }
        }

        // Removes listener for the specified event type
        public void RemoveListener(EventsType eventsType, Action<object> methodToInvoke)
        {
            if (!eventListenersData.TryGetValue(eventsType, out var value))
            {
                return;
            }

            value.ActionsOnInvoke.Remove(methodToInvoke);

            if (!value.ActionsOnInvoke.Any())
            {
                eventListenersData.Remove(eventsType);
            }
        }

        // Invokes all listeners registered for the specified event type, and the data to pass to the listeners when invoking the event
        public void InvokeEvent(EventsType eventsType, object dataToInvoke)
        {
            if (!eventListenersData.TryGetValue(eventsType, out var value))
            {
                return;
            }

            foreach (var method in value.ActionsOnInvoke)
            {
                method.Invoke(dataToInvoke);
            }
        }
    }

    // Represents the data of listeners for a specific event type.
    public class EventListenersData
    {
        public List<Action<object>> ActionsOnInvoke;

        public EventListenersData(Action<object> additionalData)
        {
            ActionsOnInvoke = new List<Action<object>>
            {
                additionalData
            };
        }
    }

// Enum representing different event types.
    public enum EventsType
    {
        GenerateGrid,
        SpawnBlocks,
        MoveLeft,
        MoveRight,
        MoveDown,
        MoveUp
    }
}