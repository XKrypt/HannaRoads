using UnityEngine;
using UnityEditor;
using HannaRoads;

[CustomEditor(typeof(CustomMeshCurve))]
public class CustomMeshEditor : Editor
{
    CustomMeshCurve customMeshCurve;
    private void OnEnable()
    {
        customMeshCurve = target as CustomMeshCurve;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.Label("References");
        if (!customMeshCurve.useIntersection)
        {
            customMeshCurve.rSegment = (RSegment)EditorGUILayout.ObjectField("Road Segment", customMeshCurve.rSegment, typeof(RSegment), true);

        }
        else
        {

            customMeshCurve.hannaIntersection = (HannaIntersection)EditorGUILayout.ObjectField("Intersection segment", customMeshCurve.hannaIntersection, typeof(HannaIntersection), true);
            if (customMeshCurve.hannaIntersection != null)
            {
                int max = customMeshCurve.hannaIntersection.crossing4 ? 3 : 1;
                customMeshCurve.intersectionIndex = EditorGUILayout.IntSlider(customMeshCurve.intersectionIndex, 0, max);
            }
        }


        customMeshCurve.nextRSegment = (RSegment)EditorGUILayout.ObjectField("Start Road Segment", customMeshCurve.nextRSegment, typeof(RSegment), true);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Use the end of the segment insteado of the start");
        customMeshCurve.useEndOfSegment = EditorGUILayout.Toggle(customMeshCurve.useEndOfSegment);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Use the start of the segment instead of the end");
        customMeshCurve.useStartOfSegment = EditorGUILayout.Toggle(customMeshCurve.useStartOfSegment);
        EditorGUILayout.EndHorizontal();
        customMeshCurve.previousRSegment = (RSegment)EditorGUILayout.ObjectField("End Road Segment", customMeshCurve.previousRSegment, typeof(RSegment), true);

        if (customMeshCurve.nextRSegment != null && customMeshCurve.previousRSegment != null)
        {

            GUILayout.Label("Control Points");
            GUILayout.Label("Start control point");
            customMeshCurve.controlAOffset.x = EditorGUILayout.Slider(customMeshCurve.controlAOffset.x, -5f, 5f);
            customMeshCurve.controlAOffset.y = EditorGUILayout.Slider(customMeshCurve.controlAOffset.y, -5f, 5f);
            customMeshCurve.controlAOffset.z = EditorGUILayout.Slider(customMeshCurve.controlAOffset.z, -5f, 5f);
            GUILayout.Label("End control point");
            customMeshCurve.controlBOffset.x = EditorGUILayout.Slider(customMeshCurve.controlBOffset.x, -5f, 5f);
            customMeshCurve.controlBOffset.y = EditorGUILayout.Slider(customMeshCurve.controlBOffset.y, -5f, 5f);
            customMeshCurve.controlBOffset.z = EditorGUILayout.Slider(customMeshCurve.controlBOffset.z, -5f, 5f);
        }

        GUILayout.Space(5);
        customMeshCurve.originalMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", customMeshCurve.originalMesh, typeof(Mesh), true);
        customMeshCurve.material = (Material)EditorGUILayout.ObjectField("Mesh material", customMeshCurve.material, typeof(Material), true);
        GUILayout.Space(10);
        GUILayout.Label("Mesh modifiers");

        EditorGUILayout.HelpBox("Use \"+\" and \"-\" to make micro adjustments", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("X offset");
        if (GUILayout.Button("-"))
        {
            customMeshCurve.offset.x -= 0.05f;
        }
        if (GUILayout.Button("+"))
        {
            customMeshCurve.offset.x += 0.05f;
        }
        customMeshCurve.offset.x = EditorGUILayout.Slider(customMeshCurve.offset.x, -100f, 100f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Y offset");
        if (GUILayout.Button("-"))
        {
            customMeshCurve.offset.y -= 0.05f;
        }
        if (GUILayout.Button("+"))
        {
            customMeshCurve.offset.y += 0.05f;
        }
        customMeshCurve.offset.y = EditorGUILayout.Slider(customMeshCurve.offset.y, -100f, 100f);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Z offset");
        if (GUILayout.Button("-"))
        {
            customMeshCurve.offset.z -= 0.05f;
        }
        if (GUILayout.Button("+"))
        {
            customMeshCurve.offset.z += 0.05f;
        }
        customMeshCurve.offset.z = EditorGUILayout.Slider(customMeshCurve.offset.z, -100f, 100f);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Number of objects");
        customMeshCurve.objectCount = EditorGUILayout.IntSlider(customMeshCurve.objectCount, 0, 100);
        EditorGUILayout.EndHorizontal();


        if (EditorGUI.EndChangeCheck())
        {
            customMeshCurve.CheckRegister();
            customMeshCurve.AlignObjetToCurve();
        }

    }
}