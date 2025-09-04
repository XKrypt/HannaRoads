using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;



namespace HannaRoads
{ 
    [ExecuteInEditMode]
    public class HannaSidewalk : MonoBehaviour
    {
        public List<SideWalkSegment> sSegments = new List<SideWalkSegment>();

        public List<SideWalkReferencePoint> referencePoints = new List<SideWalkReferencePoint>();

        public SideWalkSegment activeSegment;
        public HannaIntersection activeIntersection;

        public int maxThreadsPerSegment;


        public RSegment lastRSegment;
        public Material curbstoneDefaultMaterial;
        public Material sidewalkDefaultMaterial;

        public void AddRSegment(SideWalkSegment sSegment)
        {
            sSegments.Add(sSegment);
        }
        [ContextMenu("HannaRoads/Clear")]
        public void Clear()
        {
            sSegments.Clear();
        }

        public void UpdateRSegments()
        {
            foreach (var item in sSegments)
            {
                if (item == null)
                {
                    sSegments.Remove(item);
                    continue;
                }
                item.Generate();
            }
        }




        public SideWalkSegment SpawnRoad(Vector3 position)
        {
            GameObject roadObject = new GameObject("RSegment." + sSegments.Count);

            roadObject.AddComponent<SideWalkSegment>().meshFilter = roadObject.AddComponent<MeshFilter>();
            roadObject.AddComponent<MeshRenderer>();
            roadObject.transform.SetParent(transform);

            GameObject start = new GameObject("Start");
            GameObject b1 = new GameObject("Bezier 1");
            GameObject b2 = new GameObject("Bezier 2");
            GameObject end = new GameObject("End");




            roadObject.transform.position = position;
            start.transform.position = roadObject.transform.position;
            b1.transform.position = Vector3.Lerp(start.transform.position, end.transform.position, 0.1f);

            start.transform.SetParent(roadObject.transform);
            b1.transform.SetParent(roadObject.transform);
            b2.transform.SetParent(roadObject.transform);
            end.transform.SetParent(roadObject.transform);

            SideWalkSegment sSegment = roadObject.GetComponent<SideWalkSegment>();

            sSegment.start = start.transform;
            sSegment.end = end.transform;

            sSegment.controlPoints.Add(b1.AddComponent<SidewalkControlPoint>());
            sSegment.controlPoints.Add(b2.AddComponent<SidewalkControlPoint>());

            b1.GetComponent<SidewalkControlPoint>().root = start.transform;
            b2.GetComponent<SidewalkControlPoint>().root = end.transform;


            b1.GetComponent<SidewalkControlPoint>().sSegment = sSegment;
            b2.GetComponent<SidewalkControlPoint>().sSegment = sSegment;
            b1.GetComponent<SidewalkControlPoint>().segmentType = SegmentType.Start;
            b2.GetComponent<SidewalkControlPoint>().segmentType = SegmentType.End;

            AddRSegment(sSegment);

            sSegment.hannaSideWalkEditor = this;

            sSegment.AddCurbstone();

            return sSegment;

        }

        public SideWalkReferencePoint CreateReferencePoint(SideWalkSegment sSegment, SideWalkSegment previous = null)
        {
            GameObject referencePoint = new GameObject();
            referencePoint.name = "ReferencePoint." + referencePoints.Count;
            referencePoint.AddComponent<SideWalkReferencePoint>().hannaSideWalkEditor = this;
            referencePoint.GetComponent<SideWalkReferencePoint>().sSegment = sSegment;
            referencePoint.GetComponent<SideWalkReferencePoint>().previousSSegment = previous;
            return referencePoint.GetComponent<SideWalkReferencePoint>();
        }


        public void ClearRSegments(RSegment rSegment)
        {
            sSegments.Clear();
        }

        public void LoadSegments()
        {
            sSegments.Clear();
            sSegments.AddRange(GetComponentsInChildren<SideWalkSegment>());
        }


        private void OnDrawGizmost()
        {


            // if (referencePoints.Count - 1 != rSegments.Count)
            // {
            //     Debug.LogWarning("Less referencesPoints than segments");
            //     return;
            // }

            // foreach (var item in referencePoints)
            // {
            //     if (item.GetComponent<ReferencePoint>() == null)
            //     {
            //         item.AddComponent<ReferencePoint>();
            //     }
            // }
            // for (int i = 0; i < referencePoints.Count - 1; i++)
            // {
            //     if (i >= rSegments.Count) break;

            //     referencePoints[i].GetComponent<ReferencePoint>().SetType(SegmentType.Start);
            //     referencePoints[i + 1].GetComponent<ReferencePoint>().SetType(SegmentType.End);
            //     referencePoints[i + 1].GetComponent<ReferencePoint>().SetRSegment(rSegments[i]);
            //     rSegments[i].start.position = referencePoints[i].position;
            //     rSegments[i].end.position = referencePoints[i + 1].position;


            //     rSegments[i].transform.position = referencePoints[i].position;
            //     rSegments[i].transform.rotation = referencePoints[i].rotation;
            // }


            // for (int i = 0; i < rSegments.Count - 1; i++)
            // {
            //     List<Vector3> verticesSegA = rSegments[i].mesh.vertices.ToList();
            //     List<Vector3> verticesSegB = rSegments[i + 1].mesh.vertices.ToList();

            //     Vector3 vertexAPos = rSegments[i].meshFilter.transform.TransformPoint(verticesSegA.Last());
            //     Vector3 vertexBPos = rSegments[i].meshFilter.transform.TransformPoint(verticesSegA[verticesSegA.Count - 2]);

            //     Vector3 convertedPositionA = rSegments[i + 1].meshFilter.transform.InverseTransformPoint(vertexAPos);
            //     Vector3 convertedPositionB = rSegments[i + 1].meshFilter.transform.InverseTransformPoint(vertexBPos);





            //     verticesSegB[1] = vertexAPos;
            //     verticesSegB[0] = vertexBPos;


            //     rSegments[i + 1].mesh.SetVertices(verticesSegB);

            // }

        }

    }

}
