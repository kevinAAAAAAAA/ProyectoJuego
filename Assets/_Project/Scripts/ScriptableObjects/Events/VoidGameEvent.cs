using UnityEngine;
public interface IVoidGameEventListener : IGameEventListener 
{
}


[CreateAssetMenu(menuName = "MiJuegoProgramacion/Game Events/Void Game Event", fileName = "NewVoidGameEvent")]
public class VoidGameEvent : GameEvent<IVoidGameEventListener> 
{
    public void Raise()
    {
        base.Raise(listener => listener.OnEventRaised());
    }
}