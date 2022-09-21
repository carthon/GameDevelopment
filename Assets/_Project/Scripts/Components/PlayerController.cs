using System;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using Cinemachine;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEditor;
using UnityEngine;

public class PlayerController : NetworkBehaviour {

    [SerializeField]
    private float _pickUpDistance = 2f;
    public bool tickInputHandler;
    public Transform respawnPoint;
    private AnimatorHandler _animatorHandler;
    private CameraHandler _cameraHandler;
    private EquipmentHandler _equipmentHandler;
    private InputHandler _inputHandler;

    private Inventory _inventory;
    private Grabler _grabler;
    private Locomotion _locomotion;
    private Rigidbody _rigidbody;
    private bool _subscribed;

    private Vector2 _moveDirection;
    private UIHandler _uiHandler;
    public void Awake() {
        _locomotion = GetComponent<Locomotion>();
        _inputHandler = GetComponent<InputHandler>();
        _animatorHandler = GetComponent<AnimatorHandler>();
        _rigidbody = GetComponent<Rigidbody>();
        _grabler = GetComponent<Grabler>();
        _equipmentHandler = GetComponent<EquipmentHandler>();
        _uiHandler = GodEntity.Instance.GetUIHandler();
        _animatorHandler.Initialize();
        _locomotion.SetUp();
        Cursor.lockState = CursorLockMode.None;
    }
    public override void OnStartServer() {
        base.OnStartServer();
        SubscribeToTimeManager(true);
    }
    public override void OnStartClient() {
        base.OnStartClient();
        SubscribeToTimeManager(true);
        if (!IsOwner)
            return;
        _inventory = new Inventory("PlayerInventory", 9);
        _grabler.SetInventory(_inventory);
        _uiHandler.AddInventory(_inventory);
        tickInputHandler = true;
        Cursor.lockState = CursorLockMode.Locked;
        HandleCameraInitialization();
    }
    public override void OnStopClient() {
        base.OnStopClient();
        Cursor.lockState = CursorLockMode.None;
    }
    private void OnDestroy() {
        SubscribeToTimeManager(false);
    }
    public void FixedUpdate() {
        if (!IsOwner)
            return;
        var delta = Time.fixedDeltaTime;
        if (tickInputHandler) {
            _inputHandler.TickInput(delta);
            HandleItemPickUp(delta);
            _cameraHandler.Tick(delta);
        }
        HandleUIInteractions(delta);
        HandleAnimations(delta);
        if (tickInputHandler && !_inputHandler.IsUIEnabled)
            _cameraHandler.FixedTick(delta);
    }
    private void HandleAnimations(float delta) {
        _animatorHandler.UpdateAnimatorValues(_inputHandler.Vertical, _inputHandler.Horizontal, _inputHandler.IsSprinting);
        _animatorHandler.SetMoving(_locomotion.IsMoving);
        _animatorHandler.SetSprinting(_locomotion.IsSprinting);
        if (_locomotion.IsJumping) _animatorHandler.TriggerJump();
    }
    private void HandleUIInteractions(float delta) {
        if (_inputHandler.IsUIEnabled)
            Cursor.lockState = CursorLockMode.None;
        else if (Cursor.lockState == CursorLockMode.None && !_inputHandler.IsUIEnabled) Cursor.lockState = CursorLockMode.Locked;
        if (_inputHandler.EquipInput || _uiHandler.UpdateVisuals) {
            var hotbar = UIHandler.Instance._hotbarUi;
            hotbar.ActiveSlot = _inputHandler.HotbarSlot;
            var linkedItemLinkInSlot = hotbar.GetItemLinkInSlot(hotbar.ActiveSlot);
            if (linkedItemLinkInSlot != null && linkedItemLinkInSlot.LinkedStacks.Count > 0 && linkedItemLinkInSlot.LinkedStacks[0].GetCount() > 0)
                _equipmentHandler.LoadItemModel(linkedItemLinkInSlot.LinkedStacks[0], BodyPart.RightArm);
            else
                _equipmentHandler.UnloadItemModel(BodyPart.RightArm);
            if (_uiHandler.UpdateVisuals) _uiHandler.UpdateVisuals = false;
        }
    }
    private void HandleCameraInitialization() {
        _cameraHandler = GetComponent<CameraHandler>();
        _cameraHandler.SetOrbitalCamera(GameObject.Find("OrbitalCamera").GetComponent<CinemachineVirtualCamera>());
        _cameraHandler.SetFirstPersonCamera(GameObject.Find("1stPersonCamera").GetComponent<CinemachineVirtualCamera>());
        _cameraHandler.SetThirdPersonCamera(GameObject.Find("3rdPersonCamera").GetComponent<CinemachineVirtualCamera>());
        _cameraHandler.InitializeCamera();
        _cameraHandler.SetOrbitalInput(true);
    }

