using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Utils {
    public class NetworkUtils {
        
    }
    
    public class InputRingBuffer {
        private readonly InputMessageStruct[] buffer;
        private int head, tail, count;
        public InputRingBuffer(int size) {
            buffer = new InputMessageStruct[size];
            head = tail = count = 0;
        }
        public int Count { get => count; }
        public bool Enqueue(InputMessageStruct msg) {
            if (count == buffer.Length) return false; // buffer full
            buffer[tail] = msg;
            tail = (tail + 1) % buffer.Length;
            count++;
            return true;
        }
        public bool Peek(out InputMessageStruct msg) {
            if (count > 0) {
                msg = buffer[head];
                return true;
            }
            msg = new InputMessageStruct();
            return false;
        }
        public bool Dequeue(out InputMessageStruct msg) {
            msg = new InputMessageStruct();
            if (count <= 0)
                return false;
            msg = buffer[head];
            if (tail > 0)
                tail--;
            count--;
            return true;
        }
        public void Clear() {
            
        }
    }

}