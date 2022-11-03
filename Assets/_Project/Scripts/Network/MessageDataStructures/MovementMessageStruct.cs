using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct MovementMessageStruct : IGenericMessageStruct {
        public ushort id;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 relativeDirection;
        public Quaternion rotation;
        public Quaternion cameraPivotRotation;
        public bool[] actions;
        public MovementMessageStruct(ushort id, Vector3 position, Vector3 velocity, Vector3 relativeDirection, Quaternion rotation, 
            Quaternion cameraPivotRotation, bool[] actions) {
            this.id = id;
            this.position = position;
            this.velocity = velocity;
            this.relativeDirection = relativeDirection;
            this.rotation = rotation;
            this.cameraPivotRotation = cameraPivotRotation;
            this.actions = actions;
        }
        
        public MovementMessageStruct(Message message) {
            this.id = message.GetUShort();
            this.position = message.GetVector3();
            this.velocity = message.GetVector3();
            this.relativeDirection = message.GetVector3();
            this.rotation = message.GetQuaternion();
            this.cameraPivotRotation = message.GetQuaternion();
            this.actions = message.GetBools();
        }
    }
}