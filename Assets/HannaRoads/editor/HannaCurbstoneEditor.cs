using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HannaCurbstone))]
public class HannaCurbstoneEditor : Editor
{


    HannaCurbstone hannaSideWalk;
    void OnEnable()
    {
        hannaSideWalk = target as HannaCurbstone;
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            hannaSideWalk.GenerateCurbstone();
        }

    }
}