using UnityEngine;
using System.Collections.Generic;

using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.DSL;



namespace Assets._Project.Scripts.ScriptableObjects.Definitions
{




    [CreateAssetMenu(fileName = "Level_", menuName = "MiJuegoProgramacion/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        [TextArea(3, 5)]
        public string objectiveDescription = "Descripción del objetivo del nivel.";

        public Vector3 playerStartPosition = Vector3.zero;
        public Vector3 goalPosition = new Vector3(0, 0, 5);
        public Quaternion playerStartRotation = Quaternion.identity;

        [TextArea(5, 10)]
        public string exampleSolutionCode = "VARIABLE pasos = 5\nMOVER_ADELANTE(pasos)";

        public List<Vector3> wallPositions;
        public List<Vector3> keyPositions;
        public List<Vector3> doorPositions;
    

    }


}