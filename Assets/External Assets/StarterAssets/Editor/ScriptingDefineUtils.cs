using UnityEditor;

namespace StarterAssets {
    public static class ScriptingDefineUtils {
        public static bool CheckScriptingDefine(string scriptingDefine) {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Contains(scriptingDefine);
        }

        public static void SetScriptingDefine(string scriptingDefine) {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!defines.Contains(scriptingDefine)) {
                defines += $";{scriptingDefine}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            }
        }

        public static void RemoveScriptingDefine(string scriptingDefine) {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (defines.Contains(scriptingDefine)) {
                var newDefines = defines.Replace(scriptingDefine, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
            }
        }
    }
}