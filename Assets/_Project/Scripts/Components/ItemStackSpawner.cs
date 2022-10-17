using UnityEngine;

namespace _Project.Scripts.Components {
    public class ItemStackSpawner : MonoBehaviour {
        public Item item;
        public int count;
        public void Update() {
            if (NetworkManager.Singleton.IsServer) {
                GodEntity.SpawnItem(item, count, transform);
                Destroy(this);
            } else if (NetworkManager.Singleton.IsClient)
                Destroy(this.gameObject);
        }
    }
}