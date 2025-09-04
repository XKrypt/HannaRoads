using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;


namespace HannaRoads
{
    public class SideWalkSegment : MonoBehaviour
    {
        public Mesh _mesh;

        public MeshFilter meshFilter;


        public List<HannaCurbstone> hannaCurbstones = new List<HannaCurbstone>();

        public HannaSidewalk hannaSideWalkEditor;
        public List<SidewalkControlPoint> controlPoints = new List<SidewalkControlPoint>();


        public List<RoadObject> objectsAlongRoad = new List<RoadObject>();
        [SerializeField] public List<RoadLine> roadLines = new List<RoadLine>();
        public Transform start;

        public Transform end;
        public int detailLevel = 10;


        public SideWalkReferencePoint startRef;
        public SideWalkReferencePoint endRef;

        public int sliceResolution = 2;

        public int resolution = 32;
        public int slices = 32;
        Mesh mesh;
        public float sideWalkHeight = 1f;
        public float sideWalkWidth = 1f;

        public AnimationCurve downHorizontalCurve;

        public Vector3 offset;


        public List<DownPoint> downPoints = new List<DownPoint>();



        public List<Vector3> verticesShow = new List<Vector3>();

        private void Update()
        {

        }


        public void AddCurbstone()
        {
            GameObject curbstone = new GameObject();

            curbstone.transform.SetParent(transform);
            curbstone.transform.localPosition = Vector3.zero;

            curbstone.AddComponent<MeshFilter>();
            curbstone.AddComponent<MeshRenderer>();
            HannaCurbstone hannaCurbstone = curbstone.AddComponent<HannaCurbstone>();
            hannaCurbstone.sSegment = this;
            hannaCurbstones.Add(hannaCurbstone);



        }



        public float GetWidthInT(float t)
        {
            float nextWidth = endRef.sSegment != null ? endRef.sSegment.sideWalkWidth : sideWalkWidth;
            return Mathf.Lerp(sideWalkWidth, nextWidth, t); ;
        }




        [ContextMenu("HannaSideWalk/GenerateSideWalk")]
        public void GenerateSideWalk()
        {
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "sidewalk";
                if (GetComponent<MeshRenderer>().sharedMaterial == null)
                {
                    GetComponent<MeshRenderer>().sharedMaterial = hannaSideWalkEditor.curbstoneDefaultMaterial;
                }
            }
            mesh.Clear();
            verticesShow.Clear();


            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

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
                float currentHeight = sideWalkHeight;

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
            mesh.SetUVs(0, uvs);
            //mesh.SetNormals(normals);
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().sharedMesh = mesh;

        }



        public int hannaIntersectionLSideIndex = 0;

        void GenerateRing(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, float t, float currentHeight, bool isInDownPoint = false)
        {
            OrientedPoint point = new OrientedPoint();

            point = GetBezierPoint(t);

            if (t == 1 && endRef.sSegment != null)
            {
                point = endRef.sSegment.GetBezierPoint(0);
            }

            float nextWidth = endRef.sSegment != null ? endRef.sSegment.sideWalkWidth : sideWalkWidth;
            Vector3 nextOffset = endRef.sSegment != null ? endRef.sSegment.offset : offset;
            float currentWidth = Mathf.Lerp(sideWalkWidth, nextWidth, t);
            Vector3 left = Vector3.left * (currentWidth / 2);
            Vector3 right = Vector3.right * (currentWidth / 2);
            Vector3 currentOffset = Vector3.Lerp(offset, nextOffset, t);
            Vector3 up = Vector3.down * currentHeight / 2;


            if (downHorizontalCurve == null)
            {
                downHorizontalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }



            for (int i = 0; i < slices; i++)
            {
                float tSlice = i / (float)slices;
                Vector3 vertexPoint = Vector3.Lerp(left, right, tSlice);
                float evaluate = downHorizontalCurve.Evaluate(tSlice);

                Vector3 upLerp = isInDownPoint ? Vector3.Lerp(up, Vector3.zero * sideWalkHeight / 2, evaluate) : Vector3.zero;
                if (t == 1 && endRef.sSegment != null)
                {
                    SideWalkSegment sideWalkSegment = endRef.sSegment;
                    Vector3 worldSpace = sideWalkSegment.transform.TransformPoint(point.LocalSpace(vertexPoint + upLerp + currentOffset));
                    vertices.Add(transform.InverseTransformPoint(worldSpace));
                }
                else
                {
                    vertices.Add(point.LocalSpace(vertexPoint + upLerp + currentOffset));
                }

                normals.Add(Vector3.up);
                verticesShow.Add(point.LocalSpace(vertexPoint + upLerp + currentOffset));
                uvs.Add(new Vector2(tSlice, t));
            }

        }

