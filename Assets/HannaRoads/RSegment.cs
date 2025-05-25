using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace HannaRoads
{

    [ExecuteInEditMode]

    public class RSegment : MonoBehaviour
    {

        public Mesh _mesh;
        public HannaRoad hannaRoad;
        public MeshFilter meshFilter;
        public List<ControlPoint> controlPoints = new List<ControlPoint>();

        public HannaIntersection endIntersection;
        public HannaIntersection startIntersection;

        public bool ignoreFromRoadSystemTerrainUpdate;


        public List<RoadObject> objectsAlongRoad = new List<RoadObject>();
        [SerializeField] public List<RoadLine> roadLines = new List<RoadLine>();

        public float startOffset = 0;
        public float endOffset = 1;
        public AnimationCurve widthCurve;
        public AnimationCurve verticalProfile;
        public Transform start;
        public float verticalProfileMultiplayer = 0f;
        public Transform end;

        public Material defaultRoadMaterial;

        public float width = 1;
        public int detailLevel = 10;


        public ReferencePoint startRef;
        public ReferencePoint endRef;



        public AnimationCurve terrainAlignCurve;
        public float terrainAlignRadius = 5f;
        public float maxAlignDistance = 5f;
        public float minAlignDistance = .5f;
        public float terrainBottomMargin = .05f;

        public int sliceResolution = 2;

        public List<Vector3> vertsWorldPos = new List<Vector3>();
        public List<Terrain> terrainsBellowRoad = new List<Terrain>();
        private void OnDrawGizmos()
        {
            Handles.DrawBezier(start.position, end.position,
            controlPoints[0].transform.position,
            controlPoints[1].transform.position,
            Color.white,
            EditorGUIUtility.whiteTexture, 2f
            );

            OrientedPoint orientedPoint = GetBezierPointGlobal(0);

            Handles.DrawLine(orientedPoint.LocalSpace(Vector3.right * ((width / 2) + terrainAlignRadius)), orientedPoint.LocalSpace(Vector3.left * ((width / 2) + terrainAlignRadius)), 2.5f);

            Handles.color = Color.white;




        }
        [ContextMenu("RSegment/GenerateMesh")]
        public void Generate()
        {
            if (detailLevel > 1)
            {

                if (_mesh == null)
                {
                    _mesh = new Mesh()
                    {
                        name = "RoadSegment"
                    };
                    meshFilter.sharedMesh = _mesh;
                }
                GenerateMeshNVerticesWay(_mesh);
            }

            foreach (var item in roadLines)
            {
                GenerateRoadLine(ref item.mesh, item);
            }
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            if (isFirstTime)
            {
                meshRenderer.material = defaultRoadMaterial;
                isFirstTime = false;
            }
        }

        bool isFirstTime = true;

        float CalculateSpan(Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).magnitude;
        }

        [ContextMenu("RSegment/ResetMesh")]
        public void ResetMesh()
        {
            _mesh = null;
        }

        public int lastDetailLevel;

        private void Update()
        {

        }


        public void AlignTerrain()
        {
            isAligningTerrain = true;
            EditorCoroutineUtility.StartCoroutineOwnerless(AlignTerrainOperation());
        }
        public bool isAligningTerrain = false;


        public float alignTerrainProgress = 0;
        IEnumerator AlignTerrainOperation()
        {

            List<Terrain> terrainsToOperate = FindObjectsByType<Terrain>(FindObjectsSortMode.None).ToList();

            List<ThreadSegment> threads = new List<ThreadSegment>();


            int currentTerrain = 0;


            while (currentTerrain < terrainsToOperate.Count)
            {
                Terrain item = terrainsToOperate[currentTerrain];

                ThreadSegment threadSegment = new ThreadSegment();
                if (item.GetComponent<HannaTerrainController>() == null)
                {
                    item.AddComponent<HannaTerrainController>();
                }

                threadSegment.hanna = item.GetComponent<HannaTerrainController>();

                //Get terrain settings first
                threadSegment.data = item.terrainData;
                Vector3 terrainSizeData = threadSegment.data.size;
                int heightmapRes = threadSegment.data.heightmapResolution;


                //Generate data to process
                threadSegment.baseHeights = threadSegment.data.GetHeights(0, 0, heightmapRes, heightmapRes);
                threadSegment.accumulatedHeights = (float[,])threadSegment.baseHeights.Clone(); // start from current
                Vector3 terrainPosition = item.transform.position;

                // The amount of postions of terrain affected;
                threadSegment.affected = new bool[heightmapRes, heightmapRes];

                //List of oriented points to process along the road (bezier curve)
                OrientedPoint[] orientedPoints = new OrientedPoint[201];

                for (int i = 0; i < orientedPoints.Length; i++)
                {
                    float t = i / 201f;
                    orientedPoints[i] = GetBezierPointGlobal(t);
                }
                int threadRunning = NumberOfThreadsRunning(threads);
                if (threadRunning >= hannaRoad.maxThreadsPerSegment)
                {

                    yield return null;
                }

                //Create thread to process terrain data.
                Thread thread = new Thread(() =>
                {
                    for (int i = 0; i <= 200; i++)
                    {
                        float t = i / 200f;
                        OrientedPoint point = orientedPoints[i];
                        threadSegment.hanna.RampTerrainAlongBezier(threadSegment.accumulatedHeights, threadSegment.affected, terrainAlignRadius, point, terrainBottomMargin, terrainSizeData, heightmapRes, terrainPosition);
                        alignTerrainProgress += 100f / (terrainsToOperate.Count * 200f);
                    }

                    // Apply only where affected
                    for (int x = 0; x < heightmapRes; x++)
                    {
                        for (int z = 0; z < heightmapRes; z++)
                        {
                            if (threadSegment.affected[z, x])
                            {
                                threadSegment.baseHeights[z, x] = threadSegment.accumulatedHeights[z, x];
                            }
                        }
                    }
                });

                //Add to list

                threadSegment.thread = thread;
                threads.Add(threadSegment);
                thread.Start();

                currentTerrain++;
                yield return null;
            }


            //While have threads to process the coroutine will not stop;
            while (DetectIfAreThreadsRunning(threads))
            {
                yield return null;
            }
            ApplyResults(threads);
            isAligningTerrain = false;
            alignTerrainProgress = 0;



        }


        void ApplyResults(List<ThreadSegment> threads)
        {
            foreach (var item in threads)
            {
                item.data.SetHeights(0, 0, item.baseHeights);
            }
        }

        int NumberOfThreadsRunning(List<ThreadSegment> threads)
        {
            int count = 0;

            foreach (var item in threads)
            {
                if (item.thread.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        bool DetectIfAreThreadsRunning(List<ThreadSegment> threads)
        {
            foreach (var thread in threads)
            {
                if (thread.thread.IsAlive) return true;
            }

            return false;
        }
        void GenerateMesh(Mesh mesh)
        {


            if (mesh == null)
            {

                mesh = new Mesh() { name = "RoadSegment" };

                meshFilter.sharedMesh = mesh;

            }

            mesh.Clear();

            vertsWorldPos.Clear();

            if (widthCurve == null)
            {
                widthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }

            //Generate vertices
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();


            int resolution = detailLevel;

            for (int i = 0; i < resolution + 1; i++)
            {
                float t = i / (float)(resolution);
                OrientedPoint bezierPoint = GetBezierPoint(t);

                float meshWidth = width;


                if (endRef.rSegment != null && t >= startOffset && t <= endOffset)
                {
                    // Normaliza o t entre startOffset e endOffset para gerar um valor de 0 a 1
                    float offsetLerp = Mathf.InverseLerp(startOffset, endOffset, t);

                    // Avalia a curva com esse valor normalizado
                    float curveValue = widthCurve.Evaluate(offsetLerp);

                    // Interpola entre largura atual e a próxima
                    meshWidth = Mathf.Lerp(width, endRef.rSegment.width, curveValue);
                }
                else if (t > endOffset)
                {
                    // Já passou do blend: adota a largura final
                    meshWidth = endRef.rSegment.width;
                }
                else
                {
                    // Ainda antes do blend: mantém a largura original
                    meshWidth = width;
                }


                Vector3 rightVertex = Vector3.right * (meshWidth / 2);
                Vector3 leftVertex = Vector3.left * (meshWidth / 2);
                Vector3 vA = bezierPoint.LocalSpace(rightVertex);
                Vector3 vB = bezierPoint.LocalSpace(leftVertex);

                vertices.Add(vA);
                vertices.Add(vB);


                vertsWorldPos.Add(transform.TransformPoint(vA));
                vertsWorldPos.Add(transform.TransformPoint(vB));

                normals.Add(bezierPoint.LocalToWorldVector(Vector3.up));
                normals.Add(bezierPoint.LocalToWorldVector(Vector3.up));

                uvs.Add(new Vector2(0, t * GetApproxLength(resolution) / CalculateSpan(rightVertex, leftVertex)));
                uvs.Add(new Vector2(1, t * GetApproxLength(resolution) / CalculateSpan(rightVertex, leftVertex)));
            }


            List<int> triangles = new List<int>();
            //Generate triangles
            for (int i = 0; i < (resolution * 2) - 1; i++)
            {

                int rootVertex = i;
                triangles.Add(rootVertex + 2);
                triangles.Add(rootVertex);
                triangles.Add(rootVertex + 1);



                triangles.Add(rootVertex + 3);
                triangles.Add(rootVertex + 2);
                triangles.Add(rootVertex + 1);

            }




            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);

            if (startRef != null)
            {
                startRef.UpdateMeshVerts();
            }

            if (endRef != null)
            {
                endRef.UpdateMeshVerts();
            }

        }


        void GenerateMeshNVerticesWay(Mesh mesh)
        {

            terrainsBellowRoad.Clear();
            if (mesh == null)
            {

                mesh = new Mesh() { name = "RoadSegment" };
                meshFilter.sharedMesh = mesh;
            }

            mesh.Clear();

            vertsWorldPos.Clear();

            if (widthCurve == null)
            {
                widthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
            if (verticalProfile == null)
            {
                verticalProfile = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }

            if (sliceResolution < 2)
            {
                sliceResolution = 2;
            }

            //Generate vertices
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();


            int resolution = detailLevel;

            float uSpan = CalculateSpan(Vector3.left * (width / 2f), Vector3.right * (width / 2f));
            for (int i = 0; i < resolution + 1; i++)
            {
                float t = i / (float)(resolution);
                OrientedPoint bezierPoint = GetBezierPoint(t);

                float meshWidth = width;



                if (endRef.rSegment != null && t >= startOffset && t <= endOffset)
                {
                    // Normaliza o t entre startOffset e endOffset para gerar um valor de 0 a 1
                    float offsetLerp = Mathf.InverseLerp(startOffset, endOffset, t);

                    // Avalia a curva com esse valor normalizado
                    float curveValue = widthCurve.Evaluate(offsetLerp);

                    // Interpola entre largura atual e a próxima
                    meshWidth = Mathf.Lerp(width, endRef.rSegment.width, curveValue);
                }
                else if (t > endOffset)
                {
                    // Já passou do blend: adota a largura final
                    meshWidth = endRef.rSegment.width;
                }
                else
                {
                    // Ainda antes do blend: mantém a largura original
                    meshWidth = width;
                }


                for (int s = 0; s < sliceResolution; s++)
                {
                    float sliceT = s / (float)(sliceResolution - 1); // de 0 (left) até 1 (right)
                    float verticalProfileEnd = verticalProfile.Evaluate(sliceT);
                    float finalVerticalMultiplier = verticalProfileMultiplayer;
                    if (endRef.rSegment != null)
                    {
                        float A = verticalProfile.Evaluate(sliceT);
                        float B = endRef.rSegment.verticalProfile.Evaluate(sliceT);
                        verticalProfileEnd = Mathf.Lerp(A, B, t);
                        finalVerticalMultiplier = Mathf.Lerp(verticalProfileMultiplayer, endRef.rSegment.verticalProfileMultiplayer, t);
                    }

                    // Largura total da pista:
                    float totalWidth = meshWidth;

                    // Posição local no plano horizontal da estrada
                    Vector3 lateral = Vector3.Lerp(Vector3.left, Vector3.right, sliceT) * (totalWidth / 2f);

                    // Se quiser curvar no Y, adicione um offset aqui com curva


                    float verticalOffset = verticalProfileEnd * finalVerticalMultiplier;
                    lateral.y += verticalOffset;

                    Vector3 worldPoint = bezierPoint.LocalSpace(lateral);
                    vertices.Add(worldPoint);

                    normals.Add(bezierPoint.LocalToWorldVector(Vector3.up)); // ou normal baseada na inclinação
                    uvs.Add(new Vector2(sliceT, t * GetApproxLength(resolution) / uSpan)); // UVs bem mapeados
                }

                // Vector3 rightVertex = Vector3.right * (terrainAlignRadius / 2);
                // Vector3 leftVertex = Vector3.left * (terrainAlignRadius / 2);
                // Vector3 vA = bezierPoint.LocalSpace(rightVertex);
                // Vector3 vB = bezierPoint.LocalSpace(leftVertex);

                // DetectTerrain(vA);
                // DetectTerrain(vB);
            }


            List<int> triangles = new List<int>();
            //Generate triangles
            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < sliceResolution - 1; j++)
                {
                    int indexA = i * sliceResolution + j;
                    int indexB = (i + 1) * sliceResolution + j;
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
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);

            if (startRef != null)
            {
                startRef.UpdateMeshVerts();
            }

            if (endRef != null)
            {
                endRef.UpdateMeshVerts();
            }


        }

        public void DetectTerrain(Vector3 point)
        {
            Vector3 transformPoint = transform.TransformPoint(point);
            RaycastHit hit;
            if (Physics.Raycast(transformPoint, Vector3.down, out hit))
            {
                if (!terrainsBellowRoad.Contains(hit.collider.GetComponent<Terrain>()))
                {
                    terrainsBellowRoad.Add(hit.collider.GetComponent<Terrain>());
                }
            }
        }




        public void GenerateRoadLine(ref Mesh mesh, RoadLine segmentMesh)
        {

            if (mesh == null)
            {

                mesh = new Mesh() { name = "RoadSegment" };

                meshFilter.sharedMesh = mesh;

            }

            mesh.Clear();


            if (segmentMesh.widthProfile == null)
            {
                segmentMesh.widthProfile = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
            if (segmentMesh.verticalProfile == null)
            {
                segmentMesh.verticalProfile = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }

            //Generate vertices
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();


            int resolution = segmentMesh.detailLevel;

            float deltaT = segmentMesh.end - segmentMesh.start;

            int localResolution = Mathf.RoundToInt(resolution * deltaT);

            float uSpan = CalculateSpan(Vector3.left * (width / 2f), Vector3.right * (width / 2f));

            for (int i = 0; i < localResolution + 1; i++)
            {
                float t = Mathf.Lerp(segmentMesh.start, segmentMesh.end, i / (float)localResolution);

                if (t < segmentMesh.start) continue;
                if (t > segmentMesh.end) continue;


                OrientedPoint bezierPoint = GetBezierPoint(t);

                float meshWidth = segmentMesh.width;


                if (endRef.rSegment != null && t >= startOffset && t <= endOffset)
                {
                    // Normaliza o t entre startOffset e endOffset para gerar um valor de 0 a 1
                    float offsetLerp = Mathf.InverseLerp(startOffset, endOffset, t);

                    // Avalia a curva com esse valor normalizado
                    float curveValue = segmentMesh.widthProfile.Evaluate(offsetLerp);

                    // Interpola entre largura atual e a próxima
                    meshWidth = Mathf.Lerp(segmentMesh.width, segmentMesh.width, curveValue);
                }
                else if (t > endOffset)
                {
                    // Já passou do blend: adota a largura final
                    meshWidth = endRef.rSegment.width;
                }
                else
                {
                    // Ainda antes do blend: mantém a largura original
                    meshWidth = segmentMesh.width;
                }

                float halfWidth = meshWidth / 2f;
                float offset = segmentMesh.horizontalOffset;


                for (int s = 0; s < segmentMesh.sliceResolution; s++)
                {
                    float sliceT = s / (float)(segmentMesh.sliceResolution - 1); // de 0 (left) até 1 (right)

                    // Largura total da pista:
                    float totalWidth = meshWidth;

                    // Posição local no plano horizontal da estrada
                    Vector3 lateral = Vector3.Lerp(Vector3.left, Vector3.right, sliceT) * (totalWidth / 2f) + (Vector3.right * offset);

                    // Se quiser curvar no Y, adicione um offset aqui com curva
                    float verticalOffset = segmentMesh.verticalProfile.Evaluate(sliceT) * segmentMesh.verticalProfileMultiplayer; // curva de perfil da estrada (valeta, barranco)
                    lateral.y += verticalOffset;

                    Vector3 worldPoint = bezierPoint.LocalSpace(lateral) + (Vector3.up * segmentMesh.verticalOffset);

                    vertices.Add(worldPoint);

                    normals.Add(bezierPoint.LocalToWorldVector(Vector3.up)); // ou normal baseada na inclinação
                    uvs.Add(new Vector2(sliceT, t * GetApproxLength(resolution) / uSpan)); // UVs bem mapeados
                }



            }


            List<int> triangles = new List<int>();
            //Generate triangles
            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < segmentMesh.sliceResolution - 1; j++)
                {
                    int indexA = i * segmentMesh.sliceResolution + j;
                    int indexB = (i + 1) * segmentMesh.sliceResolution + j;
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
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);

            if (startRef != null)
            {
                startRef.UpdateMeshVerts();
            }

            if (endRef != null)
            {
                endRef.UpdateMeshVerts();
            }


        }




        void OnDestroy()
        {

            hannaRoad.rSegments.Remove(this);

            if (startRef.previousRSegment == null && startRef != null)
            {
                DestroyImmediate(startRef.gameObject);
            }
            else
            {
                startRef.rSegment = null;
                startRef.UpdatePositions();
                startRef.UpdateMeshVerts();
            }

            if (endRef.rSegment == null && endRef != null)
            {
                DestroyImmediate(endRef.gameObject);
            }
            else
            {
                endRef.previousRSegment = null;
                endRef.segmentType = SegmentType.Start;
                endRef.UpdatePositions();
                endRef.UpdateMeshVerts();

            }



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

    struct SegmentMesh
    {
        public float startOffset;
        public float endOffset;
        public AnimationCurve widthCurve;
        public float start;
        public float end;

        public float horizontalOffset;

        public float width;
        public int detailLevel;

        public Mesh mesh;
        public MeshRenderer meshRenderer;

        public AnimationCurve verticalProfile;
    }


}

public class ThreadSegment
{
    public Terrain terrain;
    public TerrainData data;

    public Thread thread;

    public float[,] baseHeights;
    public float[,] accumulatedHeights;
    public bool[,] affected;


    public HannaTerrainController hanna;


}