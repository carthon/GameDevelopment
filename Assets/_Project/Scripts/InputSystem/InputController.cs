using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.InputSystem {
    [RequireComponent(typeof(PlayerInput))]
    public class InputController : MonoBehaviour {
        private Locomotion _locomotion;
        private PlayerInput _playerInput;
        private void Awake() {
            _playerInput = GetComponent<PlayerInput>();
            _locomotion = GetComponent<Locomotion>();
        }
        private void Update() {
            _locomotion.SetDirection(Vector2.ClampMagnitude(_playerInput.move, 1f));
            _locomotion.TriggerJump(_playerInput.jump);
        }
    }
}
