namespace _Project.Scripts.Network.Client {
    public partial class Client {
        public enum PacketHandler : ushort {
            serverUsername = 1,
            serverInput,
            serverItemSwap,
            serverItemDrop,
            serverItemEquip,
            serverUpdateClient
        }
    }
}