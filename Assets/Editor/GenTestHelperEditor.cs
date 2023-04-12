using System;
using _Project.Marching_Cubes.Scripts;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenTest))]
public class GenTestHelperEditor : Editor {
    private GenTest _gentest;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        _gentest = (GenTest) target;
        if(GUILayout.Button("Init mesh"))
            _gentest.InitMesh();
    }
}