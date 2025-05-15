using RiptideNetworking;

namespace _Project.Scripts.Network.MessageDataStructures {
    public struct PlayerConnectionMessageStruct : IGenericMessageStruct {
        public string username;
        public PlayerConnectionMessageStruct(string username) {
            this.username = username;
        }
        public PlayerConnectionMessageStruct(Message message) {
            username = message.GetString();
        }
        public void Serialize(Message message) {
            message.AddString(username);
        }
    }
}