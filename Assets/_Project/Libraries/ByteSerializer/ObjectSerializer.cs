using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace _Project.Libraries.ByteSerializer {
    public class ObjectSerializer {
        public static void SerializeObject<T>(string filename, T objectToSerialize) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(filename, FileMode.Create);
            
            formatter.Serialize(stream, objectToSerialize);
            stream.Close();
        }
        public static T DeserializeObject<T>(string filename) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(filename, FileMode.Open);

            T objectToDeserialize = (T) formatter.Deserialize(stream);
            stream.Close();
            return objectToDeserialize;
        }
        public static byte[] SerializeObject<T>(T objectToSerialize) {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream()) {
                formatter.Serialize(stream, objectToSerialize);
                return stream.ToArray();
            }
        }
        public static T DeserializeObject<T>(byte[] byteArray) {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(byteArray)) {
                return (T)formatter.Deserialize(memoryStream);
            }
        }
    }
}