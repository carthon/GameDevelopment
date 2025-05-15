using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public class PlayerDespawnMessageStruct : IGenericMessageStruct {
        private ushort _playerId;
        public PlayerDespawnMessageStruct(ushort playerId) {
            this._playerId = playerId;
        }
        public PlayerDespawnMessageStruct(Message message) {
            this._playerId = message.GetUShort();
        }
        public void Serialize(Message message) {
            message.AddUShort(_playerId);
        }
    }
}