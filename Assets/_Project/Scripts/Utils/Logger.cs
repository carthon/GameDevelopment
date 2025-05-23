using System;
using System.IO;
using System.Text;
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
        private string filePath = Application.dataPath + @"\Logs\";

        private Logger(NetworkManager networkManager) {
            _networkManager = networkManager;
        }
        public static void Initialize(NetworkManager networkManager) {
            _singleton = new Logger(networkManager);
        }
        
        public void Log(string message, Type type) {
            bool isClient = _networkManager.IsClient;
            bool isServer = _networkManager.IsServer;
            string pathServer = _networkManager.IsServer ? "SERVER" : _networkManager.IsClient ? "CLIENT" : "";
            string path = filePath + $"Log{pathServer}-{DateTime.Now:yyyy'-'MM'-'dd}.txt";
            // Determine whether the directory exists.
            if (!Directory.Exists(filePath)){
                Debug.Log("Creating directory");
                Directory.CreateDirectory(filePath);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"{DateTime.Now:hh:mm:ss.fff}");
            if (isClient) sb.Append("[CLIENT]");
            if (isServer) sb.Append("[SERVER]");
            sb.Append($" {type.ToString()}: ");
            sb.Append(message + Environment.NewLine);
            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string createText = $"Logged in {path}" + Environment.NewLine;
                File.WriteAllText(path, createText);
            }
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
            File.AppendAllText(path, sb.ToString());
            Console.Write(sb.ToString());
            sb.Clear();
        }
    }
}