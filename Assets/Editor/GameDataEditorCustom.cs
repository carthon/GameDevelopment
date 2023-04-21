using _Project.Scripts.DataClasses;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Editor {
    public static class AssetHandler {
        [OnOpenAsset()]
        public static bool OpenEditor(int instanceId, int line) {
            GameData data = EditorUtility.InstanceIDToObject(instanceId) as GameData;
            if (data != null) {
                data.LoadAssets();
                GameDataEditorWindow.Open(data);
                return true;
            }
            return false;
        }
    }
    [CustomEditor(typeof(GameData))]
    public class GameDataEditorCustom : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Open Editor")) {
                GameDataEditorWindow.Open((GameData) target);
            }
        }
    }
}