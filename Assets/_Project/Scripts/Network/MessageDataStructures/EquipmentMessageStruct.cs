using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct EquipmentMessageStruct : IGenericMessageStruct {
        public ushort clientId;
        public ItemStack itemStack;
        public int equipmentSlot;
        public bool activeState;
        public EquipmentMessageStruct(ItemStack itemStack, int equipmentSlot = 0, bool activeState = false, ushort clientId = 0) {
            this.clientId = clientId;
            this.itemStack = itemStack;
            this.equipmentSlot = equipmentSlot;
            this.activeState = activeState;
        }
        public EquipmentMessageStruct(Message message) {
            clientId = message.GetUShort();
            itemStack = message.GetItemStack();
            equipmentSlot = message.GetInt();
            activeState = message.GetBool();
        }
    }
}