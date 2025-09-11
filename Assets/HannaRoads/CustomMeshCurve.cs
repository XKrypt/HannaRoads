using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using HannaRoads;
using UnityEditor;
using UnityEngine;

public class CustomMeshCurve : MonoBehaviour
{
    public RSegment rSegment;
    public RSegment nextRSegment;

    public Vector3 controlAOffset = new Vector3(0, 0, -.5f);
    public Vector3 controlBOffset = new Vector3(0, 0, .5f);

    public bool useStartOfSegment;
    public RSegment previousRSegment;
    public bool useEndOfSegment;
    public Mesh originalMesh;
    public Material material;

    public Transform connectionController;

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

        if (previousRSegment != null || nextRSegment != null) return;
        if (previousRSegment.customMeshs.Contains(this))
        {
            previousRSegment.customMeshs.Remove(this);
        }
        if (nextRSegment.customMeshs.Contains(this))
        {
            previousRSegment.customMeshs.Remove(this);
        }
    }

    public void CreateConnectionCurve()
    {
        if (nextRSegment == null || previousRSegment == null || connectionController == null)
        {
            return;
        }

        OrientedPoint start = previousRSegment.GetBezierPointGlobal(useEndOfSegment ? 1 : 0);
        OrientedPoint end = nextRSegment.GetBezierPointGlobal(useStartOfSegment ? 0 : 1);

        Handles.DrawBezier(GetConnectedCurvePoint(start, end, 0).pos, GetConnectedCurvePoint(start, end, 1).pos, start.rot * controlAOffset + start.pos, end.rot * controlBOffset + end.pos, Color.white, EditorGUIUtility.whiteTexture, 5);
    }



    public OrientedPoint GetConnectedCurvePoint(OrientedPoint start, OrientedPoint end, float t)
    {
        Vector3 p0 = start.pos;
        Vector3 p1 = start.rot * controlAOffset + start.pos;
        Vector3 p2 = end.rot * controlBOffset + end.pos;
        Vector3 p3 = end.pos;


        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);


        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 tangent = (e - d).normalized;
        // Interpolate the up-vectors of the start and end points to get the up-vector for the curve.
        Vector3 up = Vector3.Lerp(start.rot * Vector3.up, end.rot * Vector3.up, t).normalized;

        Quaternion rot = Quaternion.LookRotation(tangent, up);
        return new OrientedPoint()
        {
            pos = Vector3.Lerp(d, e, t),
            rot = rot
        };

    }



    public void CheckRegister()
    {
        if (previousRSegment == null || nextRSegment == null) return;
        previousRSegment.customMeshs.Remove(this);
        nextRSegment.customMeshs.Remove(this);
        if (!previousRSegment.customMeshs.Contains(this))
        {
            previousRSegment.customMeshs.Add(this);
        }
        if (!nextRSegment.customMeshs.Contains(this))
        {
            nextRSegment.customMeshs.Add(this);
        }


    }

    public void OnGenerate()
    {

    }

    [ContextMenu("HannaCurbstone/AlignObjectToCurve")]
    public void AlignObjetToCurve()
    {

        if (rSegment == null && hannaIntersection == null && nextRSegment == null && previousRSegment == null)
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
            if (transform.GetChild(0).gameObject.name.Contains("Copy"))
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
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

                if (nextRSegment != null && previousRSegment != null)
                {
                    OrientedPoint start = previousRSegment.GetBezierPointGlobal(useEndOfSegment ? 1 : 0);
                    OrientedPoint end = nextRSegment.GetBezierPointGlobal(useStartOfSegment ? 0 : 1);
                    orientedPoint = GetConnectedCurvePoint(start, end, vertexTPosition);
                    verticesCurved.Add(currentTransform.InverseTransformPoint(orientedPoint.LocalSpace(localSpace + offset)));
                }
                else if (useIntersection)
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

            if (nextRSegment != null && previousRSegment != null)
            {
                OrientedPoint start = previousRSegment.GetBezierPointGlobal(useEndOfSegment ? 1 : 0);
                OrientedPoint end = nextRSegment.GetBezierPointGlobal(useStartOfSegment ? 0 : 1);
                transform.position = GetConnectedCurvePoint(start, end, 0.5f).pos;
                
            }
            else if (useIntersection && hannaIntersection != null)
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
        CreateConnectionCurve();

    }
}