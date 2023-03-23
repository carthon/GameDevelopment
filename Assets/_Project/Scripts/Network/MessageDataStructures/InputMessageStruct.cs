using System.Linq;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct InputMessageStruct : IGenericMessageStruct {
        public Vector3 moveInput;
        public Quaternion headPivotRotation;
        public bool[] actions;
        public int tick;
        public InputMessageStruct(Vector3 moveInput, Quaternion headPivotRotation, int tick, bool[] actions) {
            this.moveInput = moveInput;
            this.headPivotRotation = headPivotRotation;
            this.actions = actions;
            this.tick = tick;
        }
        public InputMessageStruct(Message message) {
            this.moveInput = message.GetVector3();
            this.headPivotRotation = message.GetQuaternion();
            this.actions = message.GetBools();
            this.tick = message.GetInt();
        }
        public override string ToString() {
            return $"{moveInput.ToString()} | tick {tick}";
        }
    }
}