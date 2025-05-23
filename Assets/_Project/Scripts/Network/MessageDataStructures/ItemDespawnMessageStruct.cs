using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct ItemDespawnMessageStruct : IGenericMessageStruct {
        public readonly ushort ItemId;
        public ItemDespawnMessageStruct(ushort playerId) {
            ItemId = playerId;
        }
        public ItemDespawnMessageStruct(Message message) {
            ItemId = message.GetUShort();
        }
        public void Serialize(Message message) {
            message.AddUShort(ItemId);
        }
    }
}