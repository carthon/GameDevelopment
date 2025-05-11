using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Project.Scripts.DataClasses.ItemTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor {
    public class ExtendedEditorWindow : EditorWindow {
        protected SerializedObject serializedObject;
        protected SerializedProperty currentProperty;

        protected void DrawProperties(SerializedProperty prop, bool drawChildren) {
            string lastPropPath = string.Empty;
            foreach (SerializedProperty p in prop) {
                if (p.isArray && p.propertyType == SerializedPropertyType.Generic) {
                    EditorGUILayout.BeginHorizontal();
                    p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                    EditorGUILayout.EndHorizontal();
                    if (p.isExpanded) {
                        EditorGUI.indentLevel++;
                        DrawProperties(p, drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else {
                    if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath)) continue;
                    lastPropPath = p.propertyPath;
                    ShowTypeProperties(p);
                }
            }
        }
        public void ShowTypeProperties(SerializedProperty property) {
            Object obj = property.objectReferenceValue;
            EditorGUILayout.BeginHorizontal();
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, obj.name);
            EditorGUILayout.PropertyField(property, true);
            EditorGUILayout.EndHorizontal();
            if (property.isExpanded) {
                GUILayout.BeginVertical("Box");
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(obj.name, EditorStyles.boldLabel);

                var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (var field in fields) {
                    switch (field.GetValue(obj)) {
                        case string casted:
                            field.SetValue(obj, EditorGUILayout.TextField(field.Name, casted));
                            break;
                        case int casted:
                            field.SetValue(obj, EditorGUILayout.IntField(field.Name, casted));
                            break;
                        case float casted:
                            field.SetValue(obj, EditorGUILayout.FloatField(field.Name, casted));
                            break;
                        case bool casted:
                            field.SetValue(obj, EditorGUILayout.Toggle(field.Name, casted));
                            break;
                        case GameObject casted:
                            SerializedObject serialized = new SerializedObject(casted);
                            EditorGUILayout.PropertyField(serialized.FindProperty("m_Name"), true);
                            break;
                        case LayerMask casted:
                            int layerMaskValue = casted.value;
                            layerMaskValue = EditorGUILayout.MaskField(field.Name, layerMaskValue, UnityEditorInternal.InternalEditorUtility.layers);
                            casted.value = layerMaskValue;
                            field.SetValue(obj, casted);
                            break;
                        default:
                            try {
                                field.SetValue(obj, EditorGUILayout.TextField(field.Name, field.GetValue(obj).ToString()));
                            } catch (Exception e) { Debug.LogWarning(e.Message); }
                            break;
                        
                    }
                }
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
        }
    }
}