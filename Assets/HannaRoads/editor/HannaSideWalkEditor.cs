using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HannaSideWalk))]
public class HannaSideWalkEditor : Editor
{


    HannaSideWalk hannaSideWalk;
    void OnEnable()
    {
        hannaSideWalk = target as HannaSideWalk;
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