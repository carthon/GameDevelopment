using UnityEngine;

namespace _Project.Scripts.DataClasses.WorldData {
    public class ChunkData {
        public Vector3 centre;
        public Vector3Int id;
        public float size;
        public Texture3D densityMap;

        public ChunkData(Vector3Int coords, Vector3 centre, float size) {
            this.centre = centre;
            this.id = coords;
            this.size = size;
        }
    }
}