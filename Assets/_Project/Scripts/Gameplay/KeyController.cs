using UnityEngine;

public class KeyController : MonoBehaviour
{
    [Header("SO Event Channels - Disparar")]
    public VoidGameEvent keyCollectedEvent;

    private bool _isAvailable = true;

    public bool IsAvailable() => _isAvailable;

    public void Collect()
    {
        if (!_isAvailable) return;

        _isAvailable = false;
        if (keyCollectedEvent != null)
        {
            keyCollectedEvent.Raise();
        }
        else
        {
            Debug.LogError("KeyCollectedEvent no asignado en KeyController!", this.gameObject);
        }
        gameObject.SetActive(false);
        Debug.Log($"Key '{gameObject.name}' collected and deactivated.");
    }

}