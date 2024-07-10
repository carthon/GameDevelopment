using System;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Data/GameConfiguration")]
    [Serializable]
    public class GameConfiguration : ScriptableObject {
        [Range(1, 200)]
        public int renderDistance;
    }
}