        [SerializeField] bool enableGizmos = false;

        private void OnDrawGizmos()
        {
            if (!enableGizmos) return;
            Handles.DrawBezier(start.position, end.position,
            controlPoints[0].transform.position,
            controlPoints[1].transform.position,
            Color.white,
            EditorGUIUtility.whiteTexture, 2f
            );
            Handles.color = Color.white;




        }


        [ContextMenu("RSegment/GenerateMesh")]
        public void Generate()
        {
            if (detailLevel > 1)
            {
                GenerateSideWalk();

            }

            foreach (var curbstone in hannaCurbstones)
            {
                curbstone.GenerateCurbstone();
            }

        }

        float CalculateSpan(Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).magnitude;
        }

        [ContextMenu("RSegment/ResetMesh")]
        public void ResetMesh()
        {
            _mesh = null;
        }


        void OnDestroy()
        {

            // //hannaRoad.rSegments.Remove(this);

            // if (startRef.previousRSegment == null && startRef != null)
            // {
            //     DestroyImmediate(startRef.gameObject);
            // }
            // else
            // {
            //     startRef.rSegment = null;
            //     startRef.UpdatePositions();
            //     startRef.UpdateMeshVerts();
            // }

            // if (endRef.rSegment == null && endRef != null)
            // {
            //     DestroyImmediate(endRef.gameObject);
            // }
            // else
            // {
            //     endRef.previousRSegment = null;
            //     endRef.segmentType = SegmentType.Start;
            //     endRef.UpdatePositions();
            //     endRef.UpdateMeshVerts();

            // }



        }

        // public Vector3[] GetFirstVertices()
        // {
        //     return new Vector3[] { transform.TransformPoint(_mesh.vertices[0]), transform.TransformPoint(_mesh.vertices[1]) };
        // }

        public Vector3[] GetFirstVertices()
        {
            Vector3[] vertices = new Vector3[sliceResolution];

            for (int i = 0; i < sliceResolution - 1; i++)
            {
                vertices[i] = transform.TransformPoint(_mesh.vertices[i]);
            }
            return vertices;
        }
        public Vector3[] GetLastVertices()
        {
            Vector3[] vertices = new Vector3[sliceResolution];

            for (int i = _mesh.vertices.Length - sliceResolution; i < _mesh.vertices.Length - 1; i++)
            {
                vertices[i] = transform.TransformPoint(_mesh.vertices[i]);
            }
            return vertices;
        }

        public void SetLastVertices(Vector3[] vertices)
        {
            if (_mesh == null) return;
            List<Vector3> verts = _mesh.vertices.ToList();
            int startIndex = verts.Count() - vertices.Length;

            for (int i = 0; i < vertices.Length; i++)
            {
                verts[startIndex + i] = transform.InverseTransformPoint(vertices[i]);
            }

            _mesh.SetVertices(verts);
        }

        public void SetFirstVertices(Vector3[] vertices)
        {
            if (_mesh == null) return;
            List<Vector3> verts = _mesh.vertices.ToList();

            for (int i = 0; i < vertices.Length; i++)
            {
                verts[i] = transform.InverseTransformPoint(vertices[i]);
            }

            _mesh.SetVertices(verts);
        }


        public float GetApproxLength(int precision = 8)
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

                dist += Vector3.Distance(a, b);

            }

            return dist;


        }



        public OrientedPoint GetBezierPoint(float t)
        {
            Vector3 p0 = start.transform.localPosition;
            Vector3 p1 = controlPoints[0].transform.localPosition;
            Vector3 p2 = controlPoints[1].transform.localPosition;
            Vector3 p3 = end.transform.localPosition;


            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            Vector3 c = Vector3.Lerp(p2, p3, t);


            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            Vector3 tangent = (e - d).normalized;
            Vector3 up = Vector3.Lerp(start.up, end.up, t).normalized;

            Quaternion rot = Quaternion.LookRotation(tangent, up);
            return new OrientedPoint()
            {
                pos = Vector3.Lerp(d, e, t),
                rot = rot
            };

        }
        public OrientedPoint GetBezierPointGlobal(float t)
        {
            Vector3 p0 = start.transform.position;
            Vector3 p1 = controlPoints[0].transform.position;
            Vector3 p2 = controlPoints[1].transform.position;
            Vector3 p3 = end.transform.position;


            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            Vector3 c = Vector3.Lerp(p2, p3, t);


            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            Vector3 tangent = (e - d).normalized;
            Vector3 up = Vector3.Lerp(start.up, end.up, t).normalized;

            Quaternion rot = Quaternion.LookRotation(tangent, up);
            return new OrientedPoint()
            {
                pos = Vector3.Lerp(d, e, t),
                rot = rot
            };

        }
    }

}