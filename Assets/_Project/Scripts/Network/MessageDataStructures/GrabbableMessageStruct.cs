using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct GrabbableMessageStruct : IGenericMessageStruct {
        public ushort grabbableId;
        public int count;
        public ItemStack itemStack;
        public Vector3 position;
        public Quaternion rotation;
        
        public GrabbableMessageStruct(ushort grabbableId, ItemStack itemStack, Vector3 position, Quaternion rotation) {
            this.grabbableId = grabbableId;
            this.itemStack = itemStack;
            this.position = position;
            this.rotation = rotation;
            count = 1;
        }
        public GrabbableMessageStruct(Grabbable grabbable) {
            this.grabbableId = grabbable.Id;
            this.itemStack = grabbable.GetItemStack();
            Transform transform = grabbable.transform;
            this.position = transform.position;
            this.rotation = transform.rotation;
            count = 1;
        }
        public GrabbableMessageStruct(Message message) {
            grabbableId = message.GetUShort();
            itemStack = message.GetItemStack();
            position = message.GetVector3();
            rotation = message.GetQuaternion();
            count = message.GetInt();
        }
        public void Serialize(Message message) {
            message.AddUShort(grabbableId).AddItemStack(itemStack).AddVector3(position).AddQuaternion(rotation).AddInt(count);
        }
    }
}