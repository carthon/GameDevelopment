using _Project.Scripts.DataClasses;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Components.LocomotionComponent.Logic {
    public interface ILocomotionState
    {
        /// <summary>
        /// Se llama una única vez cuando entramos en este estado.
        /// </summary>
        void Enter(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats);

        /// <summary>
        /// Se llama cada tick de simulación (deltaTime es fijo).
        /// Actualiza state.Position, state.Velocity, y puede cambiar state.Mode si es necesario.
        /// </summary>
        void Tick(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats,
            float deltaTime);

        /// <summary>
        /// Devuelve un nuevo estado si hay que hacer cualquier transición, o null para permanecer.
        /// </summary>
        ILocomotionState CheckTransition(in LocomotionStateMessage state,
            in LocomotionInputMessage input);
    }
}