using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Utils {
    public class InputRingBuffer {
        private readonly LocomotionInputMessage[] buffer;
        private int head, tail, count;
        public InputRingBuffer(int size) {
            buffer = new LocomotionInputMessage[size];
            head = tail = count = 0;
        }
        public int Count { get => count; }
        public bool Enqueue(LocomotionInputMessage msg) {
            if (count == buffer.Length) return false; // buffer full
            buffer[head] = msg;
            head = (head + 1) % buffer.Length;
            count++;
            return true;
        }
        public bool Peek(out LocomotionInputMessage msg) {
            if (count > 0) {
                msg = buffer[tail];
                return true;
            }
            msg = new LocomotionInputMessage();
            return false;
        }
        public bool Tail(out LocomotionInputMessage msg) {
            if (count > 0) {
                msg = buffer[tail];
                return true;
            }
            msg = new LocomotionInputMessage();
            return false;
        }
        public bool Dequeue(out LocomotionInputMessage msg) {
            msg = new LocomotionInputMessage();
            if (count <= 0)
                return false;
            msg = buffer[tail];
            tail = (tail + 1) % buffer.Length;
            count--;
            return true;
        }
        public void Clear() {
            
        }
    }
}