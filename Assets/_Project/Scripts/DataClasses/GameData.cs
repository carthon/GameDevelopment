using System.Collections.Generic;
using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [CreateAssetMenu]
    public class GameData : ScriptableObject {
        public Item[] items = null;
        public ScriptableObject[] stats = null;
        private void OnValidate() {
            LoadAssets();
        }
        public void LoadAssets() {
            items = Resources.LoadAll<Item>("Scriptables");
            stats = Resources.LoadAll<ScriptableObject>("Scriptables/Stats");
        }
    }
}