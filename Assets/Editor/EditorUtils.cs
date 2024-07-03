using UnityEditor;
using UnityEngine;

namespace Editor {
    public class EditorUtils {
        public static T FindScriptableObject<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"No ScriptableObject of type {typeof(T).Name} found in assets.");
                return null;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            T scriptableObject = AssetDatabase.LoadAssetAtPath<T>(path);
            return scriptableObject;
        }
    }
}