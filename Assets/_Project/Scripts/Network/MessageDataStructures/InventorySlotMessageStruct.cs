using _Project.Scripts.DataClasses;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct InventorySlotMessageStruct : IGenericMessageStruct {
        public ushort ownerId;
        public int inventoryId;
        public InventorySlot itemSlot;
        
        public InventorySlotMessageStruct(ushort ownerId, int inventoryId, InventorySlot itemSlot) {
            this.ownerId = ownerId;
            this.inventoryId = inventoryId;
            this.itemSlot = itemSlot;
        }
        public InventorySlotMessageStruct(Message message) {
            ownerId = message.GetUShort();
            inventoryId = message.GetInt();
            itemSlot = message.GetInventorySlot();
        }
        public void Serialize(Message message) {
            message.AddUShort(ownerId);
            message.AddInt(inventoryId);
            message.AddInventorySlot(itemSlot);
        }
    }
}