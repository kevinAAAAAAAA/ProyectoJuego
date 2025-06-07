using UnityEngine;
using System.Collections.Generic; 

using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;
using Assets._Project.Scripts.DSL;
using Assets._Project.Scripts.ScriptableObjects.Definitions;

public class LevelController : MonoBehaviour
{
    [Header("SO Variables - Leer")]
    public LevelDataVariable activeLevelDataSO; 

    [Header("Prefabs a Instanciar")]
    public GameObject playerPrefab_ref; 
    public GameObject goalPrefab;
    public GameObject wallPrefab;

    private PlayerController _currentPlayerInstance;
    private GoalController _currentGoalInstance;
    private List<GameObject> _instantiatedWalls = new List<GameObject>();


    public void BuildLevel()
    {
        if (activeLevelDataSO == null || activeLevelDataSO.Value == null)
        {
            Debug.LogError("LevelController: ActiveLevelDataSO no asignado o su valor es nulo.");
            return;
        }

        ClearPreviousLevelElements(); 

        LevelData currentLevel = activeLevelDataSO.Value;

        if (goalPrefab != null)
        {
            _currentGoalInstance = Instantiate(goalPrefab, currentLevel.goalPosition, Quaternion.identity).GetComponent<GoalController>();
        }


        if (wallPrefab != null && currentLevel.wallPositions != null)
        {
            foreach (Vector3 wallPos in currentLevel.wallPositions)
            {
                GameObject newWall = Instantiate(wallPrefab, wallPos, Quaternion.identity);
                _instantiatedWalls.Add(newWall);
            }
        }

        Debug.Log($"Level '{currentLevel.name}' construido por LevelController.");
    }

    private void ClearPreviousLevelElements()
    {
        if (_currentGoalInstance != null) Destroy(_currentGoalInstance.gameObject);
        foreach (GameObject wall in _instantiatedWalls) Destroy(wall);
        _instantiatedWalls.Clear();
    }
}