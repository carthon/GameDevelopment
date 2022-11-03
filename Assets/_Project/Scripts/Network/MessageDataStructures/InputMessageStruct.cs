using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct InputMessageStruct : IGenericMessageStruct {
        public Vector3 moveInput;
        public bool[] actions;
        public int clientTick;
        public Quaternion cameraPivotRotation;
        public InputMessageStruct(Vector3 moveInput, bool[] actions, Quaternion cameraPivotRotation,int clientTick) {
            this.moveInput = moveInput;
            this.actions = actions;
            this.clientTick = clientTick;
            this.cameraPivotRotation = cameraPivotRotation;
        }
        public InputMessageStruct(Message message) {
            this.moveInput = message.GetVector3();
            this.actions = message.GetBools();
            this.clientTick = message.GetInt();
            this.cameraPivotRotation = message.GetQuaternion();
        }
    }
}