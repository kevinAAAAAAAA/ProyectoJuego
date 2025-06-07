using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("SO Event Channels - Disparar")]
    public VoidGameEvent doorOpenedEvent;

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;

    private Collider _collider;
    private MeshRenderer _renderer;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>(); 
    }

    public void Open()
    {
        if (_isOpen) return;

        _isOpen = true;
        if (doorOpenedEvent != null)
        {
            doorOpenedEvent.Raise();
        }
        else
        {
            Debug.LogError("DoorOpenedEvent no asignado en DoorController!", this.gameObject);
        }

        if (_collider != null) _collider.enabled = false; 
        if (_renderer != null) _renderer.enabled = false; 

        Debug.Log($"Door '{gameObject.name}' opened.");
    }

    public void ResetDoor() 
    {
        _isOpen = false;
        if (_collider != null) _collider.enabled = true;
        if (_renderer != null) _renderer.enabled = true;
        Debug.Log($"Door '{gameObject.name}' reset to closed state.");
    }
}