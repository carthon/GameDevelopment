using System;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Factories;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static _Project.Scripts.Network.PacketType;
using Action = System.Action;
using Logger = _Project.Scripts.Utils.Logger;
using Object = UnityEngine.Object;

#if !UNITY_SERVER
namespace _Project.Scripts.Network.Client {
    public partial class ClientHandler : RiptideNetworking.Client {
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
        
        private static ClientHandler _singleton;
        public static ClientHandler Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(ClientHandler)} instance already exists, destroying duplicate!");
                }
            }
        }
        public bool IsServerOwner;
        private ServerDummy _serverDummy;
        private Player _player;
        public INetworkSender NetworkSender;
        public Player Player {
            get
            {
                if (_player)
                    return _player;
                return null;
            }
        }
        public ClientHandler(INetworkSender networkSender) {
            _singleton = this;
            NetworkSender = networkSender;
            Connected += DidConnect;
            Disconnected += DidDisconnect;
            ConnectionFailed += FailedToConnect;
        }

        ~ClientHandler() {
            Connected -= DidConnect;
            Disconnected -= DidDisconnect;
            ConnectionFailed -= FailedToConnect;
            GC.SuppressFinalize(this);
        }
        
        private void DidConnect(object sender, EventArgs args) { Logger.Singleton.Log("Connected succesfully!", Logger.Type.INFO); }
        private void FailedToConnect (object sender, EventArgs args){ Logger.Singleton.Log("Error trying to connect...!", Logger.Type.INFO); }
        private void DidDisconnect(object sender, EventArgs args) { Logger.Singleton.Log("Disconnected succesfully!", Logger.Type.INFO); }
        
        public void SetUpClient(Player player) {
            if (NetworkManager.Singleton.debugServerPosition)
                try {
                    _serverDummy = new ServerDummy(NetworkManager.Singleton.serverDummyPlayerPrefab);
                }
                catch (Exception e) {
                    Logger.Singleton.Log(e.Message, Logger.Type.WARNING);
                }
            _player = player;
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
        public void OnReceiveSpawn(PlayerSpawnMessageStruct playerSpawnMessageStruct) {
            var player = SpawnFactory.CreatePlayerInstance(
                GameManager.Singleton.PlayerPrefab,
                playerSpawnMessageStruct.id,
                playerSpawnMessageStruct.entityId,
                playerSpawnMessageStruct.position);

            // Inyectamos el sender de cliente (para que NotifyEquipment u otras llamadas hagan Send a servidor)
            player.SetNetworking(NetworkSender);

            // Si el id coincide, es nuestro propio avatar local
            player.IsLocal = (playerSpawnMessageStruct.id == Id);

            NetworkManager.playersList.Add(playerSpawnMessageStruct.id, player);

            if (player.IsLocal)
                SetUpClient(player);
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
        public void ReceiveSpawnPlayer(Message message) {
            if (IsServerOwner) return;
            PlayerSpawnMessageStruct playerSpawnData = new PlayerSpawnMessageStruct(message);
            NetworkManager.Singleton.Tick = playerSpawnData.tick + TicksAheadOfServer;
            NetworkManager.Singleton.ClientHandler.OnReceiveSpawn(playerSpawnData);
            Message updateClient = Message.Create(MessageSendMode.reliable, serverUpdateClient);
            NetworkManager.Singleton.ClientHandler.Send(updateClient);
        }
        public void ReceiveDespawnPlayer(Message message) {
            if (IsServerOwner) return;
            ushort playerId = message.GetUShort();
            if (NetworkManager.playersList.TryGetValue(playerId, out Player player)) {
                Object.Destroy(player.gameObject);
                NetworkManager.playersList.Remove(playerId);
            }
        }
        public void ReceiveGrabbableStatus(Message message) {
            if (IsServerOwner) return;
            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(message);
            if (GameManager.grabbableItems.TryGetValue(grabbableData.grabbableId, out Grabbable grabbable)) {
                var transform = grabbable.transform;
                transform.position = grabbableData.position;
                transform.rotation = grabbableData.rotation;
            }
        }
        public void ReceiveMovement(Message message) {
            if (IsServerOwner) return;
            MovementMessageStruct movementMessageStruct = new MovementMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(movementMessageStruct.id, out Player player)) {
                if(player.IsLocal) {
                    Singleton._latestServerMovement = movementMessageStruct;
                    if (NetworkManager.Singleton.debugServerPosition && NetworkManager.Singleton.ClientHandler._serverDummy != null) {
                        NetworkManager.Singleton.ClientHandler._serverDummy.UpdateServerDummy(movementMessageStruct);
                    }
                } else {
                    bool isInstant = Vector3.Distance(player.transform.position, movementMessageStruct.position) > 5f;
                    player.UpdatePlayerMovementState(movementMessageStruct, isInstant, Time.deltaTime * movementMessageStruct.velocity.sqrMagnitude);
                }
            }
        }
        public void ReceiveEquipment(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;
            EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(equipmentData.clientId, out Player player)) {
                if (!player.IsLocal) {
                    ItemStack itemStack = equipmentData.itemStack;
                    BodyPart equipmentSlot = (BodyPart) equipmentData.equipmentSlot;
                    bool activeStatus = equipmentData.activeState;
                    player.UpdateEquipment(itemStack, equipmentSlot, activeStatus);
                }
            }
        }
        public void ReceivePlayerData(Message message) {
            if (IsServerOwner) return;
            PlayerDataMessageStruct playerData = new PlayerDataMessageStruct(message);
            NetworkManager.Singleton.Tick = playerData.tick + TicksAheadOfServer;
        }
        private InputMessageStruct SendInputs(Vector3 moveInput, bool[] actions, int currentTick) {
            InputMessageStruct inputData = new InputMessageStruct(moveInput, _player.HeadPivot.rotation, currentTick, actions);
            NetworkSender.Send(MessageSendMode.reliable, (ushort) serverInput, inputData);
            //TODO: Send relevant input feature
            return inputData;
        }
        public void SendConnectionMessage(string username) {
            NetworkSender.Send(MessageSendMode.reliable, (ushort)serverUsername, new PlayerConnectionMessageStruct(username));
        }
        public void ReceiveSpawnItem(Message message) {
            if (NetworkManager.Singleton.IsServer)
                return;
            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(message);
            Debug.Log($"Trying to get value : {grabbableData.itemStack}");
            if (NetworkManager.Singleton.itemsDictionary.TryGetValue(grabbableData.itemStack.Item.id, out Item prefabData)) {
                if (!GameManager.grabbableItems.TryGetValue(grabbableData.grabbableId, out Grabbable grabbable)) {
                    grabbable = SpawnFactory.CreateGrabbableInstance(grabbableData.itemStack, grabbableData.position, grabbableData.rotation);
                }
                else {
                    Transform transform = grabbable.transform;
                    transform.position = grabbableData.position;
                    transform.rotation = grabbableData.rotation;
                }
            }
        }
        public void ReceiveDestroyItem(Message message) {
            if (NetworkManager.Singleton.IsServer)
                return;
            ushort grabbableId = message.GetUShort();
            if (GameManager.grabbableItems.TryGetValue(grabbableId, out Grabbable grabbable)) {
                Object.Destroy(grabbable.gameObject);
            }
            GameManager.grabbableItems.Remove(grabbableId);
        }
        public void ReceiveInventorySlotChange(Message message) {
            InventorySlotMessageStruct inventorySlotMessageStruct = new InventorySlotMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(inventorySlotMessageStruct.ownerId, out Player player)) {
                if (player.IsLocal) return;
                player.InventoryManager.SetInventorySlot(inventorySlotMessageStruct.itemSlot, inventorySlotMessageStruct.inventoryId);
            }
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