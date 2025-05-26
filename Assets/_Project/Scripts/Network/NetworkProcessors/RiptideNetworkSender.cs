using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;

namespace _Project.Scripts.Network {
    public class RiptideNetworkSender : INetworkSender {
        private readonly NetworkManager _networkManager;
        public RiptideNetworkSender(NetworkManager networkManager) {
            _networkManager = networkManager;
        }
        public void SendToClients<T>(MessageSendMode mode, ushort messageId, in T data, ushort toClientId = 0) 
            where T : struct, IGenericMessageStruct {
            if (!_networkManager.IsServer) return;
            var msg = Message.Create(mode, messageId);
            data.Serialize(msg);
            _networkManager.MessagesSent++;
            if(toClientId > 0)
                _networkManager.ServerHandler.Send(msg, toClientId);
            else    
                _networkManager.ServerHandler.SendToAll(msg);
        }
        public void SendToServer<T>(MessageSendMode mode, ushort messageId, in T data) 
            where T : struct, IGenericMessageStruct {
            if (!_networkManager.IsClient) return;
            var msg = Message.Create(mode, messageId);
            data.Serialize(msg);
            _networkManager.MessagesSent++;
            _networkManager.ClientHandler.Send(msg);
        }
    }
}