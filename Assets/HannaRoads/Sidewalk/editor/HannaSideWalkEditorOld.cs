using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SideWalkOld))]
public class HannaSideWalkEditorOld : Editor
{


    SideWalkOld hannaSideWalk;
    void OnEnable()
    {
        hannaSideWalk = target as SideWalkOld;
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            hannaSideWalk.GenerateSideWalk();
        }

    }
}