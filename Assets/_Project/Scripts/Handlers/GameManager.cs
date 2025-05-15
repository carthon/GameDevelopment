using System.Collections.Generic;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Network;
using _Project.Scripts.Network.Client;
using _Project.Scripts.Network.Server;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Handlers {
    [ExecuteInEditMode]
    [RequireComponent(typeof(ChunkRenderer))]
    public class GameManager : MonoBehaviour {
        public Transform spawnPoint;
        [SerializeField] public Planet defaultPlanet;
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        [Header("WorldData")]
        public static Dictionary<ushort, Grabbable> grabbableItems = new Dictionary<ushort, Grabbable>();
        public ChunkRenderer ChunkRenderer;
        public GameConfiguration gameConfiguration;
        public GameObject PlayerPrefab { get; private set; }
        private static GameManager _singleton;
        public static GameManager Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }
        private void Awake() {
            PlayerPrefab = _playerPrefab;
            Application.targetFrameRate = -1;
            Initialize();
        }
        public void Initialize() {
            Singleton ??= this;
            ChunkRenderer = GetComponent<ChunkRenderer>();
            ChunkRenderer ??= gameObject.AddComponent<ChunkRenderer>();
        }
    }
}