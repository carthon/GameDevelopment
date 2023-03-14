using System;
using System.IO;
using System.Text;
using _Project.Scripts.Network;
using UnityEngine;

namespace _Project.Scripts.Utils {
    public class Logger {
        public enum Type {
            DEBUG = 0,
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

        private string filePath = Application.dataPath + @"\Logs\";
        
        public static void Initialize() {
            _singleton = new Logger();
        }
        
        public void Log(string message, Type type) {
            string path = filePath + $"Log-{DateTime.Now:yyyy'-'MM'-'dd}.txt";
            bool isClient = NetworkManager.Singleton.IsClient;
            bool isServer = NetworkManager.Singleton.IsServer;
            // Determine whether the directory exists.
            if (!Directory.Exists(filePath)){
                Debug.Log("Creating directory");
                Directory.CreateDirectory(filePath);
            }
            StringBuilder sb = new StringBuilder();
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