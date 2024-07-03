using System;
using UnityEngine;

namespace _Project.Scripts.DataClasses.WorldData {
    [CreateAssetMenu(fileName = "PlanetData", menuName = "Data/PlanetData")]
    [Serializable]
    public class PlanetData : ScriptableObject {
        public float gravity = 9.8f;
        public Vector3 centre = Vector3.zero;
        public LayerMask groundLayer;
        public float gravityRadius;
    }
}