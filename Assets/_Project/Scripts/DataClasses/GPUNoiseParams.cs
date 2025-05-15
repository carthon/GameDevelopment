using _Project.Scripts.Constants;

namespace _Project.Scripts.DataClasses {
    public struct GPUNoiseParams {
        public DensityEnum noiseType;
        public int numLayers;
        public float lacunarity, persistence, noiseScale;
        public override string ToString() {
            return $"[noiseType:{noiseType}, numLayers:{numLayers}, lacunarity:{lacunarity}, persistence:{persistence}, noiseScale:{noiseScale}]";
        }
    }
}