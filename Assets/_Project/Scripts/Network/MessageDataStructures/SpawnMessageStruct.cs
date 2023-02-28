using System.Runtime.CompilerServices;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct SpawnMessageStruct : IGenericMessageStruct {
        
        public ushort id;
        public string entityId;
        public Vector3 position;
        public Quaternion rotation;
        public int tick;
        
        public SpawnMessageStruct(ushort id, string entityId, Vector3 position, Quaternion rotation, int tick) {
            this.id = id;
            this.entityId = entityId;
            this.position = position;
            this.rotation = rotation;
            this.tick = tick;
        }
        public SpawnMessageStruct(Message message) {
            this.id = message.GetUShort();
            this.entityId = message.GetString();
            this.position = message.GetVector3();
            this.rotation = message.GetQuaternion();
            this.tick = message.GetInt();
        }
    }
}