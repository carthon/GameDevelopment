using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct PlayerDespawnMessageStruct : IGenericMessageStruct {
        public ushort _playerId;
        public PlayerDespawnMessageStruct(ushort playerId) {
            _playerId = playerId;
        }
        public PlayerDespawnMessageStruct(Message message) {
            _playerId = message.GetUShort();
        }
        public void Serialize(Message message) {
            message.AddUShort(_playerId);
        }
    }
}