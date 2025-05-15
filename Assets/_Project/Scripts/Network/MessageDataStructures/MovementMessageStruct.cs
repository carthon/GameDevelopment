using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct MovementMessageStruct : IGenericMessageStruct {
        public ushort id;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 relativeDirection;
        public Quaternion rotation;
        public Quaternion headPivotRotation;
        public int tick;
        public bool[] actions;
        public MovementMessageStruct(ushort id, Vector3 position, Vector3 velocity, Vector3 relativeDirection, Quaternion rotation, Quaternion headPivotRotation, int tick, bool[] actions) {
            this.id = id;
            this.position = position;
            this.velocity = velocity;
            this.relativeDirection = relativeDirection;
            this.rotation = rotation;
            this.headPivotRotation = headPivotRotation;
            this.tick = tick;
            this.actions = actions;
        }
        
        public MovementMessageStruct(Message message) {
            this.id = message.GetUShort();
            this.position = message.GetVector3();
            this.velocity = message.GetVector3();
            this.relativeDirection = message.GetVector3();
            this.rotation = message.GetQuaternion();
            this.headPivotRotation = message.GetQuaternion();
            this.tick = message.GetInt();
            this.actions = message.GetBools();
        }
        public void Serialize(Message message) {
            message.AddUShort(id).AddVector3(position).AddVector3(velocity)
                .AddVector3(relativeDirection).AddQuaternion(rotation).AddQuaternion(headPivotRotation)
                .AddInt(tick).AddBools(actions);
        }
        public override string ToString() {
            return $"ClientId:{id}, " +
                $"Position:{position.ToString()}, " +
                $"Tick:{tick}";
        }
    }
}