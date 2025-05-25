using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using PlasticGui;
using HannaRoads;


namespace HannaRoads.HannaEditor
{
    [CustomEditor(typeof(HannaIntersection))]
    [CanEditMultipleObjects]
    public class HannaIntersectionEditor : Editor
    {

        HannaIntersection hannaIntersection;

        void OnEnable()
        {
            hannaIntersection = target as HannaIntersection;

            hannaIntersection.Generate();

        }

        float size;
        float extrusion;

        public override void OnInspectorGUI()
        {

            EditorGUI.BeginChangeCheck();

            SizeController();

            GeometryController();

            TerrainController();



            if (EditorGUI.EndChangeCheck())
            {
                hannaIntersection.Generate();
            }
            serializedObject.ApplyModifiedProperties();
            if (hannaIntersection.hannaRoad.activeIntersection != hannaIntersection)
            {
                if (GUILayout.Button("Set as active"))
                {
                    hannaIntersection.hannaRoad.activeIntersection = hannaIntersection;
                }

            }
        }


        void OnSceneGUI()
        {
            foreach (var item in hannaIntersection.intersectionPoints)
            {
                Handles.DrawWireCube(item.middlePoint, Vector3.one * 0.3f);
            }
        }

        public GUIStyle TitleStyle()
        {

            return new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 16
            };
        }


        public void TerrainController()
        {
            EditorGUILayout.LabelField("Terrain settings", TitleStyle());

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Align radius");
            hannaIntersection.terrainAlignRadius = EditorGUILayout.Slider(hannaIntersection.terrainAlignRadius, 0.01f, 100);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Align  max distance");
            hannaIntersection.maxAlignDistance = EditorGUILayout.Slider(hannaIntersection.maxAlignDistance, hannaIntersection.minAlignDistance + 0.05f, hannaIntersection.terrainAlignRadius);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Align min distance");
            hannaIntersection.minAlignDistance = EditorGUILayout.Slider(hannaIntersection.minAlignDistance, 0, hannaIntersection.maxAlignDistance - 0.05f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bottom margin");
            hannaIntersection.terrainBottomMargin = EditorGUILayout.Slider(hannaIntersection.terrainBottomMargin, -1, 1);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            hannaIntersection.terrainAlignCurve = EditorGUILayout.CurveField("Align smoothness curve", hannaIntersection.terrainAlignCurve);
            EditorGUILayout.EndHorizontal();



            if (GUILayout.Button("Align terrain"))
            {
                hannaIntersection.AlignTerrain();
            }
        }

        public void SizeController()
        {




            EditorGUILayout.LabelField("Size Controllers", TitleStyle());
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size of Intersection");
            hannaIntersection.size = EditorGUILayout.Slider(hannaIntersection.size, 0, 10);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Extrusion of the intersection");
            hannaIntersection.extrusionSize = EditorGUILayout.Slider(hannaIntersection.extrusionSize, 0, 10);
            EditorGUILayout.EndHorizontal();


        }
        public void GeometryController()
        {




            EditorGUILayout.LabelField("Geometry modifiers", TitleStyle());
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resolution corner curve");
            hannaIntersection.resolutionCurve = EditorGUILayout.IntSlider(hannaIntersection.resolutionCurve, 0, 32);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Shape");
            hannaIntersection.shape = EditorGUILayout.Slider(hannaIntersection.shape, 0, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Make 3 or 4 way intersection");
            hannaIntersection.crossing4 = EditorGUILayout.Toggle(hannaIntersection.crossing4);
            EditorGUILayout.EndHorizontal();

        }

    }

}