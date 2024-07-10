using System;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class ItemStackSpawner : MonoBehaviour, IEntity {
        public Item item;
        public int count;
        private Planet _planet;
        private Chunk _spawnChunk;
        public void Start() {
            _planet = GameManager.Singleton.defaultPlanet;
        }
        public void Update() {
            if (NetworkManager.Singleton.IsServer) {
                _spawnChunk = _planet.FindChunkAtPosition(transform.position);
                if (_spawnChunk is not null && _spawnChunk.IsActive && GameManager.SpawnItem(item, count, transform, this))
                    Destroy(this.gameObject);
            } else if (NetworkManager.Singleton.IsClient)
                Destroy(this.gameObject);
        }
        public Planet GetPlanet() => _planet;
        public GameObject GetGameObject() => this.gameObject;
    }
}