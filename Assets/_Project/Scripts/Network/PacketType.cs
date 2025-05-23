namespace _Project.Scripts.Network {
    public enum PacketType : ushort {
        serverUsername = 1,
        serverInput,
        serverItemSwap,
        serverItemDrop,
        serverItemPick,
        serverItemEquip,
        serverUpdateClient,
        clientSpawnMessage,
        clientMovementMessage,
        clientPlayerDespawn,
        clientItemDespawn,
        clientItemSpawn,
        serverInventoryChange,
        clientReceiveEquipment,
        clientReceivePlayerData,
        clientGrabbablesPosition,
        clientItemSlotChange
    }
}