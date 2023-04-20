using _Project.Libraries.Marching_Cubes.Scripts;
using UnityEditor;
using UnityEngine;

namespace Editor {
    [CustomEditor(typeof(GenTest))]
    public class GenTestHelperEditor : UnityEditor.Editor {
        private GenTest _gentest;
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            _gentest = (GenTest) target;
            if(GUILayout.Button("Init mesh"))
                _gentest.InitMesh();
        }
    }
}