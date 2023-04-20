using _Project.Scripts.DataClasses;
using UnityEditor;

namespace Editor {
    public class GameDataEditorWindow : ExtendedEditorWindow {
        private GameData _gameData;
        public static void Open(GameData data) {
            GameDataEditorWindow window = GetWindow<GameDataEditorWindow>("Game data Editor");
            window.serializedObject = new SerializedObject(data);
        }
        
        public void OnGUI() {
            string[] properties = {"items", "stats"};
            foreach (string property in properties) {
                currentProperty = serializedObject.FindProperty(property);
                DrawCurrentProperty();
            }
        }
        private void DrawCurrentProperty(){
            EditorGUILayout.BeginHorizontal();
            currentProperty.isExpanded = EditorGUILayout.Foldout(currentProperty.isExpanded, currentProperty.displayName);
            EditorGUILayout.EndHorizontal();
            if (currentProperty.isExpanded) {
                EditorGUI.indentLevel++;
                DrawProperties(currentProperty, true);
                EditorGUI.indentLevel--;
            }
        }
    }
}