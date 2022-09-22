using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

public class PredictionMotor : NetworkBehaviour
{
    private struct MoveData {
        public float Horizontal;
        public float Vertical;
        public MoveData(float horizontal, float vertical) {
            Horizontal = horizontal;
            Vertical = vertical;
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
    public float moveRate = 10f;
    private Rigidbody _rigidbody;
    private bool _subscribed = false;
    private InputHandler _inputHandler;

    private void Awake() {
        // El servidor y el dueño deben tener una referencia al rigid body.
        // * Las fuerzas se aplican a los dos, cliente y servidor para que los
        // * objectos se muevan igual.
        _rigidbody = GetComponent<Rigidbody>();
        _inputHandler = GetComponent<InputHandler>();
    }

    private void SubscribeToTimeManager(bool subscribe) {
        if (base.TimeManager == null)
            return;
        if (subscribe == _subscribed)
            return;
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
    private void OnDestroy() {
        SubscribeToTimeManager(false);
    }
    public override void OnStartClient() {
        base.OnStartClient();
        SubscribeToTimeManager(true);
    }
    public override void OnStartServer() {
        base.OnStartServer();
        SubscribeToTimeManager(true);
        _inputHandler.enabled = false;
    }
    private void OnTick() {
        if (base.IsOwner) {
            /* La reconciliacion debe pasar primero
             * Esto arregla la posicion de los clientes a la que debe ser en el servidor
             * Y cachea los inputs de los clientes
             * Cuando se usa la reconciliacion  los datos cliente deben ser default y marcar el servidor como falso
             * De esta forma se hace una reconciliacion del lado del cliente.
             */
            Reconciliation(default, false);
            /* Recopila información de como se mueve el objeto. Usado por cliente y servidor */
            MoveData data;
            GatherInputs(out data);
            Move(data, false);
        }
        if (base.IsServer) {
            /*
             * El server tiene que mover lo mismo que el cliente, esto ayuda a mantener el objeto en sincronizacion.
             * Pasalos valores por defecto como data y se marca servidor como true. El servidor automáticamente
             * sabe que data tiene que usar cuando asServer es true. Como cuando se llama desde el cliente y no quiere marcar
             * el booleano réplica
             */
            Move(default, true);
            /*
             * Como se muestra abajo la reconciliacion se envía usando PostTick porque quieres que la posicion de los objetos, rotacion etc,
             * se envíen antes de que las físicas se hayan simulado.
             * Si estás usando un método de movimiento que no usa fisicas, como el caracter controller o moviendo el transform directamente,
             * puedes opt-out usando el OnPostTick y enviando la reconciliación desde aquí.
             */
        }
    }
    /* OnPostTick se ejecuta desúes de haber simulado las físicas */
    private void OnPostTick() {
        /*
         * Construye la reconciliacion usando los datos actuales del objeto. Esto se envía al cliente
         * y el cliente se resetea usando estos valores. Es EXTREMADAMENE importante enviar cualquier cosa que pueda
         * afectar a movimiento, rotación y posición del objetol. Esto incluye y no está limitado por:
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
         * Si estas uasndo un asset que usa fisicas internas es bastante posible que necesitesenviar eso valores que afectan
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
    private void GatherInputs(out MoveData data) {
        data = default;
        if (_inputHandler.isActiveAndEnabled) {
            //if (!_inputHandler.IsMoving)
            //    return;
            float horizontal = _inputHandler.Horizontal;
            float vertical = _inputHandler.Vertical;

            data = new MoveData(horizontal, vertical);
        }
    }
    [Replicate]
    private void Move(MoveData data, bool asServer, bool replaying = false) {
        /* Se puede usar asServer para saber si el servidor o el cliente esta llamando a este método. */
        
        /* Replaying debe ser true como cliente y si los inputs son relicados.
         * Cuando se llama a Move, replaying es falso porque estás llamánolo manualmente.
         * Sin embargo, cuando el cliente se reconcilia con el servidor, los inputs cacheados son automaticamente relicados
         * Un buen ejemplo es como usar el booleano de réplica sería para mostrar efectos especiales al saltar.
         *
         * Cuando replaying es falso estas llamando al método desde tu código (cliente), si replaying es verdadero
         * estás recibiendo la información de otro cliente. Los inputs son automáticamente replicados, por lo que el mismo input de salto puede ser
         * llamado multiples veces, y por eso replaying será verdadero. Puedes filtrar audio/vfx para que no se reproduzcan si replaying es verdadero.
         */
        Vector3 force = new Vector3(data.Horizontal, 0f, data.Vertical) * moveRate;
        _rigidbody.AddForce(force);
    }
    [Reconcile]
    private void Reconciliation(ReconcileData data, bool asServer) {
        transform.position = data.Position;
        transform.rotation = data.Rotation;
        _rigidbody.velocity = data.Velocity;
        _rigidbody.angularVelocity = data.AngularVelocity;
    }

}
