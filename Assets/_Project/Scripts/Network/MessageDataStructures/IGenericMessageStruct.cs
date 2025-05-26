using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public interface IGenericMessageStruct {
        public void Serialize(Message message);
    }
}