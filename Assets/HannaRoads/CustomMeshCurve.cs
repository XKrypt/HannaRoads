using System;
using System.Collections.Generic;
using HannaRoads;
using UnityEngine;

public class CustomMeshCurve : MonoBehaviour
{
    public RSegment rSegment;
    public Mesh originalMesh;
    public Material material;

    public HannaIntersection hannaIntersection;
    public bool useIntersection;
    public Vector3 offset;

    [Range(0, 3)]
    public int intersectionIndex;


    public int objectCount = 2;


    public List<Vector3> verticesShow = new List<Vector3>();


    private void OnDestroy()
    {
        if (rSegment != null)
        {
            rSegment.customMeshs.Remove(this);
        }

        if (hannaIntersection != null)
        {
            hannaIntersection.customMeshs.Remove(this);
        }
    }


    [ContextMenu("HannaCurbstone/AlignObjectToCurve")]
    public void AlignObjetToCurve()
    {

        if (rSegment == null && hannaIntersection == null)
        {
            Debug.LogError("No rSegment  or intersection Attached is not assigned.", this);
            return;
        }


        if (originalMesh == null)
        {
            Debug.LogError("originalMesh is missing.", this);
            return;
        }

        if (objectCount <= 0)
        {
            Debug.LogError("Object count must be greater than 0.", this);
            return;
        }

        AdjustPosition();

        // Clear previous children to avoid duplication on multiple runs
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        Bounds bounds = originalMesh.bounds;
        Vector3 boundingBoxSize = bounds.size;

        if (Mathf.Approximately(boundingBoxSize.z, 0))
        {
            Debug.LogError("Original mesh has zero size on the Z axis. Cannot map to curve.", this);
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        originalMesh.GetVertices(vertices);
        float tStepPerObject = 1.0f / objectCount;

        for (int i = 0; i < objectCount; i++)
        {
            GameObject obj = new GameObject();
            obj.name = originalMesh.name + " (Copy " + i + ")";
            MeshFilter currentMeshFilter = obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>().material = material;
            Mesh meshObj = new Mesh();
            meshObj.name = originalMesh.name + " (Copy " + i + ")";
            Transform currentTransform = obj.transform;
            currentTransform.SetParent(transform);
            currentTransform.localPosition = Vector3.zero;
            currentTransform.localRotation = Quaternion.identity;
            currentTransform.localScale = Vector3.one;

            List<Vector3> verticesCurved = new List<Vector3>();
            float tStart = i * tStepPerObject;

            foreach (var vertex in vertices)
            {
                float vertexObjTPosition = (vertex.z - bounds.min.z) / boundingBoxSize.z;
                float vertexTPosition = tStart + vertexObjTPosition * tStepPerObject;
                OrientedPoint orientedPoint = new OrientedPoint();
                if (useIntersection)
                {

                    orientedPoint = hannaIntersection.BevelSideOrientedPoint(hannaIntersection.intersections[intersectionIndex], vertexTPosition);
                }
                else
                {
                    orientedPoint = rSegment.GetBezierPointGlobal(vertexTPosition);
                }

                Vector3 localSpace = new Vector3(vertex.x, vertex.y, 0);
                if (useIntersection)
                {
                    Vector3 worldPosition = hannaIntersection.transform.TransformPoint(orientedPoint.LocalSpace(localSpace + offset));
                    verticesCurved.Add(currentTransform.InverseTransformPoint(worldPosition));

                }
                else
                {
                    verticesCurved.Add(currentTransform.InverseTransformPoint(orientedPoint.LocalSpace(localSpace + offset)));
                }
            }

            meshObj.SetVertices(verticesCurved);
            meshObj.triangles = originalMesh.triangles;
            meshObj.uv = originalMesh.uv;
            meshObj.normals = originalMesh.normals;
            meshObj.tangents = originalMesh.tangents;
            meshObj.colors = originalMesh.colors;

            meshObj.RecalculateBounds();
            meshObj.RecalculateNormals();

            currentMeshFilter.sharedMesh = meshObj;
        }

        void AdjustPosition()
        {

            if (useIntersection && hannaIntersection != null)
            {
                transform.localPosition = hannaIntersection.BevelSideOrientedPoint(hannaIntersection.intersections[intersectionIndex], 0.5f).pos;
            }
            else if (rSegment != null)
            {
                transform.localPosition = rSegment.GetBezierPoint(0.5f).LocalSpace(offset);
            }
        }

        void CheckIntersectionIndex()
        {
            if (hannaIntersection == null) return;
            if (hannaIntersection.crossing4 && intersectionIndex > 1)
            {
                intersectionIndex = 1;
            }
        }
    }


    private void OnDrawGizmos()
    {
        // if (meshFilter != null && meshFilter.sharedMesh != null)
        // {
        //     Gizmos.DrawWireCube(meshFilter.transform.TransformPoint(meshFilter.sharedMesh.bounds.center), meshFilter.sharedMesh.bounds.size);
        //     Gizmos.color = Color.magenta;
        //     foreach (var vertex in verticesShow)
        //     {
        //         Gizmos.DrawCube(meshFilter.transform.TransformPoint(vertex), Vector3.one * 0.05f);
        //     }
        //     Gizmos.color = Color.white;
        // }
    }
}