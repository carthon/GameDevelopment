using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;

namespace _Project.Scripts.Network {
    public class RiptideNetworkSender : INetworkSender {
        private NetworkManager _networkManager;
        public RiptideNetworkSender(NetworkManager networkManager) {
            _networkManager = networkManager;
        }
        
        public void Send(MessageSendMode mode, ushort messageId, IGenericMessageStruct data, ushort toClientId = 0) {
            var msg = Message.Create(mode, messageId);
            data.Serialize(msg);
            _networkManager.MessagesSent++;
            if (_networkManager.IsServer && !(_networkManager.ClientHandler?.IsConnected ?? false)) {
                if(toClientId > 0)
                    _networkManager.ServerHandler.Send(msg, toClientId);
                else    
                    _networkManager.ServerHandler.SendToAll(msg);
            } else if (_networkManager.IsClient && !_networkManager.IsServer)
                _networkManager.ClientHandler.Send(msg);
        }
    }
}