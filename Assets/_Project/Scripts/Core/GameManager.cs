
using UnityEngine;
using System.Collections.Generic; 
using System.Text; 

using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.DSL;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.ScriptableObjects.Definitions;


namespace Assets._Project.Scripts.Core
{

    using UnityEngine;
    using System.Collections.Generic;
    using System.Text;

    public class GameManager : MonoBehaviour
    {
        [Header("Dependencias de Escena")]
        public PlayerController playerController;
        public UIManager uiManager; 
        public GameObject goalPrefab;
        [Header("Datos de Nivel (Assets)")]
        public List<LevelData> levels; 

        [Header("SO Variables - Leer/Escribir")]
        public IntVariable currentLevelIndexSO;   
        public LevelDataVariable activeLevelDataSO; 
        public BoolVariable isGamePausedSO;     

        [Header("SO Event Channels - Escuchar")]
        public VoidGameEvent levelCompletedEvent; 
        public VoidGameEvent restartLevelEvent;   
        public VoidGameEvent pauseGameEvent;    
        public VoidGameEvent resumeGameEvent;    

        private GameEventListener _levelCompletedListener;
        private GameEventListener _restartLevelListener;
        private GameEventListener _pauseGameListener;
        private GameEventListener _resumeGameListener;

        private GoalController _currentGoalInstance;
        private List<GameObject> _instantiatedWalls = new List<GameObject>(); 
        public GameObject wallPrefab;
        private bool _levelClearConditionMet = false; 
        
        public GameObject keyPrefab; 

        private List<GameObject> _instantiatedItems = new List<GameObject>();
        private List<DoorController> _instantiatedDoors = new List<DoorController>();

        void Awake() 
        {
            if (playerController == null || uiManager == null || levels == null || levels.Count == 0 ||
                currentLevelIndexSO == null || activeLevelDataSO == null || isGamePausedSO == null ||
                levelCompletedEvent == null || restartLevelEvent == null || pauseGameEvent == null || resumeGameEvent == null)
            {
                Debug.LogError("GameManager: Faltan referencias SO o dependencias. Configura en el Inspector.");
                enabled = false;
                return;
            }

            currentLevelIndexSO.ResetToInitialValue(); 
            isGamePausedSO.ResetToInitialValue();    

            SetupEventListeners();
        }

        void Start()
        {
            if (isGamePausedSO != null && !isGamePausedSO.Value) 
            {
                Time.timeScale = 1.0f; 
                Debug.Log("[GameManager] Game started, Time.timeScale set to 1.0f");
            }

            playerController.OnActionQueueCompleted += OnPlayerActionsCompletedCallback;
            LoadLevel(currentLevelIndexSO.Value);
        }

        void OnDestroy()
        {
            if (playerController != null)
                playerController.OnActionQueueCompleted -= OnPlayerActionsCompletedCallback;

            // Limpiar listeners programáticos
            if (_levelCompletedListener != null && levelCompletedEvent != null)
                _levelCompletedListener.onEventRaisedResponse.RemoveListener(OnLevelCompleted);
        }


        // GameManager.cs
        private void SetupEventListeners()
        {
            if (levelCompletedEvent != null)
            {
                _levelCompletedListener = gameObject.AddComponent<GameEventListener>();
                _levelCompletedListener.InitializeAndRegister(levelCompletedEvent); 
                _levelCompletedListener.onEventRaisedResponse.AddListener(OnLevelCompleted);
            }
            else Debug.LogError("GameManager: LevelCompletedEvent SO no asignado!");

            if (restartLevelEvent != null)
            {
                _restartLevelListener = gameObject.AddComponent<GameEventListener>();
                _restartLevelListener.InitializeAndRegister(restartLevelEvent);
                _restartLevelListener.onEventRaisedResponse.AddListener(OnRestartLevel);
            }
            else Debug.LogError("GameManager: RestartLevelEvent SO no asignado!");

            if (pauseGameEvent != null)
            {
                _pauseGameListener = gameObject.AddComponent<GameEventListener>();
                _pauseGameListener.InitializeAndRegister(pauseGameEvent); 
                _pauseGameListener.onEventRaisedResponse.AddListener(OnPauseGame);
            }
            else Debug.LogError("GameManager: PauseGameEvent SO no asignado!");

            if (resumeGameEvent != null)
            {
                _resumeGameListener = gameObject.AddComponent<GameEventListener>();
                _resumeGameListener.InitializeAndRegister(resumeGameEvent); 
                _resumeGameListener.onEventRaisedResponse.AddListener(OnResumeGame);
            }
            else Debug.LogError("GameManager: ResumeGameEvent SO no asignado!");
        }


        public void LoadLevel(int levelIndex)
        {


            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
                Debug.LogWarning($"Índice de nivel {levelIndex} fuera de rango. Mostrando mensaje de fin.");
                uiManager.ShowFeedback("¡Has completado todos los niveles o el nivel no existe!", false);
                return;
            }

            currentLevelIndexSO.Value = levelIndex;
            activeLevelDataSO.Value = levels[levelIndex]; 

            if (playerController != null) 
                playerController.ResetPlayer(activeLevelDataSO.Value.playerStartPosition, activeLevelDataSO.Value.playerStartRotation);
            else
                Debug.LogError("GameManager: PlayerController es null en LoadLevel!");


            if (_currentGoalInstance != null) Destroy(_currentGoalInstance.gameObject);
            foreach (GameObject wall in _instantiatedWalls) Destroy(wall);
            _instantiatedWalls.Clear();


