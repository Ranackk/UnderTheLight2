using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JSDK.Misc;

namespace JSDK.Events
{
    public class GameEvent {}
    
    public class EventManager : Singleton<EventManager>
    {
        // A delegate for a function that takes in a GameEvent T
        public delegate void EventDelegate<T> (T e) where T : GameEvent;

        ////////////////////////////////////////////////////////////////

        readonly Dictionary<Type, Delegate> m_Delegates = new Dictionary<Type, Delegate>();

        ////////////////////////////////////////////////////////////////

        public void AddListener<T>(EventDelegate<T> listener) where T : GameEvent
        {
            Delegate foundDelegate;
            if (m_Delegates.TryGetValue(typeof(T), out foundDelegate))
            {
                // Entry already present? Add to this entry.
                m_Delegates[typeof(T)] = Delegate.Combine(foundDelegate, listener);
            }
            else
            {
                // Entry not present yet? Add it!
                m_Delegates[typeof(T)] = listener;
            }
        }

        ////////////////////////////////////////////////////////////////

        public void RemoveListener<T>(EventDelegate<T> listener) where T : GameEvent
        {
            Delegate foundEntry;
            if (m_Delegates.TryGetValue(typeof(T), out foundEntry))
            {
                // Is listener registered?
                Delegate resultDelegate = Delegate.Remove(foundEntry, listener);
                
                if (resultDelegate == null)
                {
                    // No entry for this event left. Clear it!
                    m_Delegates.Remove(typeof(T));
                }
                else
                {
                    // Update entry for this event
                    m_Delegates[typeof(T)] = resultDelegate;
                }
            }
            else
            {
                Debug.LogWarning("EventManager: A listener tries to unregister, even though he is not registered! " + listener.ToString());
            }
        }

        ////////////////////////////////////////////////////////////////

        public void FireEvent<T>(T e) where T : GameEvent
        {
            if (e == null)
            {
                Debug.LogError("EventManager: An event is null!");
                return;
            }

            Delegate foundEntry;
            if (m_Delegates.TryGetValue(typeof(T), out foundEntry))
            {
                EventDelegate<T> eventCallback = foundEntry as EventDelegate<T>;

                if (eventCallback != null)
                {
                    eventCallback(e);
                }
            }
        }
    }
}

