using UnityEngine;
using UnityEngine.UI;
using TMPro;



namespace Assets._Project.Scripts.UI
{
    public class UIManager : MonoBehaviour 
    {
        [Header("UI Elements")]
        public TMP_InputField codeInputField;
        public Button runButton;
        public Button resetButton;
        public TMP_Text objectiveText;
        public TMP_Text feedbackText;
        public TMP_Text levelNameText;

        [Header("SO Event Channels - Disparar")]
        public StringGameEvent executeDSLEvent; 
        public VoidGameEvent restartLevelEvent; 

        [Header("SO Event Channels - Escuchar")]
        public StringGameEvent dslErrorEvent;    

        [Header("SO Variables - Escribir (Opcional para este script)")]
        public StringVariable playerCodeInputSO; 

        [Header("SO Event Channels - Disparar (HUD)")]
        public VoidGameEvent pauseGameEventFromHUD; 


        public void Initialize() 
        {
            runButton.onClick.AddListener(OnRunClicked);
            resetButton.onClick.AddListener(OnResetClicked);
            ClearFeedback();

        }

        public void OnRunClicked()
        {
            Debug.Log("====== UIManager: OnRunClicked() - MÉTODO LLAMADO ======");
            string code = codeInputField.text;

            if (playerCodeInputSO != null)
            {
                playerCodeInputSO.Value = code;
                Debug.Log($"[UIManager] PlayerCodeInputSO actualizado a: {playerCodeInputSO.Value}");
            }

            if (executeDSLEvent != null)
            {
                Debug.Log($"[UIManager] INTENTANDO DISPARAR ExecuteDSLEvent con código: '{code}'");
                executeDSLEvent.Raise(code); 
                Debug.Log("[UIManager] ExecuteDSLEvent DISPARADO (o al menos se intentó)."); 
            }
            else
            {
                Debug.LogError("[UIManager] ERROR CRÍTICO: executeDSLEvent (StringGameEvent) NO está asignado en el Inspector de UIManager!");
            }

            runButton.interactable = false;
            resetButton.interactable = false;
        }

        public void OnResetClicked()
        {
            if (restartLevelEvent != null)
            {
                restartLevelEvent.Raise(); 
            }
            else
            {
                Debug.LogError("RestartLevelEvent no asignado en UIManager!");
            }
            ClearFeedback();
        }

        public void OnDSLErrorReceived(string errorMessage)
        {
            ShowFeedback(errorMessage, true);
            EnableButtons();
        }

        public void OnDSLExecutionSuccess()
        {
            EnableButtons();
        }

        public void OnPlayerActionsCompleted()
        {
            EnableButtons();
        }


        public void SetObjective(string description)
        {
            objectiveText.text = "Objetivo: " + description;
        }
        public void SetLevelName(string name)
        {
            levelNameText.text = name;
        }

        public void ShowFeedback(string message, bool isError)
        {
            feedbackText.text = message;
            feedbackText.color = isError ? Color.red : Color.green;
        }

        public void ClearFeedback()
        {
            feedbackText.text = "";
        }

        public void SetCode(string code) 
        {
            codeInputField.text = code;
        }
        public void EnableButtons()
        {
            runButton.interactable = true;
            resetButton.interactable = true;
        }
        public void OnPauseButtonClicked()
        {
            if (pauseGameEventFromHUD != null)
            {
                pauseGameEventFromHUD.Raise();
            }
        }
    }
}