using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public class ItemDespawnMessageStruct : IGenericMessageStruct {
        public readonly ushort ItemId;
        public ItemDespawnMessageStruct(ushort playerId) {
            this.ItemId = playerId;
        }
        public ItemDespawnMessageStruct(Message message) {
            this.ItemId = message.GetUShort();
        }
        public void Serialize(Message message) {
            message.AddUShort(ItemId);
        }
    }
}