using System;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Constants;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Factories;
using _Project.Scripts.Handlers;
using _Project.Scripts.Handlers.CameraHandler;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using RiptideNetworking;
using UnityEngine;
using static _Project.Scripts.Network.PacketType;
using Action = System.Action;
using Logger = _Project.Scripts.Utils.Logger;
using Object = UnityEngine.Object;

#if !UNITY_SERVER
namespace _Project.Scripts.Network.Client {
    public class ClientHandler : RiptideNetworking.Client {
        private const float ServerPositionError = 0.1f;
        private const float ServerPositionThreshold = 2f;
        private static int TicksAheadOfServer = 10;
        private bool _isSynced = false;
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
        private ServerDummy _serverDummy;
        private Player _player;
        private string _username;
        public readonly INetworkSender NetworkSender;
        private readonly IMovementApplier _movementApplier;
        public NetworkManager NetworkManager { get; }
        public Player Player {
            get
            {
                if (_player)
                    return _player;
                return null;
            }
        }
        public ClientHandler(INetworkSender networkSender, NetworkManager networkManager) {
            _singleton = this;
            NetworkSender = networkSender;
            NetworkManager = networkManager;
            _movementApplier = new DefaultMovementApplier();
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

        private void DidConnect(object sender, EventArgs args) {
            Logger.Singleton.Log("Connected succesfully!", Logger.Type.INFO);
            SendConnectionMessage();
        }
        private void FailedToConnect (object sender, EventArgs args){ Logger.Singleton.Log("Error trying to connect...!", Logger.Type.INFO); }
        private void DidDisconnect(object sender, EventArgs args) { Logger.Singleton.Log("Disconnected succesfully!", Logger.Type.INFO); }
        
        public void SetUpClient(Player player) {
            if (NetworkManager.Singleton.debugServerPosition && !NetworkManager.IsServer)
                try {
                    _serverDummy = new ServerDummy(NetworkManager.Singleton.serverDummyPlayerPrefab, NetworkManager);
                }
                catch (Exception e) {
                    Logger.Singleton.Log(e.Message, Logger.Type.WARNING);
                }
            _player = player;
            if (!NetworkManager.IsServer)
                GameManager.Singleton.ChunkRenderer.GenerateChunksAround(player.Planet, _player.transform.position, GameManager.Singleton.gameConfiguration.renderDistance);
            CameraHandler.Singleton.InitializeCamera(_player.Head, _player.HeadFollow, _player.HeadPivot);
            OnClientReady?.Invoke();
        }

        public void Tick(int currentTick) {
            if (_player && _player.IsLocal) {
                HandleCamera();
                HandlePlayer(currentTick);
                HandleWorld(currentTick);
                if (_player.Planet is not null) {
                    var position = _player.transform.position;
                    UIHandler.Instance.UpdateWatchedVariables("continentalness", $"Continentalness:{_player.Planet.GetHeightMapValuesAtPoint(position)}");
                    float planetSize = _player.Planet.NumChunks * 100;
                    UIHandler.Instance.UpdateWatchedVariables("planetheight", $"Planet height {position.magnitude / planetSize}");
                }
                UIHandler.Instance.UpdateWatchedVariables("2DPosition", $"2DPosition {MathUtility.SphericalToEquirectangular(_player.transform.position)}");
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
            if (!_isSynced) return;
            int bufferIndex = currentTick % NetworkManager.BufferSize;
            Vector3 moveInput = new Vector3(InputHandler.Singleton.Horizontal, 0, InputHandler.Singleton.Vertical);
            ulong actions = (ulong)InputHandler.Singleton.GetActions();
            _inputBuffer[bufferIndex] = SendInputs(moveInput, actions, currentTick);
            Logger.Singleton.Log($"Sending Inputs: {_inputBuffer[bufferIndex]}", Logger.Type.DEBUG);
            _movementApplier.ApplyMovement(_player, _inputBuffer[bufferIndex]);
            _player.Locomotion.FixedTick();
            MovementMessageStruct movementMessage = _player.GetMovementState(currentTick);
            _movementBuffer[bufferIndex] = movementMessage;
            InputHandler.Singleton.ClearInputs();
            if (ShouldReconcile())
                HandleServerReconciliation(currentTick);
        }
        public void ReceiveSpawnPlayer(Message message) {
            PlayerSpawnMessageStruct playerSpawnData = new PlayerSpawnMessageStruct(message);
            Player player;
            NetworkTimer networkTimer = NetworkManager.NetworkTimer;
            bool isLocal = Id == playerSpawnData.id;
            if (!NetworkManager.IsHost && !NetworkManager.IsServer) {
                if (isLocal) {
                    Logger.Singleton.Log($"Ajustando ticks de: {networkTimer.CurrentTick} a {playerSpawnData.tick + TicksAheadOfServer}: " +
                        $"Diferencia de {networkTimer.CurrentTick - playerSpawnData.tick + TicksAheadOfServer}", Logger.Type.DEBUG);
                    NetworkManager.NetworkTimer.CurrentTick = playerSpawnData.tick + TicksAheadOfServer;
                    _isSynced = true;
                }
                player = SpawnFactory.CreatePlayerInstance(
                    GameManager.Singleton.PlayerPrefab,
                    playerSpawnData.id,
                    playerSpawnData.entityId,
                    playerSpawnData.position);
                NetworkManager.playersList.Add(playerSpawnData.id, player);
            } else if (NetworkManager.playersList.TryGetValue(playerSpawnData.id, out player)) {
                _isSynced = true;
                Logger.Singleton.Log("Host se ha unido al servidor", Logger.Type.DEBUG);
            } else // El jugador es null
                return;
            // Inyectamos el sender de cliente (para que NotifyEquipment u otras llamadas hagan Send a servidor)
            player.SetNetworking(NetworkSender, NetworkManager);
            // Si el id coincide, es nuestro propio avatar local
            player.IsLocal = isLocal;
            //TODO: Realizar ajustes en la colisión servidor-servidor para replicarlo correctamente en cliente
            int playerLayer = global::_Project.Scripts.Constants.Constants.LAYER_REMOTEPLAYER;
            if (player.IsLocal && !NetworkManager.IsHost)
                playerLayer = global::_Project.Scripts.Constants.Constants.LAYER_LOCALPLAYER;
            player.gameObject.layer = playerLayer;
            if (player.IsLocal)
                SetUpClient(player);
        }
        public void ReceiveDespawnPlayer(Message message) {
            if (NetworkManager.IsHost) return;
            PlayerDespawnMessageStruct playerDespawn = new PlayerDespawnMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(playerDespawn._playerId, out Player player)) {
                Object.Destroy(player.gameObject);
                NetworkManager.playersList.Remove(playerDespawn._playerId);
            }
        }
        public void ReceiveGrabbableStatus(Message message) {
            if (NetworkManager.IsHost) return;
            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(message);
            if (GameManager.grabbableItems.TryGetValue(grabbableData.grabbableId, out Grabbable grabbable)) {
                var transform = grabbable.transform;
                transform.position = grabbableData.position;
                transform.rotation = grabbableData.rotation;
            }
        }
        public void ReceiveMovement(Message message) {
            if (NetworkManager.IsHost) return;
            MovementMessageStruct movementMessageStruct = new MovementMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(movementMessageStruct.id, out Player player)) {
                Logger.Singleton.Log($"[{NetworkManager.NetworkTimer.CurrentTick}]Received Movement: {movementMessageStruct}", Logger.Type.DEBUG);
                if(player.IsLocal) {
                    _latestServerMovement = movementMessageStruct;
                    if (NetworkManager.debugServerPosition && _serverDummy != null) {
                        _serverDummy.UpdateServerDummy(movementMessageStruct);
                    }
                } else {
                    player.UpdatePlayerMovementState(movementMessageStruct, true, Time.deltaTime * 
                        movementMessageStruct.velocity.sqrMagnitude);
                    UIHandler.Instance.UpdateWatchedVariables("DiffTicks", 
                        $"Diff between server ticks {NetworkManager.NetworkTimer.CurrentTick - movementMessageStruct.tick}");
                }
            }
        }
        public void ReceiveEquipment(Message message) {
            if (NetworkManager.IsHost) return;
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
            if (NetworkManager.IsHost) return;
            PlayerDataMessageStruct playerData = new PlayerDataMessageStruct(message);
            //NetworkManager.Tick = playerData.tick + TicksAheadOfServer;
        }
        private InputMessageStruct SendInputs(Vector3 moveInput, ulong actions, int currentTick) {
            InputMessageStruct inputData = new InputMessageStruct(moveInput, _player.HeadPivot.rotation, currentTick, actions);
            NetworkSender.SendToServer(MessageSendMode.unreliable, (ushort) serverInput, inputData);
            //TODO: Send relevant input feature
            return inputData;
        }
        private void SendConnectionMessage() {
            NetworkSender.SendToServer(MessageSendMode.reliable, (ushort) serverUsername, new PlayerConnectionMessageStruct(_username));
        }
        public void ReceiveSpawnItem(Message message) {
            if (NetworkManager.IsHost) return;
            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(message);
            Debug.Log($"Trying to get value : {grabbableData.itemStack}");
            if (NetworkManager.Singleton.itemsDictionary.TryGetValue(grabbableData.itemStack.Item.Id, out Item prefabData)) {
                if (!GameManager.grabbableItems.TryGetValue(grabbableData.grabbableId, out Grabbable grabbable)) {
                    grabbable = SpawnFactory.CreateGrabbableInstance(grabbableData.grabbableId, grabbableData.itemStack, grabbableData.position, grabbableData.rotation);
                }
                else {
                    Transform transform = grabbable.transform;
                    transform.position = grabbableData.position;
                    transform.rotation = grabbableData.rotation;
                }
            }
        }
        public void ReceiveDestroyItem(Message message) {
            if (NetworkManager.IsHost) return;
            GrabbableMessageStruct grabbablePickupMessage = new GrabbableMessageStruct(message);
            if (!GameManager.grabbableItems.TryGetValue(grabbablePickupMessage.grabbableId, out Grabbable grabbable))
                return;
            Object.Destroy(grabbable.gameObject);
        }
        public void ReceiveInventorySlotChange(Message message) {
            if (NetworkManager.IsHost) return;
            InventorySlotMessageStruct inventorySlotMessageStruct = new InventorySlotMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(inventorySlotMessageStruct.ownerId, out Player player)) {
                if (player.IsLocal) return;
                player.InventoryManager.SetInventorySlot(inventorySlotMessageStruct.itemSlot, inventorySlotMessageStruct.inventoryId);
            }
        }
        private bool ShouldReconcile() {
            return  !NetworkManager.IsHost && !_latestServerMovement.Equals(default(MovementMessageStruct)) &&
                (_lastProcessedMovement.Equals(default(MovementMessageStruct)) ||
                    !_latestServerMovement.Equals(_lastProcessedMovement));
        }
        private void HandleServerReconciliation(int currentTick) {
            if (NetworkManager.IsHost) return;
            _lastProcessedMovement = _latestServerMovement;

            int serverMovementBufferIndex = _lastProcessedMovement.tick % NetworkManager.BufferSize;
            ulong actions = _lastProcessedMovement.actions;
            //TODO: Verificar que las acciones recibidas del server están bien enviadas
            Vector3 positionError = (_lastProcessedMovement.position - _movementBuffer[serverMovementBufferIndex].position);
            UIHandler.Instance.UpdateWatchedVariables("PositionError", $"PositionError:{positionError.sqrMagnitude}");
            if (positionError.sqrMagnitude is > ServerPositionError and < ServerPositionThreshold)
            {
                Logger.Singleton.Log($"Rewind : positionError {positionError.sqrMagnitude} lastMovementTick: {_lastProcessedMovement.tick}", Logger.Type.DEBUG);
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
                    _player.HandleLocomotion(_inputBuffer[bufferIndex]);
                    _player.Locomotion.FixedTick();
                    tickToProcess++;
                }
            } else if (positionError.sqrMagnitude >= ServerPositionThreshold) {
                _player.UpdatePlayerMovementState(_lastProcessedMovement);
            }
        }

        private class ServerDummy {
            private readonly GameObject _self;
            private readonly AnimatorHandler _animator;
            private readonly Transform _transform;
            private readonly Rigidbody _rb;
            private readonly NetworkManager _networkManager;
            public ServerDummy(GameObject prefab, NetworkManager networkManager) {
                _self = Object.Instantiate(prefab, Vector3.one, Quaternion.identity);
                _transform = _self.transform;
                _animator = _self.GetComponent<AnimatorHandler>();
                _animator.Initialize();
                _rb = _self.GetComponent<Rigidbody>();
                _networkManager = networkManager;
                networkManager.ServerDummy = _self;
            }
            public void UpdateServerDummy(MovementMessageStruct movementMessage) {
                _transform.position = movementMessage.position;
                _transform.rotation = movementMessage.modelRotation;
                _rb.velocity = movementMessage.velocity;
                _animator.UpdateAnimatorValues(movementMessage.relativeDirection.z, movementMessage.relativeDirection.x,
                    _networkManager.NetworkTimer.MinTimeBetweenTicks);
            }
        }
        public void SetUsername(string username) {
            _username = username;
        }
    }
#endif
}