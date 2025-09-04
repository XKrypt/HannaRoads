using System.Collections.Generic;
using System.Linq;
using HannaRoads;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteAlways]
public class SideWalkOld : MonoBehaviour
{

    public RSegment rSegment;
    public HannaIntersection hannaIntersection;

    public HannaCurbstone hannaCurbstone;

    public int resolution = 32;
    public int slices = 32;
    Mesh mesh;
    public float streetGuideHeight;
    public float streetGuideWidth;

    public AnimationCurve downHorizontalCurve;
    public float startDown;

    public Vector3 offset;


    public List<DownPoint> downPoints = new List<DownPoint>();



    public List<Vector3> verticesShow = new List<Vector3>();

    private void Update() {
        if (hannaCurbstone != null)
        {
            downPoints = hannaCurbstone.downPoints;
        }
    }


    [ContextMenu("HannaSideWalk/GenerateSideWalk")]
    public void GenerateSideWalk()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "curbstone";
        }
        mesh.Clear();
        verticesShow.Clear();


        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        // List<int> triangles = new List<int>();

        // Lista para armazenar o 't' para cada anel de vértices
        List<float> tPoints = new List<float>();

        // 1. Gere os pontos 't' para a resolução global
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            tPoints.Add(t);
        }

        // 2. Adicione os pontos 't' dos DownPoints
        foreach (var downPoint in downPoints)
        {
            float startT = downPoint.tPosition - downPoint.size / 2;
            float endT = downPoint.tPosition + downPoint.size / 2;

            // Adicione os pontos da rampa à lista
            for (int i = 0; i <= downPoint.resolution; i++)
            {
                float t = Mathf.Lerp(startT, endT, i / (float)downPoint.resolution);
                tPoints.Add(t);
            }
        }

        // 3. Ordene a lista de 't' para garantir a ordem correta
        tPoints = tPoints.Distinct().OrderBy(t => t).ToList();


        // 4. Gere os vértices e normais para cada ponto 't'
        for (int i = 0; i < tPoints.Count; i++)
        {
            bool isInDownPoint = false;
            float t = tPoints[i];
            float currentHeight = streetGuideHeight;

            // Verifique se o 't' está dentro de um DownPoint para ajustar a altura
            foreach (var downPoint in downPoints)
            {
                float startT = downPoint.tPosition - downPoint.size / 2;
                float endT = downPoint.tPosition + downPoint.size / 2;

                if (t >= startT && t <= endT)
                {
                    float tInCurve = Mathf.InverseLerp(startT, endT, t);
                    currentHeight *= 1 - downPoint.down.Evaluate(tInCurve);
                    isInDownPoint = true;
                    break; // Sai do loop para evitar múltiplas avaliações
                }
            }

            GenerateRing(vertices, normals, uvs, t, currentHeight, isInDownPoint);
        }


        // List<Vector3> vertices = new List<Vector3>();
        // List<Vector3> normals = new List<Vector3>();

        // float t = 0;
        // if (downPoints.Count > 0)
        // {
        //     int resolutionPerRing = resolution / downPoints.Count;
        //     foreach (var downPoint in downPoints)
        //     {
        //         float startPoint = downPoint.tPosition - downPoint.size;
        //         while (t <= startPoint)
        //         {

        //             GenerateRing(vertices, normals, t);

        //             if (t == startPoint)
        //             {
        //                 break;
        //             }

        //             t += 1 / (float)resolutionPerRing;

        //             if (t > startPoint)
        //             {
        //                 t = startPoint;
        //             }
        //         }
        //     }
        // }
        // else
        // {
        //     while (t <= 1)
        //     {

        //         GenerateRing(vertices, normals, t);

        //         if (t == 1)
        //         {
        //             break;
        //         }

        //         t += 1 / (float)resolution;

        //         if (t > 1)
        //         {
        //             t = 1;
        //         }
        //     }
        // }


        //Connect Edges

        List<int> triangles = new List<int>();
        for (int ring = 0; ring < tPoints.Count - 1; ring++)
        {

            for (int v = 0; v < slices - 1; v++)
            {
                int indexA = ring * slices + v;
                int indexB = (ring + 1) * slices + v;
                int indexC = indexA + 1;
                int indexD = indexB + 1;

                triangles.Add(indexA);
                triangles.Add(indexB);
                triangles.Add(indexC);

                triangles.Add(indexC);
                triangles.Add(indexB);
                triangles.Add(indexD);
            }

        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0,uvs);
        //mesh.SetNormals(normals);
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;

    }

    public int hannaIntersectionLSideIndex = 0;

    void GenerateRing(List<Vector3> vertices, List<Vector3> normals,List<Vector2> uvs,  float t, float currentHeight, bool isInDownPoint = false)
    {
        OrientedPoint point = new OrientedPoint();
        if (rSegment != null)
        {
            point = rSegment.GetBezierPoint(t);

        }
        if (hannaIntersection != null)
        {
            point = hannaIntersection.BevelSideOrientedPoint(hannaIntersection.intersections[hannaIntersectionLSideIndex], t);
        }

        Vector3 left = Vector3.left * streetGuideWidth;
        //Vector3 right = Vector3.right * streetGuideWidth;


        Vector3 up = Vector3.down * currentHeight / 2;
        //Vector3 down = Vector3.down * streetGuideWidth / 2;



        for (int i = 0; i < slices; i++)
        {
            float tSlice = i / (float)slices;
            Vector3 vertexPoint = Vector3.Lerp(left, Vector3.zero, tSlice);
            float evaluate = downHorizontalCurve.Evaluate(tSlice);

            Vector3 upLerp = isInDownPoint ? Vector3.Lerp(up, Vector3.zero * streetGuideHeight / 2, evaluate) : Vector3.zero;

            vertices.Add(point.LocalSpace(vertexPoint + upLerp + offset));
            normals.Add(Vector3.up);
            verticesShow.Add(point.LocalSpace(vertexPoint + upLerp + offset));
            uvs.Add(new Vector2(tSlice,t));
        }

    }
    void GenerateRing(List<Vector3> vertices, List<Vector3> normals, float t, DownPoint downPoint, float downPointT)
    {
        OrientedPoint point = rSegment.GetBezierPoint(t);

        Vector3 left = Vector3.left * streetGuideWidth / 2;
        Vector3 right = Vector3.right * streetGuideWidth / 2;


        Vector3 up = Vector3.up * streetGuideHeight / 2 * downPoint.down.Evaluate(downPointT);
        Vector3 down = Vector3.down * streetGuideHeight / 2;


        verticesShow.Add(point.LocalSpace(left + up));
        verticesShow.Add(point.LocalSpace(right + up));

        verticesShow.Add(point.LocalSpace(left + down));
        verticesShow.Add(point.LocalSpace(right + down));

        vertices.Add(point.LocalSpace(left + up));
        vertices.Add(point.LocalSpace(right + up));

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);


        vertices.Add(point.LocalSpace(right + up));
        vertices.Add(point.LocalSpace(right + down));

        normals.Add(Vector3.right);
        normals.Add(Vector3.right);


        vertices.Add(point.LocalSpace(right + down));
        vertices.Add(point.LocalSpace(left + down));

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);


        vertices.Add(point.LocalSpace(left + down));
        vertices.Add(point.LocalSpace(left + up));
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);

    }


    public void GenerateDownPoints(DownPoint downPoint, List<Vector3> vertices, List<Vector3> normals)
    {
        float init = downPoint.tPosition - downPoint.size / 2;
        float end = downPoint.tPosition + downPoint.size / 2;

        for (int i = 0; i < downPoint.resolution; i++)
        {
            float t = Mathf.Lerp(init, end, i / (float)resolution);

            GenerateRing(vertices, normals, t, downPoint, i / (float)resolution);

        }
    }

    [SerializeField] bool enableGizmos = false;
    private void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        Gizmos.color = Color.magenta;
        foreach (var vertex in verticesShow)
        {
            Gizmos.DrawCube(transform.TransformPoint(vertex), Vector3.one * 0.05f);
        }
        Gizmos.color = Color.white;


        // Vector3 a = hannaIntersection.intersections[hannaIntersectionLSideIndex].subPoint1;
        // Vector3 b = hannaIntersection.intersections[hannaIntersectionLSideIndex].mainPoint;
        // Vector3 c = hannaIntersection.intersections[hannaIntersectionLSideIndex].subPoint2;


        // Gizmos.DrawSphere(transform.TransformPoint(a), 0.05f);
        // Gizmos.DrawSphere(transform.TransformPoint(b), 0.05f);
        // Gizmos.DrawSphere(transform.TransformPoint(c), 0.05f);
    }

}