    [Replicate]
    private void HandleLocomotion(MoveData data, bool isServer, bool replaying = false) {
        var thisTransform = transform;
        var calculateDirection = data.Vertical * thisTransform.forward +
            data.Horizontal * thisTransform.right;
        calculateDirection = calculateDirection.normalized;
        _moveDirection = new Vector2(calculateDirection.x, calculateDirection.z);
        _locomotion.IsMoving = data.isMoving;
        _locomotion.IsSprinting = data.isSprinting;
        _locomotion.RelativeDirection = new Vector3(data.Horizontal, 0, data.Vertical);
        _locomotion.TargetPosition = new Vector3(_moveDirection.x, 0, _moveDirection.y).normalized;
        _locomotion.Tick();
        HandleRotation(data, isServer, replaying);
    }
    private void HandleRotation(MoveData moveData, bool isServer, bool replaying = false) {
        var rb = _locomotion.Rb;
        rb.rotation = Quaternion.Euler(0.0f, rb.rotation.eulerAngles.y +
            moveData.RotationX, 0.0f);
    }

    public void HandleItemPickUp(float delta) {
        _grabler.CanPickUp = _inputHandler.IsPicking;
        var rayOrigin = _cameraHandler.MainCamera.ViewportPointToRay(Vector3.one / 2);
        rayOrigin.origin = _cameraHandler.CameraFollow.position;
        var pickable = _grabler.GetPickableInRange(rayOrigin, _pickUpDistance);
        if (pickable) {
            _uiHandler.ItemPickerUI.HandlePickUpUI(_cameraHandler.MainCamera, pickable.transform);
            if (_inputHandler.IsPicking)
                _grabler.TryPickItems(pickable);
        }
        else {
            if (_uiHandler.ItemPickerUI.gameObject.activeSelf)
                _uiHandler.ItemPickerUI.gameObject.SetActive(false);
        }
    }
    public void Respawn() {
        var thisTransform = transform;
        thisTransform.position = respawnPoint.position;
        thisTransform.rotation = respawnPoint.rotation;
    }

    #region NETCODE
    private struct MoveData {
        public float Horizontal;
        public float Vertical;
        public float RotationX;
        public bool isMoving;
        public bool isSprinting;
        public MoveData(float horizontal, float vertical, float rotationX, bool isMoving, bool isSprinting) {
            Horizontal = horizontal;
            Vertical = vertical;
            RotationX = rotationX;
            this.isMoving = isMoving;
            this.isSprinting = isSprinting;
        }
    }

