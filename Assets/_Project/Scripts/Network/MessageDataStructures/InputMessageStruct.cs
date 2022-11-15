using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct InputMessageStruct : IGenericMessageStruct {
        public Vector3 moveInput;
        public bool[] actions;
        public int clientTick;
        public Quaternion headPivotRotation;
        public InputMessageStruct(Vector3 moveInput, bool[] actions, Quaternion headPivotRotation,int clientTick) {
            this.moveInput = moveInput;
            this.actions = actions;
            this.clientTick = clientTick;
            this.headPivotRotation = headPivotRotation;
        }
        public InputMessageStruct(Message message) {
            this.moveInput = message.GetVector3();
            this.actions = message.GetBools();
            this.clientTick = message.GetInt();
            this.headPivotRotation = message.GetQuaternion();
        }
    }
}