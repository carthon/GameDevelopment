using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent {
    public struct SolverData {
        public struct LocomotionParams {
        public readonly Vector3 CurrentPosition;       // posición actual (mundo)
        public readonly Vector3 CurrentVelocity;       // velocidad en tick previo
        public readonly Vector2 InputMoveDir2D;        // dirección de input (X,Z), ya normalizada
        public readonly bool    IsGrounded;            // flag de si el personaje pisa suelo
        public readonly bool    IsJumping;             // flag de salto (primario)
        public readonly bool    IsDoubleJumping;       // flag de doble salto (u otro trigger)
        public readonly bool    IsCrouching;           // flag de agacharse
        public readonly bool    IsSprinting;           // flag de sprint
        public readonly bool    IsFlying;              // flag de vuelo (si el estado lo permite)
        public readonly float   MovementSpeed;         // velocidad lineal (según estado: correr, caminar, volar…)
        public readonly float   JumpStrength;          // fuerza de salto inicial
        public readonly float   GravityStrength;       // magnitud de gravedad
        public readonly Vector3 GravityUp;             // vector apuntando desde el centro gravitatorio
        public readonly float   Delta;                 // Time.fixedDeltaTime

        public LocomotionParams(
            Vector3 currentPosition,
            Vector3 currentVelocity,
            Vector2 inputMoveDir2D,
            bool isGrounded,
            bool isJumping,
            bool isDoubleJumping,
            bool isCrouching,
            bool isSprinting,
            bool isFlying,
            float movementSpeed,
            float jumpStrength,
            float gravityStrength,
            Vector3 gravityUp,
            float delta
        ) {
            CurrentPosition   = currentPosition;
            CurrentVelocity   = currentVelocity;
            InputMoveDir2D    = inputMoveDir2D;
            IsGrounded        = isGrounded;
            IsJumping         = isJumping;
            IsDoubleJumping   = isDoubleJumping;
            IsCrouching       = isCrouching;
            IsSprinting       = isSprinting;
            IsFlying          = isFlying;
            MovementSpeed     = movementSpeed;
            JumpStrength      = jumpStrength;
            GravityStrength   = gravityStrength;
            GravityUp         = gravityUp;
            Delta             = delta;
        }
    }

    // ------------------------------------------------------------------
    // 2. Struct con el resultado del solver
    // ------------------------------------------------------------------
    public struct LocomotionResult {
        public readonly Vector3 NewVelocity;       // velocidad tras aplicar sprint/salto/gravedad
        public readonly Vector3 PositionOffset;    // desplazamiento a aplicar (velocidad · delta)
        public readonly Quaternion NewRotation;    // rotación deseada (alineación con GravityUp, etc.)

        public LocomotionResult(Vector3 newVelocity, Vector3 positionOffset, Quaternion newRotation) {
            NewVelocity    = newVelocity;
            PositionOffset = positionOffset;
            NewRotation    = newRotation;
        }
    }
    }
}