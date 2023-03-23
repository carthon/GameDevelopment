using System;
using UnityEditor;
using UnityEngine;

namespace EditorAttributes {
    public class ScriptableObjectId : PropertyAttribute { }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableObjectId))]
    public class ScriptableObjectIdDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GUI.enabled = false;
            if (string.IsNullOrEmpty(property.stringValue)) {
                property.stringValue = Guid.NewGuid().ToString("N");
            }
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
}