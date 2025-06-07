using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel; 

    [Header("SO Variables - Leer")]
    public BoolVariable isGamePausedSO; 

    [Header("SO Event Channels - Disparar")]
    public VoidGameEvent resumeGameEvent; 
    public VoidGameEvent restartLevelEvent; 


    void Update()
    {
        if (pauseMenuPanel != null && isGamePausedSO != null)
        {
            if (pauseMenuPanel.activeSelf != isGamePausedSO.Value)
            {
                pauseMenuPanel.SetActive(isGamePausedSO.Value);
            }
        }
    }

    public void OnResumeButtonPressed()
    {
        if (resumeGameEvent != null)
        {
            resumeGameEvent.Raise();
        }
    }

    public void OnRestartButtonPressed()
    {
        if (isGamePausedSO.Value && resumeGameEvent != null)
        {
            resumeGameEvent.Raise(); 
        }
        if (restartLevelEvent != null)
        {
            restartLevelEvent.Raise();
        }
    }

}