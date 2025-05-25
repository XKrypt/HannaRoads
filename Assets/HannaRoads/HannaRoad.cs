using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;



namespace HannaRoads
{
    [ExecuteInEditMode]
    public class HannaRoad : MonoBehaviour
    {
        public List<RSegment> rSegments = new List<RSegment>();

        public List<ReferencePoint> referencePoints = new List<ReferencePoint>();

        public RSegment activeSegment;
        public HannaIntersection activeIntersection;

        public int maxThreadsPerSegment;


        public RSegment lastRSegment;


        public void AddRSegment(RSegment rSegment)
        {
            rSegments.Add(rSegment);
        }
        [ContextMenu("HannaRoads/Clear")]
        public void Clear()
        {
            rSegments.Clear();
        }

        public void UpdateRSegments()
        {
            foreach (var item in rSegments)
            {
                if (item == null)
                {
                    rSegments.Remove(item);
                    continue;
                }
                item.Generate();
            }
        }

        public void AlignTerrainForAllSegments()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(UpdateAllRoadsTerrains());
        }


        IEnumerator UpdateAllRoadsTerrains()
        {
            int count = 0;
            while (count < rSegments.Count)
            {
                rSegments[count].AlignTerrain();
                EditorUtility.DisplayProgressBar("Updating Roads", $"Updating {rSegments[count].name}", count / (float)rSegments.Count);

                while (rSegments[count].isAligningTerrain)
                {
                    yield return null;
                }
                count++;
                yield return null;
            }
            EditorUtility.ClearProgressBar();
        }

        public RSegment SpawnRoad(Vector3 position)
        {
            GameObject roadObject = new GameObject("RSegment." + rSegments.Count);

            roadObject.AddComponent<RSegment>().meshFilter = roadObject.AddComponent<MeshFilter>();
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

            RSegment rSegment = roadObject.GetComponent<RSegment>();

            rSegment.start = start.transform;
            rSegment.end = end.transform;

            rSegment.controlPoints.Add(b1.AddComponent<ControlPoint>());
            rSegment.controlPoints.Add(b2.AddComponent<ControlPoint>());

            b1.GetComponent<ControlPoint>().root = start.transform;
            b2.GetComponent<ControlPoint>().root = end.transform;


            b1.GetComponent<ControlPoint>().rSegment = rSegment;
            b2.GetComponent<ControlPoint>().rSegment = rSegment;
            b1.GetComponent<ControlPoint>().segmentType = SegmentType.Start;
            b2.GetComponent<ControlPoint>().segmentType = SegmentType.End;

            AddRSegment(rSegment);

            rSegment.hannaRoad = this;

            return rSegment;

        }

        public ReferencePoint CreateReferencePoint(RSegment rSegment, RSegment previous = null)
        {
            GameObject referencePoint = new GameObject();
            referencePoint.name = "ReferencePoint." + referencePoints.Count;
            referencePoint.AddComponent<ReferencePoint>().hannaRoad = this;
            referencePoint.GetComponent<ReferencePoint>().rSegment = rSegment;
            referencePoint.GetComponent<ReferencePoint>().previousRSegment = previous;
            return referencePoint.GetComponent<ReferencePoint>();
        }


        public void ClearRSegments(RSegment rSegment)
        {
            rSegments.Clear();
        }

        public void LoadSegments()
        {
            rSegments.Clear();
            rSegments.AddRange(GetComponentsInChildren<RSegment>());
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
