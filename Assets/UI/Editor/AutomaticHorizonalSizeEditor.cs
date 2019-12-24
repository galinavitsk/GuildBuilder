using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutomaticHorizonalSize))]
public class AutomaticHorizonalSizeEditor :Editor{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        if(GUILayout.Button("Recalc Size")){
        ((AutomaticHorizonalSize)target).AdjustSize();
        }
        
    }
}
