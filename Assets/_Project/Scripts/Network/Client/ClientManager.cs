using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.Client {
    public class ClientManager : MonoBehaviour {
        private PlayerNetworkManager _player;
        
        private void LateUpdate() {
            if (_player.IsLocal) {
                SendInputs();
            }
        }
        private void SendInputs() {
            InputHandler inputHandler = InputHandler.Singleton;
            Vector3 moveInput = new Vector3(inputHandler.Horizontal, 0, inputHandler.Vertical);
            bool[] actions = new[] {
                inputHandler.IsMoving,
                inputHandler.IsJumping,
                inputHandler.IsSprinting,
                inputHandler.IsPicking
            };
            InputMessageStruct inputData = new InputMessageStruct(moveInput, actions, CameraHandler.Singleton.CameraPivot.rotation,NetworkManager.ClientTick);
            NetworkMessage networkMessage = new NetworkMessage(MessageSendMode.reliable, (ushort)NetworkManager.ClientToServerId.serverInput, inputData);
            networkMessage.Send(true);
        }
    }
}