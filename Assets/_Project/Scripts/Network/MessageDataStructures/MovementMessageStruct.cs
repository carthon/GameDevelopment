using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct MovementMessageStruct : IGenericMessageStruct {
        public ushort id;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 relativeDirection;
        public Vector3 forwardDirection;
        public Quaternion modelRotation;
        public Quaternion headPivotRotation;
        public int tick;
        public ulong actions;
        public MovementMessageStruct(ushort id, Vector3 position, Vector3 velocity, Vector3 relativeDirection, Vector3 forwardDirection, Quaternion modelRotation, 
            Quaternion headPivotRotation, int tick, ulong actions) {
            this.id = id;
            this.position = position;
            this.velocity = velocity;
            this.relativeDirection = relativeDirection;
            this.forwardDirection = forwardDirection;
            this.modelRotation = modelRotation;
            this.headPivotRotation = headPivotRotation;
            this.tick = tick;
            this.actions = actions;
        }
        
        public MovementMessageStruct(Message message) {
            id = message.GetUShort();
            position = message.GetVector3();
            velocity = message.GetVector3();
            relativeDirection = message.GetVector3();
            forwardDirection = message.GetVector3();
            modelRotation = message.GetQuaternion();
            headPivotRotation = message.GetQuaternion();
            tick = message.GetInt();
            actions = message.GetULong();
        }
        public void Serialize(Message message) {
            message.AddUShort(id).AddVector3(position).AddVector3(velocity)
                .AddVector3(relativeDirection).AddVector3(forwardDirection).AddQuaternion(modelRotation).AddQuaternion(headPivotRotation)
                .AddInt(tick).AddULong(actions);
        }
        
        public override string ToString() {
            return $"ClientId:{id}, " +
                $"Position:{position.ToString()}, " +
                $"Rotation: {modelRotation}" +
                $"relativeDirection: {relativeDirection}" +
                $"forwardDirection: {forwardDirection}" +
                $"velocity: {velocity}" +
                $"Tick:{tick}";
        }
    }
}