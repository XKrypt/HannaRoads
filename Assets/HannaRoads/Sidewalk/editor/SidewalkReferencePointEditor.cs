using UnityEngine;
using UnityEditor;
using HannaRoads;
using System.Linq;

[CustomEditor(typeof(SideWalkReferencePoint))]
[CanEditMultipleObjects]
public class sideWalkReferencePointEditor : Editor
{

    SideWalkReferencePoint referencePoint;
    SideWalkReferencePoint[] referencePoints = new SideWalkReferencePoint[1];

    void OnEnable()
    {
        referencePoint = target as SideWalkReferencePoint;
        referencePoints = targets.Cast<SideWalkReferencePoint>().ToArray();
    }

    public GUIStyle TitleStyle()
    {

        return new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
    }

    public void OnSceneGUI()
    {


    }


    public override void OnInspectorGUI()
    {

        //Add connection later
        if (referencePoints != null && referencePoints.Length > 1)
        {
            if (referencePoints.Length == 2 && referencePoints[1].segmentType != referencePoints[0].segmentType)
            {
                if (GUILayout.Button("Connect"))
                {
                    SideWalkReferencePoint start = referencePoints[0].segmentType == SegmentType.Start ? referencePoints[0] : referencePoints[1];
                    SideWalkReferencePoint end = referencePoints[1].segmentType == SegmentType.End ? referencePoints[1] : referencePoints[0];


                    SideWalkSegment createdSegment = start.hannaSideWalkEditor.SpawnRoad(end.transform.position);

                    createdSegment.startRef = end;
                    createdSegment.endRef = start;

                    createdSegment.controlPoints[1].transform.position = Vector3.Lerp(start.transform.position, end.transform.position, 0.1f);
                    createdSegment.controlPoints[0].transform.position = Vector3.Lerp(end.transform.position, start.transform.position, 0.1f);

                    start.previousSSegment = createdSegment;
                    end.sSegment = createdSegment;

                    start.segmentType = SegmentType.End;

                    start.UpdatePositions();
                    end.UpdatePositions();

                    start.UpdateReference();
                    end.UpdateReference();
                }
            }

            return;
        }






        EditorGUILayout.LabelField($"Segment Type: {referencePoint.segmentType}", TitleStyle());
        EditorGUI.BeginChangeCheck();


        if (EditorGUI.EndChangeCheck())
        {
            referencePoint.UpdateReference();
        }
        serializedObject.ApplyModifiedProperties();
    }
}



