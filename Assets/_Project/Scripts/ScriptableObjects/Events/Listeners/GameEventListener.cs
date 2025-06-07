// GameEventListener.cs
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour, IVoidGameEventListener
{
    public VoidGameEvent gameEvent;
    public UnityEvent onEventRaisedResponse = new UnityEvent();
    private bool _isRegistered = false;

    private void OnEnable()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        TryUnregister();
    }

    public void InitializeAndRegister(VoidGameEvent eventToListenTo)
    {
        if (gameEvent != null && gameEvent != eventToListenTo)
        {
            TryUnregister();
        }
        gameEvent = eventToListenTo;
        TryRegister();
    }

    private void TryRegister()
    {
        if (gameEvent == null) return;
        if (!_isRegistered)
        {
            gameEvent.RegisterListener(this);
            _isRegistered = true;
            Debug.Log($"====== GameEventListener ({gameObject.name}): Registrado al evento '{(gameEvent != null ? gameEvent.name : "NULL")}'");
        }
    }

    private void TryUnregister()
    {
        if (gameEvent == null || !_isRegistered) return;
        gameEvent.UnregisterListener(this);
        _isRegistered = false;
        Debug.Log($"====== GameEventListener ({gameObject.name}): Des-registrado del evento '{(gameEvent != null ? gameEvent.name : "NULL")}'");
    }

    public void OnEventRaised()
    {
        Debug.Log($"====== GameEventListener ({gameObject.name}): OnEventRaised() RECIBIDO - Evento: {(gameEvent != null ? gameEvent.name : "NULL")}");
        onEventRaisedResponse.Invoke();
        Debug.Log($"====== GameEventListener ({gameObject.name}): onEventRaisedResponse.Invoke() LLAMADO.");
    }
}