using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Network;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class ItemStackSpawner : MonoBehaviour {
        public Item item;
        public int count;
        public void Update() {
            if (NetworkManager.Singleton.IsServer) {
                GodEntity.SpawnItem(item, count, transform);
                Destroy(this.gameObject);
            } else if (NetworkManager.Singleton.IsClient)
                Destroy(this.gameObject);
        }
    }
}