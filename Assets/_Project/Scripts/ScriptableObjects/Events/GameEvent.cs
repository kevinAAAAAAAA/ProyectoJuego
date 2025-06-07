using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent<T_Listener> : ScriptableObject where T_Listener : class 
{
    private readonly List<T_Listener> _listeners = new List<T_Listener>();

    public void Raise(System.Action<T_Listener> action) 
    {
     
        for (int i = _listeners.Count - 1; i >= 0; i--)
        {
            if (_listeners[i] != null) 
            {
                action.Invoke(_listeners[i]);
            }
            else
            {
            }
        }
    }

    public void RegisterListener(T_Listener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    public void UnregisterListener(T_Listener listener)
    {
        if (_listeners.Contains(listener))
        {
            _listeners.Remove(listener);
        }
    }
}

public interface IGameEventListener
{
    void OnEventRaised(); 
}