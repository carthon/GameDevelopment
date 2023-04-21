using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using _Project.Scripts.DataClasses;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Editor {
    public class GameDataEditorWindow : ExtendedEditorWindow {
        private GameData _gameData;
        private FieldInfo[] properties;
        private Dictionary<string,Type> availableScriptableClasses;
        private string newAssetTitle = "Name";
        private bool showTooltip;
        public static void Open(GameData data) {
            GameDataEditorWindow window = GetWindow<GameDataEditorWindow>("Game data Editor");
            window._gameData = data;
            window.OnLoad();
        }
        public static void CreateAsset(Type type, string title, string assetPath) {
            ScriptableObject asset = CreateInstance(type);
            if (assetPath != "")
            {
                AssetDatabase.CreateAsset(asset, Path.Combine(assetPath, title + ".asset"));
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
        }
        public static List<Type> GetDerivedTypes(Type baseType)
        {
            Assembly assembly = Assembly.GetAssembly(baseType);
            return assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
        }
        private void OnLoad() {
            properties = typeof(GameData).GetFields();
            availableScriptableClasses = new Dictionary<string, Type>();
            foreach (var obj in Resources.LoadAll<ScriptableObject>("Scriptables")) {
                if (!availableScriptableClasses.ContainsValue(obj.GetType()))
                    availableScriptableClasses.Add(AssetDatabase.GetAssetPath(obj), obj.GetType());
            }
            serializedObject = new SerializedObject(_gameData);
        }
        private static Assembly CompileFile(CodeDomProvider provider, CompilerParameters parameters, string file) {
            CompilerResults results = provider.CompileAssemblyFromFile(parameters, file);

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Debug.LogError(error.ErrorText);
                }
                return null;
            }
            Assembly assembly = Assembly.LoadFrom(results.PathToAssembly);
            return assembly;
        }
        private static Assembly[] CompileDirectory(string directoryPath)
        {
            List<Assembly> assemblies = new List<Assembly>();

            // Crea un proveedor de compilación de C# (CSharpCodeProvider)
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            // Opciones de compilación
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false; // Compilar una biblioteca de clases
            parameters.GenerateInMemory = false; // No generar el ensamblado en memoria
            parameters.IncludeDebugInformation = false; // No incluir información de depuración
            parameters.TreatWarningsAsErrors = false; // No tratar las advertencias como errores
            parameters.CompilerOptions = "/optimize"; // Opciones adicionales del compilador (opcional)

            // Agrega todas las referencias necesarias al compilador
            string absolutePath = "B:\\Archivos de Programas\\Unity\\2020.3.40f1\\Editor\\Data\\Managed\\";
            parameters.ReferencedAssemblies.Add("B:\\Archivos de Programas\\Unity\\2020.3.40f1\\Editor\\Data\\NetCore\\runtime-317-win-x64\\shared\\Microsoft.NETCore.App\\3.1.7\\" + "System.dll");
            parameters.ReferencedAssemblies.Add(absolutePath + "UnityEngine.dll");

            assemblies.AddRange(CompileDirectory(directoryPath, provider, parameters));
            return assemblies.ToArray();
        }
        private static Assembly[] CompileDirectory(string directory,CodeDomProvider provider, CompilerParameters parameters) {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string file in Directory.GetFiles(directory, "*.cs")) {
                Assembly assembly = CompileFile(provider, parameters, file);
                if(assembly != null) assemblies.Add(assembly);
            }
            foreach (string directoryPath in Directory.GetDirectories(directory)) {
                assemblies.AddRange(CompileDirectory(directoryPath, provider, parameters));
            }
            return assemblies.ToArray();
        }
        public static Type[] LoadClasses(Assembly[] assemblies)
        {
            List<Type> types = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] assemblyTypes = assembly.GetTypes();

                if (types != null)
                {
                    types.AddRange(assemblyTypes);
                }
            }

            return types.ToArray();
        }
        public void OnGUI() {
            newAssetTitle = EditorGUILayout.TextField(newAssetTitle);
            int i = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (string path in availableScriptableClasses.Keys) {
                Type type = availableScriptableClasses[path];
                string directory = Path.GetDirectoryName(path);
                string[] splitted = Regex.Split(type.ToString(), "\\.");
                if (GUILayout.Button(new GUIContent("Crear " + splitted[splitted.Length - 1], directory))) {
                    CreateAsset(type, newAssetTitle, directory);
                    _gameData.LoadAssets();
                    OnLoad();
                }
                i++;
            }
            EditorGUILayout.EndHorizontal();
            foreach (FieldInfo property in properties) {
                currentProperty = serializedObject.FindProperty(property.Name);
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