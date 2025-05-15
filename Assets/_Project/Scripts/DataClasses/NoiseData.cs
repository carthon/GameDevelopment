using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [CreateAssetMenu(menuName = "Data/NoiseData", fileName = "Noise Data")]
    public class NoiseData : ScriptableObject {
        public List<NoiseParams> noiseParams;
    }
}