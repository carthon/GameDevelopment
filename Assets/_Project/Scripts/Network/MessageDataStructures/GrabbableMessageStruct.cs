using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct GrabbableMessageStruct : IGenericMessageStruct {
        public ushort grabbableId;
        public string itemId;
        public Vector3 position;
        public Quaternion rotation;
        
        public GrabbableMessageStruct(ushort grabbableId, string itemId, Vector3 position, Quaternion rotation) {
            this.grabbableId = grabbableId;
            this.itemId = itemId;
            this.position = position;
            this.rotation = rotation;
        }
        public GrabbableMessageStruct(Message message) {
            grabbableId = message.GetUShort();
            itemId = message.GetString();
            position = message.GetVector3();
            rotation = message.GetQuaternion();
        }
    }
}