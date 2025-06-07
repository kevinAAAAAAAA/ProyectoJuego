using UnityEngine;

using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;
namespace Assets._Project.Scripts.Gameplay
{




public class GoalController : MonoBehaviour
{
    [Header("SO Event Channels - Disparar")]
    public VoidGameEvent levelCompletedEvent;

    private bool _isTriggered = false; 

    void OnTriggerEnter(Collider other)
    {
        if (_isTriggered) return;

        if (other.CompareTag("Player"))
        {
            if (levelCompletedEvent != null)
            {
                Debug.Log("Player reached the GOAL! Raising LevelCompletedEvent.");
                levelCompletedEvent.Raise();
                _isTriggered = true; 
            }
            else
            {
                Debug.LogError("LevelCompletedEvent no asignado en GoalController!");
            }
        }
    }

    public void ResetGoal()
    {
        _isTriggered = false;
    }
}

}