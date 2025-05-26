using System.Runtime.CompilerServices;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct PlayerSpawnMessageStruct : IGenericMessageStruct {
        
        public ushort id;
        public string entityId;
        public Vector3 position;
        public Quaternion rotation;
        public int tick;
        
        public PlayerSpawnMessageStruct(ushort id, string entityId, Vector3 position, Quaternion rotation, int tick) {
            this.id = id;
            this.entityId = entityId;
            this.position = position;
            this.rotation = rotation;
            this.tick = tick;
        }
        public PlayerSpawnMessageStruct(Message message) {
            this.id = message.GetUShort();
            this.entityId = message.GetString();
            this.position = message.GetVector3();
            this.rotation = message.GetQuaternion();
            this.tick = message.GetInt();
        }
        public void Serialize(Message message) {
            message.AddUShort(id).AddString(entityId).AddVector3(position).AddQuaternion(rotation).AddInt(tick);
        }
    }
}