using _Project.Scripts.Components.LocomotionComponent.Logic;
using _Project.Scripts.DataClasses;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Components.LocomotionComponent {
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class LocomotionBridge : MonoBehaviour {
        // --- 1) Referencias Unity ---
        private Rigidbody _rb;
        private Transform _tf;
        private Transform _model;
        private CapsuleCollider _capsule;

        public LocomotionStats Stats; // tu estructura de estadísticas (runSpeed, jumpStrength, etc.)

        // --- 2) Motor determinista ---
        public LocomotionStateMessage StateData;
        private LocomotionProcessor _processor;
        private float _fixedDelta;

        // --- 3) Buffers circulares ---
        private int _bufferSize;
        private LocomotionInputMessage[] _inputBuffer;
        private LocomotionStateMessage[] _movementBuffer;

        // --- 4) Reconciliación cliente local ---
        private LocomotionStateMessage _latestServerMove;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _tf = transform;
            _capsule = GetComponent<CapsuleCollider>();
        }

        private void Start() {
            _fixedDelta = Time.fixedDeltaTime;
            _bufferSize = NetworkManager.BufferSize; // p. ej. 1024

            // 1) Inicializar stateData desde transform y Rb actuales
            StateData = new LocomotionStateMessage {
                position = _rb.position,
                velocity = _rb.velocity,
                modelRotation = _tf.rotation,
                isGrounded = true,
                mode = LocomotionMode.Grounded,
                tick = 0,
                actions = 0UL
            };

            // 2) Crear processor
            var initialInput = new LocomotionInputMessage(
                Vector3.zero,
                _tf.rotation,
                0,
                0UL
            );
            _processor = new LocomotionProcessor(StateData, initialInput, Stats);

            // 3) Reservar buffers
            _inputBuffer = new LocomotionInputMessage[_bufferSize];
            _movementBuffer = new LocomotionStateMessage[_bufferSize];
        }

        public void SetUp(PlanetData planetData, Transform model, float delta) {
            StateData.planetData = planetData;
            _model = model;
            _fixedDelta = delta;
        }

        #region → Encolar y procesar tick

        /// <summary>
        /// Enqueuea un InputMessageStruct (cliente local o desde red).
        /// </summary>
        public void EnqueueInput(LocomotionInputMessage msg) {
            int idx = msg.tick % _bufferSize;
            _inputBuffer[idx] = msg;
        }
        public void UpdateGrounded() {
            // 3) Calcular IsGrounded (antes de tick). Ejemplo simplificado:
            var position = _rb.position;
            Vector3 upDir = (position - StateData.planetData.Center).normalized;
            Vector3 origin = position + (upDir * (_capsule.height * 0.5f + _capsule.radius));
            if (!Stats.ignoreGround) {
                bool grounded = Physics.Raycast(origin, -upDir, out RaycastHit hit, Stats.height, Stats.groundLayer);
                StateData.isGrounded = grounded;
            }
            else {
                StateData.isGrounded = false;
            }
        }
        public void ServerProcessTick(int currentTick, ushort clientId, in LocomotionInputMessage input, out LocomotionStateMessage stateMessage) {
            // 1) Calcular IsGrounded
            UpdateGrounded();

            // 2) Llamar a processor determinista
            _processor.Tick(ref StateData, in input, Stats, _fixedDelta);

            // 3) Aplicar a Unity
            _tf.position = StateData.position;
            _rb.velocity = StateData.velocity;
            _model.rotation = StateData.modelRotation;
            _capsule.isTrigger = Stats.ignoreGround;

            // 4) Enviar resultado a todos los clientes
            stateMessage = StateData;
        }
        public void ClientProcessTick(int currentTick, in LocomotionInputMessage inputMessage, out LocomotionStateMessage stateData) {
            // 4) Ejecutar lógica determinista
            _processor.Tick(ref StateData, in inputMessage, Stats, _fixedDelta);

            // 5) Aplicar a Unity
            _tf.position = StateData.position;
            _rb.velocity = StateData.velocity;
            _model.rotation = StateData.modelRotation;

            // Mantener collider en trigger si ignoreGround
            _capsule.isTrigger = Stats.ignoreGround;
            stateData = StateData;
        }

        #endregion

        #region → Reconciliación en cliente local


        /// <summary>
        /// Recibe el último Movimiento del servidor para este jugador local.
        /// </summary>
        public void ReceiveServerMovement(LocomotionStateMessage serverMove) {
            _latestServerMove = serverMove;
        }

        /// <summary>
        /// Compara la predicción local con lo que envió el servidor:
        /// - Si el error es pequeño: rewind & replay.
        /// - Si es grande: snap to server.
        /// </summary>
        public void Reconcile(int currentTick) {
            int idxServer = _latestServerMove.tick % _bufferSize;
            var serverData = _latestServerMove;

            var localPred = _movementBuffer[idxServer];
            Vector3 ours = localPred.position;
            Vector3 goal = serverData.position;
            float sqrErr = (goal - ours).sqrMagnitude;

            const float SMALL_ERR = 0.1f;
            const float LARGE_THRESH = 2f;

            if (sqrErr <= SMALL_ERR * SMALL_ERR) {
                // Rewind & replay
                _movementBuffer[idxServer] = serverData;
                int tickToSim = serverData.tick;
                while (tickToSim < currentTick) {
                    int idx = tickToSim % _bufferSize;
                    var inp = _inputBuffer[idx];
                    UpdateGrounded();
                    ClientProcessTick(tickToSim, inp, out LocomotionStateMessage stateData);
                    _movementBuffer[idx] = stateData;
                    tickToSim++;
                }
            }
            else if (sqrErr >= LARGE_THRESH * LARGE_THRESH) {
                // Snap to server
                _tf.position = serverData.position;
                _model.rotation = serverData.modelRotation;
                _rb.velocity = serverData.velocity;

                StateData.position = serverData.position;
                StateData.velocity = serverData.velocity;
                StateData.modelRotation = serverData.modelRotation;
                StateData.tick = serverData.tick;
                StateData.actions = serverData.actions;
            }
        }
        #endregion

        private void OnDrawGizmos() {
            if (_rb != null) {
                Vector3 rbPosition = _rb.position;
                Vector3 upDir = (rbPosition - StateData.planetData.Center).normalized;
                Vector3 castOrigin = rbPosition + upDir * (-_capsule.height / 2f + _capsule.radius);
                float groundedRayRadius = _capsule.radius * Stats.groundedRayRadius;

                float groundedRayDst = _capsule.radius - groundedRayRadius + Stats.height;
                Gizmos.color = (StateData.isGrounded) ? Color.green : Color.red;
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
                Gizmos.DrawSphere(castOrigin, groundedRayRadius);


                Vector3 collisionSphereTip = castOrigin - upDir * (groundedRayRadius + groundedRayDst);
                Gizmos.DrawSphere(collisionSphereTip + upDir * groundedRayRadius, groundedRayRadius);
                Gizmos.color = (StateData.isGrounded) ? Color.green : Color.red;
                Gizmos.DrawRay(castOrigin - upDir * groundedRayRadius, -upDir * groundedRayDst);
            }
        }
    }
}