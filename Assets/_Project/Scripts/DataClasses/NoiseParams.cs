using System;
using _Project.Scripts.Constants;

namespace _Project.Scripts.DataClasses {
    [Serializable]
    public struct NoiseParams {
        public string noiseName;
        public DensityEnum noiseType;
        public int numLayers;
        public float lacunarity, persistence, noiseScale;
        public override string ToString() {
            return $"[noiseName:{noiseName}, noiseType:{noiseType}, numLayers:{numLayers}, lacunarity:{lacunarity}, persistence:{persistence}, noiseScale:{noiseScale}]";
        }
    }
}