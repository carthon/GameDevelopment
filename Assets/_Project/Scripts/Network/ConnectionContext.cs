using System.IO;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using _Project.Scripts.Network.MessageDataStructures;
using UnityEngine;

namespace _Project.Scripts.Network {
    //TODO: Mejorar el sistema de colas y recepción de inputs
    public class ConnectionContext {
        public readonly ushort ClientId;
        public readonly Channel<InputMessageStruct> InputChannel;
        private readonly NetworkStream _stream;
        private bool _running = true;

        public ConnectionContext(ushort clientId, TcpClient tcpClient, int channelCapacity = 1024) {
            ClientId = clientId;
            _stream = tcpClient.GetStream();
            var options = new BoundedChannelOptions(channelCapacity) {
                SingleReader = true,
                SingleWriter = false,
                FullMode     = BoundedChannelFullMode.DropOldest
            };
            InputChannel = Channel.CreateBounded<InputMessageStruct>(options);

            // Arranca recepción de red
            _ = Task.Run(() => ReceiveLoopAsync());
        }

        private async Task ReceiveLoopAsync() {
            while (_running) {
                InputMessageStruct msg = await DeserializeAsync(_stream);
                await InputChannel.Writer.WriteAsync(msg);
            }
        }
        private async Task<InputMessageStruct> DeserializeAsync(NetworkStream stream)
        {
            // Ejemplo usando un buffer fijo y BinaryReader:
            var buffer = new byte[sizeof(ushort) + sizeof(ulong) + 3 * sizeof(int)];
            int read = 0;
            while (read < buffer.Length)
                read += await stream.ReadAsync(buffer, read, buffer.Length - read);

            using var ms = new MemoryStream(buffer);
            using var reader = new BinaryReader(ms);
            return new InputMessageStruct {
                tick       = reader.ReadUInt16(),
                actions    = reader.ReadUInt64(),
                moveInput   = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                )
            };
        }

        public bool TryGetInput(out InputMessageStruct msg)
            => InputChannel.Reader.TryRead(out msg);

        public void Stop() {
            _running = false;
            InputChannel.Writer.Complete();
        }
    }
}