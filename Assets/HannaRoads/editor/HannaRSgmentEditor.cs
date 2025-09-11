using UnityEngine;
using UnityEditor;
using HannaRoads;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace HannaRoads.HannaEditor
{


    [CustomEditor(typeof(RSegment))]
    public class HannaRSgmentEditor : Editor
    {
        RSegment rSegment;
        void OnEnable()
        {
            rSegment = target as RSegment;
            if (rSegment.widthCurve == null)
            {
                rSegment.widthCurve = AnimationCurve.Constant(0, 1, 1);
            }
            if (rSegment.widthCurveToNextRSegment == null)
            {
                rSegment.widthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
            if (rSegment.terrainAlignCurve == null)
            {
                rSegment.terrainAlignCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
        }
        public override void OnInspectorGUI()
        {


            EditorGUILayout.LabelField("Shift + E to connect to active intersection");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width");
            rSegment.width = EditorGUILayout.Slider(rSegment.width, 0.02f, 20);
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Detail Level");
            rSegment.detailLevel = EditorGUILayout.IntSlider(rSegment.detailLevel, 1, 200);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Horizontal detail Level");
            rSegment.sliceResolution = EditorGUILayout.IntSlider(rSegment.sliceResolution, 2, 10);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Shape configurations", TitleStyle());
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Start curve offset");
            rSegment.startOffset = EditorGUILayout.Slider(rSegment.startOffset, 0, 0.99f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("End curve offset");
            rSegment.endOffset = EditorGUILayout.Slider(rSegment.endOffset, 0.01f, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            rSegment.widthCurveToNextRSegment = EditorGUILayout.CurveField("Width smoothness curve to next segment", rSegment.widthCurveToNextRSegment);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Don´t be affeccted by the next road width curve");
            rSegment.dontBeAffectedForNextRoadWidthShape = EditorGUILayout.Toggle(rSegment.dontBeAffectedForNextRoadWidthShape);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Don´t be affeccted by the next road height curve");
            rSegment.dontBeAffectedForNextRoadHeightShape = EditorGUILayout.Toggle(rSegment.dontBeAffectedForNextRoadHeightShape);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            rSegment.widthCurve = EditorGUILayout.CurveField("Width smoothness curve", rSegment.widthCurve);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            rSegment.verticalProfile = EditorGUILayout.CurveField("Height smoothness curve", rSegment.verticalProfile);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width profile multiplier");
            rSegment.widthProfileMultiplier = EditorGUILayout.Slider(rSegment.widthProfileMultiplier, -5, 5);
            if (rSegment.widthProfileMultiplier == 0)
            {
                rSegment.widthProfileMultiplier = 1;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height profile multiplier");
            rSegment.verticalProfileMultiplayer = EditorGUILayout.Slider(rSegment.verticalProfileMultiplayer, -5, 5);
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(15);

            if (GUILayout.Button("Add custom mesh"))
            {
                rSegment.AddCustomMeshCurve();
            }
            GUILayout.Space(15);

            GUILayout.Space(50);

            EditorGUILayout.LabelField("Terrain settings", TitleStyle());

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Align radius");
            rSegment.terrainAlignRadius = EditorGUILayout.Slider(rSegment.terrainAlignRadius, 0.01f, 100);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(2);
            EditorGUILayout.HelpBox("If this is marked, the terrain will not be aligned with this segment when \"Align all roads with terrain\" button is pressed", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ignore this segment from being updated in terrain global alignment");
            rSegment.ignoreFromRoadSystemTerrainUpdate = EditorGUILayout.Toggle(rSegment.ignoreFromRoadSystemTerrainUpdate);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

           

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bottom margin");
            rSegment.terrainBottomMargin = EditorGUILayout.Slider(rSegment.terrainBottomMargin, -1, 1);
            EditorGUILayout.EndHorizontal();



            if (GUILayout.Button("Align terrain"))
            {
                if (!rSegment.isAligningTerrain)
                {
                    rSegment.AlignTerrain();
                }
                else
                {
                    Debug.LogWarning("Operation is already in process, wait for it ends to run again");
                }
            }



            Rect rect = GUILayoutUtility.GetRect(18, 18);
            EditorGUI.ProgressBar(rect, rSegment.alignTerrainProgress, $"{(int)(rSegment.alignTerrainProgress)}%");


            GUILayout.Space(10);



            EditorGUILayout.LabelField("Reference points", TitleStyle());

            rSegment.startRef = (ReferencePoint)EditorGUILayout.ObjectField("Start reference point", rSegment.startRef, typeof(ReferencePoint), true);
            rSegment.endRef = (ReferencePoint)EditorGUILayout.ObjectField("End reference point", rSegment.endRef, typeof(ReferencePoint), true);



            if (rSegment.hannaRoad.activeSegment != rSegment && rSegment.endRef.rSegment == null)
            {
                if (GUILayout.Button("Set as active"))
                {
                    rSegment.hannaRoad.activeSegment = rSegment;
                }

            }

            MeshesEditor();



            if (rSegment.endOffset <= rSegment.startOffset)
            {
                rSegment.endOffset = rSegment.startOffset + 0.01f;
                rSegment.startOffset = rSegment.endOffset - 0.01f;

                rSegment.startOffset = Mathf.Clamp(rSegment.startOffset, 0, 0.99f);
                rSegment.endOffset = Mathf.Clamp(rSegment.endOffset, 0.01f, 1f);
            }




            if (EditorGUI.EndChangeCheck())
            {
                rSegment.Generate();
                if (rSegment.startRef.previousRSegment != null)
                {
                    rSegment.startRef.previousRSegment.Generate();
                }

                UpdateRoadLines();

            }

            serializedObject.ApplyModifiedProperties();

            if (rSegment.isAligningTerrain)
            {
                Repaint();

                previousAligning = true;
            }

            if (!rSegment.isAligningTerrain && previousAligning)
            {
                Repaint();

                previousAligning = false;
            }
        }


        bool previousAligning;


        public void UpdateRoadLines()
        {
            for (int i = 0; i < rSegment.roadLines.Count; i++)
            {
                RoadLine roadLine = rSegment.roadLines[i];
                rSegment.GenerateRoadLine(ref roadLine.mesh, roadLine);
            }
        }



        public void MeshesEditor()
        {


            GUILayout.Space(50);
            EditorGUILayout.LabelField("Road Lines settings", TitleStyle());
            for (int i = 0; i < rSegment.roadLines.Count; i++)
            {

                RoadLine roadLine = rSegment.roadLines[i];
                if (roadLine == null) continue;



                EditorGUILayout.LabelField(roadLine.gameObject.name, TitleStyle(14));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Width");
                roadLine.width = EditorGUILayout.Slider(roadLine.width, 0.02f, 20);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Horizontal offset");
                roadLine.horizontalOffset = EditorGUILayout.Slider(roadLine.horizontalOffset, -20f, 20);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Vertical offset");
                roadLine.verticalOffset = EditorGUILayout.Slider(roadLine.verticalOffset, -1, 1);
                EditorGUILayout.EndHorizontal();



                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Detail Level");
                roadLine.detailLevel = EditorGUILayout.IntSlider(roadLine.detailLevel, 2, 200);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Start");
                roadLine.start = EditorGUILayout.Slider(roadLine.start, 0, roadLine.end -0.3f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("End");
                roadLine.end = EditorGUILayout.Slider(roadLine.end, 1, roadLine.start + 0.3f);
                EditorGUILayout.EndHorizontal();


                // EditorGUILayout.BeginHorizontal();
                // roadLine.widthProfile = EditorGUILayout.CurveField("Width smoothness curve", roadLine.widthProfile);
                // EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Horizontal resolution");
                roadLine.sliceResolution = EditorGUILayout.IntSlider(roadLine.sliceResolution, 2, 200);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                roadLine.verticalProfile = EditorGUILayout.CurveField("Vertical profile", roadLine.verticalProfile);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Vertical profile multiplier");
                roadLine.verticalProfileMultiplayer = EditorGUILayout.Slider(roadLine.verticalProfileMultiplayer, -1, 1);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);


                if (GUILayout.Button("Remove"))
                {
                    DestroyImmediate(roadLine.gameObject);
                }

                GUILayout.Space(7);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Add road line"))
            {
                GameObject newObj = new GameObject();

                newObj.name = "roadLine." + rSegment.roadLines.Count;



                RoadLine roadLine = newObj.AddComponent<RoadLine>();

                newObj.transform.position = rSegment.transform.position;
                newObj.transform.parent = rSegment.transform;

                roadLine.detailLevel = 10;

                Mesh newMesh = new Mesh();
                newMesh.name = "Road_Line." + rSegment.roadLines.Count;
                roadLine.meshRenderer = newObj.AddComponent<MeshRenderer>();
                newObj.AddComponent<MeshFilter>().sharedMesh = newMesh;
                roadLine.mesh = newMesh;

                roadLine.width = rSegment.width;
                roadLine.horizontalOffset = 0f;
                roadLine.widthProfile = AnimationCurve.EaseInOut(0, 0, 1, 1);
                roadLine.start = 0;
                roadLine.end = 1;

                roadLine.meshRenderer.sharedMaterial = rSegment.defaultRoadLineMaterial;

                roadLine.rSegment = rSegment;

                rSegment.GenerateRoadLine(ref newMesh, roadLine);

                rSegment.roadLines.Add(roadLine);
            }

            EditorGUILayout.Separator();
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