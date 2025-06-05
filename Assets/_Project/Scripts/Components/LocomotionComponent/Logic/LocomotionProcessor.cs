using System.Collections.Generic;
using _Project.Scripts.DataClasses;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Components.LocomotionComponent.Logic {
    public class LocomotionProcessor {
        private readonly Dictionary<LocomotionMode, ILocomotionState> _stateTable;
        private ILocomotionState _currentState;

        public LocomotionProcessor(LocomotionStateMessage initialState,
            LocomotionInputMessage initialInput,
            LocomotionStats stats) {
            // 1) Crear instancias de cada estado.
            _stateTable = new Dictionary<LocomotionMode, ILocomotionState> {
                { LocomotionMode.Grounded, new GroundedState() },
                { LocomotionMode.Airborne, new AirborneState() },
                { LocomotionMode.Fly, new FlyState() }
            };

            // 2) Configurar estado inicial basado en initialState.Mode
            _currentState = _stateTable[initialState.mode];
            _currentState.Enter(ref initialState, in initialInput, stats);
        }

        /// <summary>
        /// Ejecuta un tick determinista: comprueba transición y actualiza estado.
        /// </summary>
        public void Tick(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats,
            float deltaTime) {
            // 1) TRANSICIÓN DE ESTADO (si corresponde)
            var next = _currentState.CheckTransition(in state, in input);
            if (next != null && next != _currentState) {
                _currentState = next;
                _currentState.Enter(ref state, in input, stats);
            }

            // 2) EJECUTAR LÓGICA DEL ESTADO ACTUAL
            _currentState.Tick(ref state, in input, stats, deltaTime);

            // 3) ACTUALIZAR tick y acciones en el estado (para enviarlos a clientes)
            state.tick = input.tick;
            state.actions = input.actions;
        }
    }
}