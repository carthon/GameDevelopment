using System.IO;
using System.Linq;
using System.Net.Sockets;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct InputMessageStruct : IGenericMessageStruct {
        public Vector3 moveInput;
        public Quaternion headPivotRotation;
        public ulong actions;
        public int tick;
        public InputMessageStruct(Vector3 moveInput, Quaternion headPivotRotation, int tick, ulong actions) {
            this.moveInput = moveInput;
            this.headPivotRotation = headPivotRotation;
            this.actions = actions;
            this.tick = tick;
        }
        public InputMessageStruct(int tick) {
            moveInput = Vector3.zero;
            headPivotRotation = Quaternion.identity;
            actions = 0;
            this.tick = tick;
        }
        public InputMessageStruct(Message message) {
            moveInput = message.GetVector3();
            headPivotRotation = message.GetQuaternion();
            actions = message.GetULong();
            tick = message.GetInt();
        }
        public void Serialize(Message message) {
            message.AddVector3(moveInput).AddQuaternion(headPivotRotation)
                .AddULong(actions).AddInt(tick);
        }
        public override string ToString() {
            return $"{moveInput.ToString()} | tick {tick} | headPivot {headPivotRotation.ToString()}";
        }
    }
}