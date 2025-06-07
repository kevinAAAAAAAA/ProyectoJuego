using UnityEngine;
using UnityEngine.Events;
public interface IStringGameEventListener : IGameEventListener
{
    void OnEventRaised(string value);
}

[CreateAssetMenu(menuName = "MiJuegoProgramacion/Game Events/String Game Event", fileName = "NewStringGameEvent")]
public class StringGameEvent : GameEvent<IStringGameEventListener>
{
    public void Raise(string value)
    {
        base.Raise(listener => listener.OnEventRaised(value));
    }
}