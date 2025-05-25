using UnityEngine;
using UnityEditor;
using HannaRoads;
using System.Linq;

[CustomEditor(typeof(ReferencePoint))]
[CanEditMultipleObjects]
public class ReferencePointEditor : Editor
{

    ReferencePoint referencePoint;
    ReferencePoint[] referencePoints = new ReferencePoint[1];

    void OnEnable()
    {
        referencePoint = target as ReferencePoint;
        referencePoints = targets.Cast<ReferencePoint>().ToArray();
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

        Event frameEvent = Event.current;

        if (frameEvent.shift && frameEvent.keyCode == KeyCode.E && frameEvent.type == EventType.KeyDown && referencePoint.hannaRoad.activeIntersection != null)
        {
            if (referencePoint.segmentType == SegmentType.End)
            {
                if (referencePoint.rSegment == null)
                {
                    referencePoint.intersectionAttachment = referencePoint.hannaRoad.activeIntersection;

                    referencePoint.previousRSegment.endIntersection = referencePoint.hannaRoad.activeIntersection;
                }
            }
            if (referencePoint.segmentType == SegmentType.Start)
            {
                if (referencePoint.previousRSegment == null)
                {
                    referencePoint.intersectionAttachment = referencePoint.hannaRoad.activeIntersection;
                    referencePoint.rSegment.startIntersection = referencePoint.hannaRoad.activeIntersection;
                }
            }

            referencePoint.UpdateReference();
        }
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
                    ReferencePoint start = referencePoints[0].segmentType == SegmentType.Start ? referencePoints[0] : referencePoints[1];
                    ReferencePoint end = referencePoints[1].segmentType == SegmentType.End ? referencePoints[1] : referencePoints[0];


                    RSegment createdSegment = start.hannaRoad.SpawnRoad(end.transform.position);

                    createdSegment.startRef = end;
                    createdSegment.endRef = start;

                    createdSegment.controlPoints[1].transform.position = Vector3.Lerp(start.transform.position,end.transform.position, 0.1f);
                    createdSegment.controlPoints[0].transform.position = Vector3.Lerp(end.transform.position,start.transform.position, 0.1f);

                    start.previousRSegment = createdSegment;
                    end.rSegment = createdSegment;

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

        if (referencePoint.intersectionAttachment)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Intersection position");
            referencePoint.interIndex = EditorGUILayout.IntSlider(referencePoint.interIndex, 0, referencePoint.intersectionAttachment.crossing4 ? 3 : 2);
            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.BeginHorizontal();
        referencePoint.intersectionAttachment = (HannaIntersection)EditorGUILayout.ObjectField("Intersection connected", referencePoint.intersectionAttachment, typeof(HannaIntersection), true);
        EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("Add intersection"))
        {
            referencePoint.CreateIntersection();
        }


        if (EditorGUI.EndChangeCheck())
        {
            referencePoint.UpdateReference();
        }
        serializedObject.ApplyModifiedProperties();
    }
}



