using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StringUnityEvent : UnityEvent<string> { }

public class StringGameEventListener : MonoBehaviour, IStringGameEventListener
{
    [Tooltip("El evento String al que este listener se suscribirá.")]
    public StringGameEvent gameEvent; 

    [Tooltip("Respuesta a invocar cuando el evento es disparado, pasando el string.")]
    public StringUnityEvent onEventRaisedResponse = new StringUnityEvent();

    private bool _isRegistered = false; 

    private void OnEnable()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        TryUnregister();
    }

    public void InitializeAndRegister(StringGameEvent eventToListenTo)
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
        if (gameEvent == null)
        {
            return;
        }

        if (!_isRegistered)
        {
            gameEvent.RegisterListener(this);
            _isRegistered = true;
            Debug.Log($"====== StringGameEventListener ({gameObject.name}): Registrado al evento '{(gameEvent != null ? gameEvent.name : "NULL")}'");
        }
    }

    private void TryUnregister()
    {
        if (gameEvent == null || !_isRegistered)
        {
            return;
        }
        gameEvent.UnregisterListener(this);
        _isRegistered = false;
        Debug.Log($"====== StringGameEventListener ({gameObject.name}): Des-registrado del evento '{(gameEvent != null ? gameEvent.name : "NULL")}'");
    }


    public void OnEventRaised(string value)
    {
        Debug.Log($"====== StringGameEventListener ({gameObject.name}): OnEventRaised(string) RECIBIDO con valor: '{value}' - Evento: {(gameEvent != null ? gameEvent.name : "NULL")}");
        onEventRaisedResponse.Invoke(value);
        Debug.Log($"====== StringGameEventListener ({gameObject.name}): onEventRaisedResponse.Invoke(value) LLAMADO.");
    }

    void IGameEventListener.OnEventRaised()
    {
        Debug.LogWarning($"StringGameEventListener en {gameObject.name} recibió OnEventRaised() sin payload. La respuesta con string no será invocada por esta vía.");
    }
}