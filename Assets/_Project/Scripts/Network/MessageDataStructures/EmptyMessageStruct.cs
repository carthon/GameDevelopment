using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public class EmptyMessageStruct : IGenericMessageStruct {
        public EmptyMessageStruct() {
            
        }
        public void Serialize(Message message) { }
    }
}