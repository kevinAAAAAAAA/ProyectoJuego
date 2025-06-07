using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.DSL;

using UnityEngine;
using Assets._Project.Scripts.Core;

public class DSLExecutionManager : MonoBehaviour
{
    [Header("SO Event Channels - Escuchar")]
    public StringGameEvent executeDSLEventSO; 

    [Header("SO Event Channels - Disparar (desde Intérprete)")]
    public StringGameEvent dslErrorEventSO;  

    [Header("Dependencias")]
    public PlayerController playerController;
    public GameManager gameManager;   

    private DslInterpreter _interpreter;
    private StringGameEventListener _executeDslListenerComponent;


    void Awake()
    {
        if (playerController == null || gameManager == null || dslErrorEventSO == null)
        {
            Debug.LogError("DSLExecutionManager: Faltan dependencias o dslErrorEventSO!");
            enabled = false;
            return;
        }
        _interpreter = new DslInterpreter(playerController, gameManager, dslErrorEventSO);
    }

    void Start()
    {
        Debug.Log("[DSLExecutionManager] Start() llamado.");
        if (executeDSLEventSO != null)
        {
            _executeDslListenerComponent = gameObject.AddComponent<StringGameEventListener>();
            if (_executeDslListenerComponent == null) {
                Debug.LogError("[DSLExecutionManager] Falló AddComponent<StringGameEventListener>!");
                return;
            }

            _executeDslListenerComponent.InitializeAndRegister(executeDSLEventSO);
            Debug.Log($"[DSLExecutionManager] Llamado InitializeAndRegister en listener con evento: {executeDSLEventSO.name}");


            if (_interpreter != null)
            {
                _executeDslListenerComponent.onEventRaisedResponse.AddListener(_interpreter.ProcessDSLExecution);
                Debug.Log("[DSLExecutionManager] Listener de respuesta conectado a _interpreter.ProcessDSLExecution.");
            }
            else
            {
                Debug.LogError("[DSLExecutionManager] _interpreter es NULL al intentar conectar el listener de respuesta!");
            }
        }
        else
        {
            Debug.LogError("[DSLExecutionManager] ERROR CRÍTICO: ExecuteDSLEventSO NO está asignado en el Inspector de DSLExecutionManager!");
        }
    }

    void OnDestroy()
    {
        if (_executeDslListenerComponent != null && _executeDslListenerComponent.onEventRaisedResponse != null)
        {
            _executeDslListenerComponent.onEventRaisedResponse.RemoveListener(_interpreter.ProcessDSLExecution);
        }
    }
}