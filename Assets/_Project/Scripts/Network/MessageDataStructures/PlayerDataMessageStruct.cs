using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct PlayerDataMessageStruct : IGenericMessageStruct {
        public ushort clientId;
        public int equipmentDataCount;
        public List<EquipmentMessageStruct> equipmentData;
        public PlayerDataMessageStruct(List<EquipmentMessageStruct> equipmentData, ushort clientId = 0 ) {
            this.clientId = clientId;
            this.equipmentDataCount = equipmentData.Count;
            this.equipmentData = equipmentData;
        }
        public PlayerDataMessageStruct(Message message) {
            clientId = message.GetUShort();
            equipmentDataCount = message.GetInt();
            equipmentData = new List<EquipmentMessageStruct>();
            for (int i = 0; i < equipmentDataCount; i++) {
                equipmentData.Add(new EquipmentMessageStruct(message));
            }
        }
    }
}