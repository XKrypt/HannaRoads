using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
[RequireComponent(typeof(MeshFilter))]
public class RoadSegment : MonoBehaviour
{
    public Mesh2D shape2D;
    [Range(0, 1)]
    [SerializeField] float tTest = 0;

    [Range(2, 32)]
    [SerializeField] int edgeRingCount = 8;
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    Vector3 GetPoint(int i){

        if (i == 0) return startPoint.position;
        if (i == 1) return startPoint.TransformPoint(Vector3.forward * startPoint.localScale.z);
        if (i == 2) return endPoint.TransformPoint(Vector3.back * endPoint.localScale.z);
        if (i == 3) return endPoint.position;

        return default;
    }
    Mesh mesh;



    private void Awake()
    {

        mesh = new Mesh
        {
            name = "Segment"
        };
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
    void GenerateMesh()
    {

        mesh.Clear();


        float uSpan = shape2D.CalcUspan();

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int ring = 0; ring < edgeRingCount + 1; ring++)
        {
            float t = ring / (float)(edgeRingCount - 1);
            OrientedPoint op = GetBezierPoint(t);
            for (int i = 0; i < shape2D.vertices.Length; i++)
            {
                verts.Add(op.LocalToWorldPos(shape2D.vertices[i].point));
                normals.Add(op.LocalToWorldVector(shape2D.vertices[i].normal));
                uvs.Add(new Vector2(shape2D.vertices[i].u , t * GetApproxLength() / uSpan));
            }
        }


        //Triangles

        List<int> trianglesIndices = new List<int>();
        for (int ring = 0; ring < edgeRingCount - 1; ring++)
        {

            int rootIndex = ring * shape2D.VertexCount;
            int rootIndexNext = (ring + 1) * shape2D.VertexCount;

            for (int line = 0; line < shape2D.LineCount; line += 2)
            {
                int lineIndexA = shape2D.lineIndices[line];
                int lineIndexB = shape2D.lineIndices[line + 1];

                int currentA = rootIndex + lineIndexA;
                int currentB = rootIndex + lineIndexB;


                int nextA = rootIndexNext + lineIndexA;
                int nextB = rootIndexNext + lineIndexB;

                trianglesIndices.Add(currentA);
                trianglesIndices.Add(nextA);
                trianglesIndices.Add(nextB);

                trianglesIndices.Add(currentA);
                trianglesIndices.Add(nextB);
                trianglesIndices.Add(currentB);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetTriangles(trianglesIndices, 0);
        mesh.SetUVs(0, uvs);


    }


    float GetApproxLength(int precision = 8)
    {
        Vector3[] points = new Vector3[precision];


        for (int i = 0; i < precision; i++)
        {
            float t = i / (precision - 1);
            points[i] = GetBezierPoint(t).pos;
        }

            float dist = 0;

        for (int i = 0; i < precision - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];

            float t = i / (precision - 1);

            dist += Vector3.Distance(a,b);

        }

        return dist;


    }

    private void Update()
    {
        GenerateMesh();
    }

    OrientedPoint GetBezierPoint(float t)
    {
        Vector3 p0 = GetPoint(0);
        Vector3 p1 = GetPoint(1);
        Vector3 p2 = GetPoint(2);
        Vector3 p3 = GetPoint(3);


        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);


        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 tangent = (e - d).normalized;
        Vector3 up = Vector3.Lerp(startPoint.up,endPoint.up,t).normalized;

        Quaternion rot = Quaternion.LookRotation(tangent,up);
        return new OrientedPoint()
        {
            pos = Vector3.Lerp(d, e, t),
            rot = rot
        };

    }





    void OnDrawGizmos()
    {

        

        Handles.DrawBezier(
            GetPoint(0),
             GetPoint(3),
              GetPoint(1),
               GetPoint(2),
               Color.white,
               EditorGUIUtility.whiteTexture, 1f);

        OrientedPoint testPoint = GetBezierPoint(tTest);




        Gizmos.DrawSphere(testPoint.pos, 0.05f);

        Handles.PositionHandle(testPoint.pos, testPoint.rot);


        float radius = 0.03f;
        void DrawPoint(Vector2 localPos) => Gizmos.DrawSphere(testPoint.LocalToWorldPos(localPos), 0.15f);


        Vector3[] verts = shape2D.vertices.Select(v => testPoint.LocalToWorldPos(v.point)).ToArray();
        for (int i = 0; i < shape2D.lineIndices.Length; i += 2)
        {
            Vector3 a = verts[shape2D.lineIndices[i]];
            Vector3 b = verts[shape2D.lineIndices[i + 1]];

            Gizmos.DrawLine(a, b);
        }
    }


}




public struct OrientedPoint
{
    public Vector3 pos;
    public Quaternion rot;


    public Vector3 LocalToWorldPos(Vector3 localSpace)
    {

        return pos + rot * localSpace;
    }
    public Vector3 LocalSpace(Vector3 localSpace)
    {

        return pos + rot * localSpace;
    }
    public Vector3 LocalToWorldVector(Vector3 localSpace)
    {

        return rot * localSpace;
    }
}