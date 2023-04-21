using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [CreateAssetMenu]
    public class GameData : ScriptableObject {
        public Item[] items = null;
        public ScriptableObject[] stats = null;
        public ScriptableObject[] actions = null;
        private static GameData _singleton;
        public static GameData Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(GameData)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }
        private void OnValidate() {
            _singleton = this;
            LoadAssets();
        }
        public void LoadAssets() {
            items = Resources.LoadAll<Item>("Scriptables");
            stats = Resources.LoadAll<ScriptableObject>("Scriptables/Stats");
            actions = Resources.LoadAll<ScriptableObject>("Scriptables/Actions");
        }
    }
}