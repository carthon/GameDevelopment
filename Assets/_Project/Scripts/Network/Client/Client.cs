using System;
using _Project.Scripts.Components;
using _Project.Scripts.DiegeticUI;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;
using Object = UnityEngine.Object;

#if !UNITY_SERVER
namespace _Project.Scripts.Network.Client {
    public partial class Client : RiptideNetworking.Client {
        private const float ServerPositionError = 0.01f;
        private static int TicksAheadOfServer = 5;
        public event Action OnClientReady;

        #region ReconciliationVariables
        private MovementMessageStruct[] _movementBuffer = new MovementMessageStruct[NetworkManager.BufferSize];
        private InputMessageStruct[] _inputBuffer = new InputMessageStruct[NetworkManager.BufferSize];
        private MovementMessageStruct _latestServerMovement;
        private MovementMessageStruct _lastProcessedMovement;
        private Vector3 _clientMovementError;
        #endregion
        
        private static Client _singleton;
        public static Client Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(Client)} instance already exists, destroying duplicate!");
                }
            }
        }
        public bool IsServerOwner;
        private ServerDummy _serverDummy;
        private Player _player;
        public Player Player {
            get
            {
                if (_player)
                    return _player;
                return null;
            }
        }
        public Client() {
            _singleton = this;
        }
        public void SetUpClient(Player player) {
            if (NetworkManager.Singleton.debugServerPosition)
                try {
                    _serverDummy = new ServerDummy(NetworkManager.Singleton.serverDummyPlayerPrefab);
                }
                catch (Exception e) {
                    Logger.Singleton.Log(e.Message, Logger.Type.WARNING);
                }
            _player = player;
            if(NetworkManager.Singleton.TryGetComponent(out ContainerRenderer renderer)) {
                renderer.InitializeRenderer(_player.InventoryManager, _player.inventorySpawnTransform);
            }
            GameManager.Singleton.ChunkRenderer.GenerateChunksAround(player.Planet, _player.transform.position, GameManager.Singleton.gameConfiguration.renderDistance);
            CameraHandler.Singleton.InitializeCamera(_player.Head, _player.HeadFollow, _player.HeadPivot);
            OnClientReady?.Invoke();
        }

        public void Tick(int currentTick) {
            if (_player && _player.IsLocal) {
                HandleCamera();
                HandlePlayer(currentTick);
                HandleWorld(currentTick);
            }
            base.Tick();
        }
        private void HandleCamera() {
            float fixedDelta = Time.fixedDeltaTime;
            float delta = Time.deltaTime;
            if (!InputHandler.Singleton.IsInMenu)
                CameraHandler.Singleton.FixedTick(fixedDelta);
            CameraHandler.Singleton.Tick(delta);
        }
        private void HandleWorld(int currentTick) {
            GameManager.Singleton.ChunkRenderer.GenerateChunksAround(_player.Planet, _player.transform.position, GameManager.Singleton.gameConfiguration.renderDistance);
            //TODO: Handle world creation
        }
        private void HandlePlayer(int currentTick) {
            int bufferIndex = currentTick % NetworkManager.BufferSize;
            Vector3 moveInput = new Vector3(InputHandler.Singleton.Horizontal, 0, InputHandler.Singleton.Vertical);
            
            bool[] actions = InputHandler.Singleton.GetActions();
            _player.SetActions(actions);
            _inputBuffer[bufferIndex] = SendInputs(moveInput, actions, currentTick);
            _player.HandleAnimations(actions);
            _player.HandleLocomotion(NetworkManager.Singleton.minTimeBetweenTicks, moveInput);
            _player.Locomotion.FixedTick();
            MovementMessageStruct movementMessage = _player.GetMovementState(currentTick);
            _movementBuffer[bufferIndex] = movementMessage;
            
            InputHandler.Singleton.ClearInputs();
            
            if (!_latestServerMovement.Equals(default(MovementMessageStruct)) &&
                (_lastProcessedMovement.Equals(default(MovementMessageStruct)) ||
                    !_latestServerMovement.Equals(_lastProcessedMovement)))
            {
                HandleServerReconciliation(currentTick);
            }
        }
        private InputMessageStruct SendInputs(Vector3 moveInput, bool[] actions, int currentTick) {
            InputMessageStruct inputData = new InputMessageStruct(moveInput, _player.HeadPivot.rotation, currentTick, actions);
            NetworkMessageBuilder networkMessageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) PacketHandler.serverInput, inputData);
            //TODO: Send relevant input feature
            networkMessageBuilder.Send(asClient:true);
            return inputData;
        }

        private void HandleServerReconciliation(int currentTick) {
            _lastProcessedMovement = _latestServerMovement;

            int serverMovementBufferIndex = _lastProcessedMovement.tick % NetworkManager.BufferSize;
            bool[] actions = _lastProcessedMovement.actions;
            //TODO: Verificar que las acciones recibidas del server están bien enviadas
            Vector3 positionError = (_lastProcessedMovement.position - _movementBuffer[serverMovementBufferIndex].position);
            if (positionError.sqrMagnitude > ServerPositionError)
            {
                // Rewind & Replay
                _player.UpdatePlayerMovementState(_lastProcessedMovement);

                // Update buffer at index of latest server state
                _movementBuffer[serverMovementBufferIndex] = _lastProcessedMovement;

                // Now re-simulate the rest of the ticks up to the current tick on the client
                int tickToProcess = _lastProcessedMovement.tick;

                while (tickToProcess < currentTick)
                {
                    int bufferIndex = tickToProcess % NetworkManager.BufferSize;
                    
                    _movementBuffer[bufferIndex] = _player.GetMovementState(tickToProcess);

                    // Process new movement with reconciled state
                    _player.HandleLocomotion(NetworkManager.Singleton.minTimeBetweenTicks, _inputBuffer[bufferIndex].moveInput);
                    
                    Physics.Simulate(NetworkManager.Singleton.minTimeBetweenTicks);


                    tickToProcess++;
                }
            }
        }

        private class ServerDummy {
            private readonly GameObject _self;
            private readonly AnimatorHandler _animator;
            private readonly Transform _transform;
            private readonly Rigidbody _rb;
            public ServerDummy(GameObject prefab) {
                _self = Object.Instantiate(prefab, Vector3.one, Quaternion.identity);
                _transform = _self.transform;
                _animator = _self.GetComponent<AnimatorHandler>();
                _animator.Initialize();
                _rb = _self.GetComponent<Rigidbody>();
                NetworkManager.Singleton.ServerDummy = _self;
            }
            public void UpdateServerDummy(MovementMessageStruct movementMessage) {
                _transform.position = movementMessage.position;
                _rb.rotation = movementMessage.rotation;
                _rb.velocity = movementMessage.velocity;
                _animator.UpdateAnimatorValues(movementMessage.relativeDirection.z, movementMessage.relativeDirection.x);
            }
        }
    }
#endif
}