    /// <summary>
    /// Contiene información de como debe reiniciarse el objecto a valores del servidor. Estos son los valores que se enviarán al cliente
    /// </summary>
    private struct ReconcileData {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        
        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }
    }
    
    private void OnTick() {
        if (IsOwner) {
            /* La reconciliacion debe pasar primero
             * Esto arregla la posicion de los clientes a la que debe ser en el servidor
             * Y cachea los inputs de los clientes
             * Cuando se usa la reconciliacion  los datos cliente deben ser default y marcar el servidor como falso
             * De esta forma se hace una reconciliacion del lado del cliente.
             */
            Reconciliation(default, false);
            /* Recopila información de como se mueve el objeto. Usado por cliente y servidor */
            GatherInputs(out MoveData data);
            HandleLocomotion(data, false);
        }
        if (IsServer) {
            /*
             * El server tiene que mover lo mismo que el cliente, esto ayuda a mantener el objeto en sincronizacion.
             * Pasalos valores por defecto como data y se marca servidor como true. El servidor automáticamente
             * sabe que data tiene que usar cuando asServer es true. Como cuando se llama desde el cliente y no quiere marcar
             * el booleano réplica
             */
            HandleLocomotion(default, true);
            /*
             * Como se muestra abajo la reconciliacion se envía usando PostTick porque quieres que la posicion de los objetos, rotacion etc,
             * se envíen antes de que las físicas se hayan simulado.
             * Si estás usando un método de movimiento que no usa fisicas, como el caracter controller o moviendo el transform directamente,
             * puedes opt-out usando el OnPostTick y enviando la reconciliación desde aquí.
             */
        }
    }
    /* OnPostTick se ejecuta despúes de haber simulado las físicas */
    private void OnPostTick() {
        if (IsServer) {
            /*
             * Construye la reconciliacion usando los datos actuales del objeto. Esto se envía al cliente
             * y el cliente se resetea usando estos valores. Es EXTREMADAMENE importante enviar cualquier cosa que pueda
             * afectar a movimiento, rotación y posición del objeto. Esto incluye y no está limitado por:
             * transforms (position,rotation), rigidbody velocities, colliders, etc.
             *
             * En detalle: si estás usando predición en un vehiculo que esta controlado por colliders en las ruedas, esos colliders
             * se comportan de manera independiente la raíz del vehículo. Se deben enviar la posición de los colliders, la rotación y otros
             * valores que puedan afectar al movimiento
             *
             * Otro ejemplo sería correr con estamina. Si correr depende de la estamina tambien quieres enviar la estamina junto con el
             * estado de correr para que el cliente pueda ajustarse localmente si difiere de como está en el servidor.
             * Si la estamina existiera en em cliente pero no en el servidor, entonces el servidor se movería más despacio y se desincronizaría.
             * Si no envías la stamina al cliente continuarían desincronizados hasta que también se quedase sin estamina.
             *
             * Si estas uasndo un asset que usa fisicas internas es bastante posible que necesites enviar eso valores que afectan
             * al movimiento.
             *
             * Cuando todos los datos se resetean correctamente las probabilidades de desync son muy bajas, y casi imposibles cuando no se usan fisicas.
             * Incluso cuando se desyncronisza la proibabilidad es bastante baja y se corregirá sin turbulencias visuales.
             * Hay algunos casos sin embargo, en los que si la desync es demasiado seria el cliente se puede teleportar al valor correcto.
             * Se incluye in componente para reducir cualcuier turbulencia visual durante largas desincronizaciones.
             */
            ReconcileData data = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity);
            /*
             * Después de construir los datos hay que enviarselos al método de reconciliacion marcando true para servidor. Se puede llamar al metodo Reconcile cada tick
             * en el servidor y el cliente. Fish-Networking sabe internamente cuando los datos son nuevos o no y no tirará ancho de banda reenviando
             * datos que no son nuevos. Bastante fachero.
             */
            Reconciliation(data, true);
        }
    }
    private void SubscribeToTimeManager(bool subscribe) {
        if (base.TimeManager == null) {
            return;
        }
        if (subscribe == _subscribed) {
            return;
        }
        _subscribed = subscribe;
        if (subscribe) {
            base.TimeManager.OnTick += OnTick;
            base.TimeManager.OnPostTick += OnPostTick;
        }
        else {
            base.TimeManager.OnTick -= OnTick;
            base.TimeManager.OnPostTick -= OnPostTick;
        }
    }
    private void GatherInputs(out MoveData data) {
        data = default;
        if (_inputHandler != null && tickInputHandler) {
            _locomotion.IsSprinting = _inputHandler.IsSprinting;
            _locomotion.IsJumping = _inputHandler.IsJumping;
            _locomotion.IsMoving = Math.Abs(_inputHandler.Horizontal) > 0 || Math.Abs(_inputHandler.Vertical) > 0;
            
            float horizontal = _inputHandler.Horizontal;
            float vertical = _inputHandler.Vertical;
            Vector3 rotationX = _cameraHandler.GetLookInput(_inputHandler.MouseX, _inputHandler.MouseY);
            float resultRotation = rotationX.x * _cameraHandler.CameraData.rotationMultiplier;

            data = new MoveData(horizontal, vertical, resultRotation, _locomotion.IsMoving, _inputHandler.IsSprinting);
        }
    }

    [Reconcile]
    private void Reconciliation(ReconcileData data, bool asServer) {
        transform.position = data.Position;
        transform.rotation = data.Rotation;
        _rigidbody.velocity = data.Velocity;
        _rigidbody.angularVelocity = data.AngularVelocity;
    }
    #endregion
}