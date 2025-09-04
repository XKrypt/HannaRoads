using UnityEngine;
using UnityEditor;

namespace HannaRoads.HannaEditor
{


    [CustomEditor(typeof(SideWalkSegment))]
    public class SideWalkSegmentEditor : Editor
    {
        SideWalkSegment sSegment;
        void OnEnable()
        {
            sSegment = target as SideWalkSegment;
            if (sSegment.downHorizontalCurve == null)
            {
                sSegment.downHorizontalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
        }
        public override void OnInspectorGUI()
        {


            EditorGUILayout.LabelField("Shift + E : Connect to active intersection");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("width");
            sSegment.sideWalkWidth = EditorGUILayout.Slider(sSegment.sideWalkWidth, 0.02f, 20);
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Detail Level");
            sSegment.resolution = EditorGUILayout.IntSlider(sSegment.resolution, 1, 200);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Horizontal detail Level");
            sSegment.slices = EditorGUILayout.IntSlider(sSegment.slices, 2, 10);
            EditorGUILayout.EndHorizontal();



            // EditorGUILayout.BeginHorizontal();
            // sSegment.widthCurve = EditorGUILayout.CurveField("Width smoothness curve", sSegment.widthCurve);
            // EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            sSegment.downHorizontalCurve = EditorGUILayout.CurveField("Down curve", sSegment.downHorizontalCurve);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Reference points", TitleStyle());

            sSegment.startRef = (SideWalkReferencePoint)EditorGUILayout.ObjectField("Start reference point", sSegment.startRef, typeof(SideWalkReferencePoint), true);
            sSegment.endRef = (SideWalkReferencePoint)EditorGUILayout.ObjectField("End reference point", sSegment.endRef, typeof(SideWalkReferencePoint), true);



            if (sSegment.hannaSideWalkEditor.activeSegment != sSegment && sSegment.endRef.sSegment == null)
            {
                if (GUILayout.Button("Set as active"))
                {
                    sSegment.hannaSideWalkEditor.activeSegment = sSegment;
                }

            }

            if (GUILayout.Button("Add curbstone"))
            {
                sSegment.AddCurbstone();
            }



            base.OnInspectorGUI();


            if (EditorGUI.EndChangeCheck())
            {
                sSegment.Generate();
                if (sSegment.startRef.previousSSegment != null)
                {
                    sSegment.startRef.previousSSegment.Generate();
                }

            }

            serializedObject.ApplyModifiedProperties();



        }






        public GUIStyle TitleStyle(int fontSize = 16)
        {

            return new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = fontSize
            };
        }
    }

}