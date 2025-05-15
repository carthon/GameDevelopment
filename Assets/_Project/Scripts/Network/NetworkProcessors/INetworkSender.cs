using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;

namespace _Project.Scripts.Network {
    public interface INetworkSender {
        /// <summary>Envía un mensaje genérico a un cliente o a todos.</summary>
        /// <param name="mode">Reliable o Unreliable (UDP).</param>
        /// <param name="messageId">ID de paquete (PacketHandler).</param>
        /// <param name="data">Payload que implementa IGenericMessageStruct.</param>
        /// <param name="toClientId">0 = todos los clientes; >0 un cliente concreto.</param>
        void Send(MessageSendMode mode, ushort messageId, IGenericMessageStruct data, ushort toClientId = 0);
    }
}