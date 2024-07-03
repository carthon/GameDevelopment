using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class ItemStackSpawner : MonoBehaviour, Entity {
        public Item item;
        public int count;
        public void Update() {
            if (NetworkManager.Singleton.IsServer) {
                GameManager.SpawnItem(item, count, transform, this);
                Destroy(this.gameObject);
            } else if (NetworkManager.Singleton.IsClient)
                Destroy(this.gameObject);
        }
        public Planet GetPlanet() => GameManager.Singleton.defaultPlanet;
    }
}