            if (goalPrefab != null)
            {
                GameObject goalGO = Instantiate(goalPrefab, activeLevelDataSO.Value.goalPosition, Quaternion.identity);
                _currentGoalInstance = goalGO.GetComponent<GoalController>(); // Obtener componente
                if (_currentGoalInstance != null)
                {
                    _currentGoalInstance.levelCompletedEvent = this.levelCompletedEvent;
                    _currentGoalInstance.ResetGoal();
                }
                else Debug.LogError("El GoalPrefab no tiene GoalController o falló la instanciación.");
            }
            else Debug.LogError("GameManager: GoalPrefab no asignado!");


            if (wallPrefab != null && activeLevelDataSO.Value.wallPositions != null)
            {
                foreach (Vector3 wallPos in activeLevelDataSO.Value.wallPositions)
                {
                    GameObject newWall = Instantiate(wallPrefab, wallPos, Quaternion.identity);
                    _instantiatedWalls.Add(newWall);
                }
            }

            if (uiManager != null) 
            {
                uiManager.SetObjective(activeLevelDataSO.Value.objectiveDescription);
                uiManager.SetLevelName($"Nivel {levelIndex + 1}");
                uiManager.SetCode(activeLevelDataSO.Value.exampleSolutionCode);
                uiManager.ClearFeedback();
                uiManager.EnableButtons(); 
            }
            else
            {
                Debug.LogError("GameManager: UIManager es null en LoadLevel!");
            }
            




            foreach (GameObject item in _instantiatedItems) Destroy(item);
            _instantiatedItems.Clear();
            foreach (DoorController door in _instantiatedDoors)
            {
                if (door != null) Destroy(door.gameObject); 
            }
            _instantiatedDoors.Clear();


            if (wallPrefab != null && activeLevelDataSO.Value.wallPositions != null)
            {
                foreach (Vector3 wallPos in activeLevelDataSO.Value.wallPositions)
                {
                }
            }


            if (keyPrefab != null && activeLevelDataSO.Value.keyPositions != null) 
            {
                foreach (Vector3 keyPos in activeLevelDataSO.Value.keyPositions)
                {
                    GameObject newKey = Instantiate(keyPrefab, keyPos, Quaternion.identity);
                    _instantiatedItems.Add(newKey);
                }
            }


            DoorController[] doorsInScene = FindObjectsByType<DoorController>(FindObjectsSortMode.None);

            foreach (DoorController door in doorsInScene)
            {
                door.ResetDoor();
            }






















        }

        public void OnRestartLevel()
        {
            Debug.Log("GameManager: RestartLevelEvent recibido. Recargando nivel actual.");
            if (isGamePausedSO.Value && resumeGameEvent != null) 
            {
                resumeGameEvent.Raise();
            }
            LoadLevel(currentLevelIndexSO.Value);
        }

        public void OnLevelCompleted()
        {
            Debug.Log("GameManager: LevelCompletedEvent recibido!");
            if (uiManager != null) uiManager.ShowFeedback("¡Nivel Completado!", false);
            _levelClearConditionMet = true; 
            if (playerController != null && playerController.ActionQueueCount == 0 && !playerController.IsCurrentlyExecutingAction)
            {
                Debug.Log("GameManager: Meta alcanzada y cola de acciones ya vacía. Cargando siguiente nivel inmediatamente.");
                ProcessLoadNextLevelAfterCompletion();
            }
        }

        private void OnPlayerActionsCompletedCallback()
        {
            Debug.Log("GameManager: PlayerActionsCompletedCallback (OnActionQueueCompleted) recibido.");

            if (_levelClearConditionMet)
            {
                ProcessLoadNextLevelAfterCompletion();
            }
            else
            {
                if (uiManager != null)
                {
                    float distanceToGoal = float.MaxValue;
                    if (_currentGoalInstance != null && playerController != null)
                    {
                        distanceToGoal = Vector3.Distance(playerController.transform.position, _currentGoalInstance.transform.position);
                    }

                    if (distanceToGoal > 0.5f) 
                    {
                        uiManager.ShowFeedback("Acciones terminadas. ¿Alcanzaste la meta?", false);
                    }
                    uiManager.EnableButtons();
                }
            }
        }


        private void ProcessLoadNextLevelAfterCompletion()
        {
            if (_levelClearConditionMet)
            {
                Debug.Log("GameManager: Condiciones cumplidas (Meta alcanzada y cola de acciones vacía). Procediendo a cargar el siguiente nivel.");
                
                Invoke(nameof(LoadNextLevel), 0.1f); 
                _levelClearConditionMet = false; 
            }
        }


        public void LoadNextLevel()
        {
            LoadLevel(currentLevelIndexSO.Value + 1);
        }

        public void OnPauseGame()
        {
            if (isGamePausedSO.Value) return; 

            isGamePausedSO.Value = true;
            Time.timeScale = 0f; 
            Debug.Log("Game Paused");
        }

        public void OnResumeGame()
        {
            if (!isGamePausedSO.Value && Time.timeScale != 0f) return; 

            isGamePausedSO.Value = false;
            Time.timeScale = 1f; 
            Debug.Log("Game Resumed");
        }

        public void ShowMessageInUI(string message)
        {
            if (uiManager != null)
            {
                uiManager.ShowFeedback(message, false);
            }
            else
            {
                Debug.LogWarning("GameManager: UIManager es null, no se puede mostrar mensaje: " + message);
            }
        }



    }

}
 