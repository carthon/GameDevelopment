using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using _Project.Scripts.Network;
using UnityEngine;

namespace _Project.Scripts.Utils {
    public class Logger {
        public enum Type {
            DEBUG = 0,
            INFO,
            WARNING,
            ERROR
        }
        private static Logger _singleton;
        public static Logger Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(Logger)} instance already exists, destroying duplicate!");
                }
            }
        }
        private NetworkManager _networkManager;
        public Type LogLevel = Type.INFO;
        private string filePath = Application.dataPath + @"\Logs\";
        private readonly BlockingCollection<(string path, byte[] data)> _logQueue;
        private readonly CancellationTokenSource _cts;
        [ThreadStatic]
        private static StringBuilder _threadStringBuilder;
        public static StringBuilder GetBuilder() {
            if (_threadStringBuilder == null)
                _threadStringBuilder = new StringBuilder(1024); // Capacidad Inicial
            else
                _threadStringBuilder.Clear();
            return _threadStringBuilder;
        }

        private Logger(NetworkManager networkManager) {
            _networkManager = networkManager;
            Directory.CreateDirectory(filePath);

            _logQueue = new BlockingCollection<(string path, byte[] data)>();
            _cts = new CancellationTokenSource();

            Task.Factory.StartNew(ProcessQueueAsync, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        public static void Initialize(NetworkManager networkManager) {
            _singleton = new Logger(networkManager);
        }

        ~Logger() {
            Shutdown();
        }
        
        public void Log(string message, Type type, bool logOnConsole = false) {
            if (type < LogLevel) return;
            bool isClient = _networkManager.IsClient;
            bool isServer = _networkManager.IsServer;
            string pathServer = _networkManager.IsServer ? "SERVER" : _networkManager.IsClient ? $"CLIENT[{_networkManager.ClientHandler.Id}]" : "";
            string path = filePath + $"Log{pathServer}-{DateTime.Now:yyyy'-'MM'-'dd}.txt";
            Encoding utf8 = Encoding.UTF8;
            StringBuilder sb = GetBuilder();
            sb.Append($"{DateTime.Now:hh:mm:ss.fff}");
            if (isClient) sb.Append($"[CLIENT-{_networkManager.ClientHandler.Id}]");
            if (isServer) sb.Append("[SERVER]");
            sb.Append($" {type.ToString()}: ");
            sb.Append(message + Environment.NewLine);
            if(logOnConsole) {
                switch (type) {
                    case Type.DEBUG:
                        Debug.Log(sb.ToString());
                        break;
                    case Type.WARNING:
                        Debug.LogWarning(sb.ToString());
                        break;
                    case Type.ERROR:
                        Debug.LogError(sb.ToString());
                        break;
                }
            }
            byte[] logBytes = utf8.GetBytes(sb.ToString());
            _logQueue.Add((path, logBytes));
        }
        private async Task ProcessQueueAsync() {
            foreach (var (path, data) in _logQueue.GetConsumingEnumerable(_cts.Token)) {
                try {
                    await using FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true);
                    await fs.WriteAsync(data, 0, data.Length, _cts.Token);
                } catch (Exception e) {
                    Debug.LogError($"Error writing log file: {e.Message}");
                }
            }
        }

        public void Shutdown() {
            _logQueue.CompleteAdding();
            _cts.Cancel();
        }
    }
}