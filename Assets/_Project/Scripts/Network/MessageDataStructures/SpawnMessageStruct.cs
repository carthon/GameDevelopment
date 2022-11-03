using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct SpawnMessageStruct : IGenericMessageStruct {
        
        public ushort id;
        public string username;
        public Vector3 position;
        
        public SpawnMessageStruct(ushort id, string username, Vector3 position) {
            this.id = id;
            this.username = username;
            this.position = position;
        }
        public SpawnMessageStruct(Message message) {
            this.id = message.GetUShort();
            this.username = message.GetString();
            this.position = message.GetVector3();
        }
    }
}