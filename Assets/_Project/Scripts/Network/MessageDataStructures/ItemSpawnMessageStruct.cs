using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct ItemSpawnMessageStruct : IGenericMessageStruct{
    public ushort playerId;
    public int inventoryId;
    public Vector2Int inventorySlot;
    public void Serialize(Message message) {
        message.AddUShort(playerId);
        message.AddInt(inventoryId);
        message.AddVector2Int(inventorySlot);
    }
    }
}