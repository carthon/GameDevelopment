using _Project.Scripts.Handlers;

namespace _Project.Scripts.Utils {
    public static class LocomotionUtils {
        public static bool IsMoving(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.Moving) != 0;
        public static bool IsJumping(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.Jumping) != 0;
        public static bool IsSprinting(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.Sprinting) != 0;
        public static bool IsCrouching(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.Crouching) != 0;
        public static bool IsDoubleJumping(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.DoubleJumping) != 0;
        public static bool IsSearching(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.Searching) != 0;
        public static bool IsAttacking(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.Attacking) != 0;
        public static bool IsInInventory(ulong actions) => (actions & (ulong) InputHandler.PlayerActions.InInventory) != 0;
    }
}