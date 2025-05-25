using _Project.Scripts.Components;
using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct GrabbablePickupMessageStruct : IGenericMessageStruct {
        public readonly ushort ItemId;
        public GrabbablePickupMessageStruct(ushort itemId) {
            ItemId = itemId;
        }
        public GrabbablePickupMessageStruct(Grabbable grabbable) {
            ItemId = grabbable.Id;
        }
        public GrabbablePickupMessageStruct(Message message) {
            ItemId = message.GetUShort();
        }
        public void Serialize(Message message) {
            message.AddUShort(ItemId);
        }
    }
}