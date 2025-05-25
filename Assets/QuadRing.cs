using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
public class QuadRing : MonoBehaviour
{


    [Range(0.01f, 1f)]
    public float innerRadius;
    [Range(0.01f, 1f)]
    public float thickness;

    int VertexCount => angularSegmentsCount * 2;
    float RadiusOuter => innerRadius + thickness;

    [Range(3, 256)]
    public int angularSegmentsCount = 3;


    Mesh mesh;

    [SerializeField] Projection projection;
    public enum Projection
    {
        ZProject,
        RadialProjection
    }
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        GenerateMesh();
    }

    private void OnDrawGizmosSelected()
    {

        Gizmosfs.DrawWireCircle(transform.position, transform.rotation, innerRadius, angularSegmentsCount);
        Gizmosfs.DrawWireCircle(transform.position, transform.rotation, RadiusOuter, angularSegmentsCount);

    }


    void Awake()
    {
        mesh = new Mesh();

        mesh.name = "QuadRing";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void GenerateMesh()
    {
        mesh.Clear();

        int vCount = VertexCount;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < angularSegmentsCount + 1; i++)
        {
            float t = i / (float)angularSegmentsCount;
            float angRad = t * MathFunc.TAU;

            Vector2 dir = MathFunc.GetUnitVectorByAngle(angRad);

            vertices.Add(dir * RadiusOuter);
            vertices.Add(dir * innerRadius);

            normals.Add(Vector3.forward);
            normals.Add(Vector3.forward);


            switch (projection)
            {
                case Projection.RadialProjection:
                    uvs.Add(new Vector2(t, 1));
                    uvs.Add(new Vector2(t, 0));
                    break;

                case Projection.ZProject:
                    uvs.Add(dir*0.5f + Vector2.one * 0.5f);
                    uvs.Add(dir * (innerRadius / RadiusOuter) * dir*0.5f + Vector2.one * 0.5f);
                    break;

                default:
                    break;
            }

        }

        List<int> triangleIndices = new List<int>();


        for (int i = 0; i < angularSegmentsCount; i++)
        {
            int indexRoot = i * 2;

            int indexInnerRoot = indexRoot + 1;
            int indexOuterNext = (indexRoot + 2);
            int indexInnerNext = (indexRoot + 3);

            triangleIndices.Add(indexRoot);
            triangleIndices.Add(indexOuterNext);
            triangleIndices.Add(indexInnerNext);


            triangleIndices.Add(indexRoot);
            triangleIndices.Add(indexInnerNext);
            triangleIndices.Add(indexInnerRoot);

        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangleIndices, 0);
        mesh.SetUVs(0, uvs);

    }
}
