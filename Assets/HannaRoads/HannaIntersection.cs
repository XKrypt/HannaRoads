using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using HannaRoads;
using UnityEngine;

namespace HannaRoads
{

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class HannaIntersection : MonoBehaviour
    {
        public IntersectionAttachment[] intersectionPoints = new IntersectionAttachment[4];
        public float size = 1f;
        public float uvSize = 1f;

        public float extrusionSize = 1f;

        public HannaRoad hannaRoad;

        [SerializeField] MeshFilter meshFilter;
        [SerializeField] Mesh mesh;


        public int resolutionCurve = 3;
        public float shape = 1;

        public bool crossing4 = true;

        public List<Vector3> wordVertices = new List<Vector3>();


        public AnimationCurve terrainAlignCurve;
        public float terrainAlignRadius = 5f;
        public float maxAlignDistance = 5f;
        public float minAlignDistance = .5f;
        public float terrainBottomMargin = .05f;
        [SerializeField] List<LSideIntersection> intersections = new List<LSideIntersection>();

        public void Generate()
        {
            SetVerticesEdgePoints();
            if (mesh == null || meshFilter != null)
            {
                meshFilter = GetComponent<MeshFilter>();
                mesh = new Mesh();
                meshFilter.sharedMesh = mesh;

            }

            if (meshFilter != null)
            {
                mesh.Clear();
                mesh.name = "Intersection";
            }
            if (!crossing4)
            {
                GenerateMesh3Ways();
            }
            else
            {
                GenerateMesh();
            }


        }

        public void AlignTerrain()
        {
            Terrain[] terrains = FindObjectsByType<Terrain>(FindObjectsSortMode.None);

            foreach (var item in terrains)
            {
                if (item.GetComponent<HannaTerrainController>() == null)
                {
                    item.gameObject.AddComponent<HannaTerrainController>();
                }


                HannaTerrainController hannaTerrainController = item.GetComponent<HannaTerrainController>();

                foreach (var vert in wordVertices)
                {
                    hannaTerrainController.RampTerrain(vert, terrainAlignRadius, maxAlignDistance, minAlignDistance, terrainAlignCurve, terrainBottomMargin);
                }
            }
        }


        Vector3 BevelSidePoint(Vector3 A, Vector3 B, Vector3 C, float amount)
        {

            Vector3 aBLerp = Vector3.Lerp(A, B, amount);
            Vector3 bCLerp = Vector3.Lerp(B, C, amount);


            return Vector3.Lerp(aBLerp, bCLerp, amount);

        }




        void SetVerticesEdgePoints()
        {

            Vector3 endPoint1 = Vector3.forward * (size + extrusionSize);
            Vector3 endPoint2 = (Vector3.back * (size + extrusionSize));
            Vector3 endPoint3 = (Vector3.left * (size + extrusionSize));
            Vector3 endPoint4 = (Vector3.right * (size + extrusionSize));

            intersectionPoints[0] = new IntersectionAttachment()
            {
                middlePoint = endPoint1,
                leftPoint = endPoint1 + Vector3.right * size,
                rightPoint = endPoint1 + Vector3.left * size,
            };

            intersectionPoints[1] = new IntersectionAttachment()
            {
                middlePoint = endPoint4,
                leftPoint = endPoint4 + Vector3.back * size,
                rightPoint = endPoint4 + Vector3.forward * size
            };

            if (crossing4)
            {
                intersectionPoints[2] = new IntersectionAttachment()
                {
                    middlePoint = endPoint2,
                    leftPoint = endPoint2 + Vector3.left * size,
                    rightPoint = endPoint2 + Vector3.right * size
                };
            }

            intersectionPoints[crossing4 ? 3 : 2] = new IntersectionAttachment()
            {
                middlePoint = endPoint3,
                leftPoint = endPoint3 + Vector3.forward * size,
                rightPoint = endPoint3 + Vector3.back * size
            };



        }

        List<Vector3> GenerateBevelPoints(LSideIntersection lSide, float shapeAmount)
        {

            Vector3 pointA = lSide.subPoint1;
            Vector3 pointB = lSide.subPoint2;
            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < resolutionCurve + 1; i++)
            {
                float t = i / (float)resolutionCurve;

                points.Add(BevelSidePoint(pointA, lSide.mainPoint * (shapeAmount + 1), pointB, t));
            }


            return points;
        }






        void GenerateQuad()
        {

            List<LSideIntersection> intersectionLSides = new List<LSideIntersection>();


            LSideIntersection A = new LSideIntersection()
            {
                mainPoint = new Vector3(-1, 0, 1) * size
            };
            LSideIntersection B = new LSideIntersection()
            {
                mainPoint = new Vector3(1, 0, 1) * size
            };
            LSideIntersection C = new LSideIntersection()
            {
                mainPoint = new Vector3(-1, 0, -1) * size
            };
            LSideIntersection D = new LSideIntersection()
            {
                mainPoint = new Vector3(1, 0, -1) * size
            };


            //A extrusions
            A.subPoint1 = (Vector3.left * extrusionSize) + A.mainPoint;
            A.subPoint2 = (Vector3.forward * extrusionSize) + A.mainPoint;


            //B extrusions
            B.subPoint1 = (Vector3.forward * extrusionSize) + B.mainPoint;
            B.subPoint2 = (Vector3.right * extrusionSize) + B.mainPoint;

            D.subPoint1 = (Vector3.right * extrusionSize) + D.mainPoint;
            D.subPoint2 = (Vector3.back * extrusionSize) + D.mainPoint;


            C.subPoint2 = (Vector3.back * extrusionSize) + C.mainPoint;
            C.subPoint1 = (Vector3.left * extrusionSize) + C.mainPoint;




            intersectionLSides.Add(A);
            intersectionLSides.Add(B);
            intersectionLSides.Add(C);
            intersectionLSides.Add(D);







            for (int i = 0; i < intersectionLSides.Count; i++)
            {
                LSideIntersection lSide = intersectionLSides[i];
                lSide.SetBevelPoints(GenerateBevelPoints(lSide, shape));

                intersectionLSides[i] = lSide;
            }



            Vector3 a = transform.TransformPoint(intersectionLSides[0].mainPoint);
            Vector3 b = transform.TransformPoint(intersectionLSides[1].mainPoint);
            Vector3 c = transform.TransformPoint(intersectionLSides[2].mainPoint);
            Vector3 d = transform.TransformPoint(intersectionLSides[3].mainPoint);





            intersections = intersectionLSides;
        }

        void GenerateMesh()
        {


            if (resolutionCurve < 4)
            {
                resolutionCurve = 4;
            }

            shape = Mathf.Clamp(shape, 0, 1);

            LSideIntersection A = new LSideIntersection()
            {
                mainPoint = new Vector3(-1, 0, 1) * size
            };
            LSideIntersection B = new LSideIntersection()
            {
                mainPoint = new Vector3(1, 0, 1) * size
            };
            LSideIntersection C = new LSideIntersection()
            {
                mainPoint = new Vector3(-1, 0, -1) * size
            };
            LSideIntersection D = new LSideIntersection()
            {
                mainPoint = new Vector3(1, 0, -1) * size
            };

            intersections.Add(A);
            intersections.Add(B);
            intersections.Add(C);
            intersections.Add(D);



            //A extrusions
            A.subPoint1 = (Vector3.left * (extrusionSize)) + A.mainPoint;
            A.subPoint2 = (Vector3.forward * (extrusionSize)) + A.mainPoint;


            //B extrusions
            B.subPoint1 = (Vector3.forward * (extrusionSize)) + B.mainPoint;
            B.subPoint2 = (Vector3.right * (extrusionSize)) + B.mainPoint;

            D.subPoint1 = (Vector3.right * (extrusionSize)) + D.mainPoint;
            D.subPoint2 = (Vector3.back * (extrusionSize)) + D.mainPoint;


            C.subPoint1 = (Vector3.back * (extrusionSize)) + C.mainPoint;
            C.subPoint2 = (Vector3.left * (extrusionSize)) + C.mainPoint;

            A.SetBevelPoints(GenerateBevelPoints(A, shape));
            B.SetBevelPoints(GenerateBevelPoints(B, shape));
            C.SetBevelPoints(GenerateBevelPoints(C, shape));
            D.SetBevelPoints(GenerateBevelPoints(D, shape));



            List<Vector3> vertices = new List<Vector3>
        {
            A.mainPoint,
            B.mainPoint,
            D.mainPoint,
            C.mainPoint
        };

            List<Vector3> normals = new List<Vector3>
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
        };

            vertices.AddRange(A.bevelPoints);
            vertices.AddRange(B.bevelPoints);
            vertices.AddRange(D.bevelPoints);
            vertices.AddRange(C.bevelPoints);


            for (int i = 0; i < vertices.Count - 4; i++)
            {
                normals.Add(Vector3.up);
            }



            List<int> triangles = new List<int>();


            int current = 3;
            for (int i = 0; i < 4; i++)
            {

                for (int j = 0; j < resolutionCurve; j++)
                {
                    current++;
                    triangles.Add(i);
                    triangles.Add(current);
                    triangles.Add(current + 1);


                }


                current++;
            }




            int section = 0;
            for (int i = 4; i < 4 * (resolutionCurve + 1); i += resolutionCurve + 1)
            {
                int end = i + resolutionCurve;
                int start = i;

                if (section == 3)
                {


                    triangles.Add(end);
                    triangles.Add(0);
                    triangles.Add(section);

                    triangles.Add(4);
                    triangles.Add(0);
                    triangles.Add(end);
                    continue;
                }






                triangles.Add(end);
                triangles.Add(end + 1);
                triangles.Add(section);

                triangles.Add(end + 1);
                triangles.Add(section + 1);
                triangles.Add(section);


                section++;

            }

            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);

            triangles.Add(3);
            triangles.Add(0);
            triangles.Add(2);








            debugTriangles = triangles;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, vertices.Select(vertex => new Vector2(vertex.x, vertex.z) * uvSize).ToList());

            wordVertices.Clear();
            foreach (var item in vertices)
            {
                wordVertices.Add(transform.TransformPoint(item));
            }





        }


        void GenerateMesh3Ways()
        {


            if (resolutionCurve < 4)
            {
                resolutionCurve = 4;
            }

            shape = Mathf.Clamp(shape, 0, 1);

            LSideIntersection A = new LSideIntersection()
            {
                mainPoint = new Vector3(-1, 0, 1) * size
            };
            LSideIntersection B = new LSideIntersection()
            {
                mainPoint = new Vector3(1, 0, 1) * size
            };
            LSideIntersection C = new LSideIntersection()
            {
                mainPoint = new Vector3(-1, 0, -1) * size
            };
            LSideIntersection D = new LSideIntersection()
            {
                mainPoint = new Vector3(1, 0, -1) * size
            };

            //A extrusions
            A.subPoint1 = (Vector3.left * extrusionSize) + A.mainPoint;
            A.subPoint2 = (Vector3.forward * extrusionSize) + A.mainPoint;


            //B extrusions
            B.subPoint1 = (Vector3.forward * extrusionSize) + B.mainPoint;
            B.subPoint2 = (Vector3.right * extrusionSize) + B.mainPoint;

            C.subPoint2 = (Vector3.left * extrusionSize) + C.mainPoint;

            D.subPoint1 = (Vector3.right * extrusionSize) + D.mainPoint;
            D.subPoint2 = (Vector3.back * extrusionSize) + D.mainPoint;



            A.SetBevelPoints(GenerateBevelPoints(A, shape));
            B.SetBevelPoints(GenerateBevelPoints(B, shape));



            List<Vector3> vertices = new List<Vector3>
        {
            A.mainPoint,
            B.mainPoint,
            C.mainPoint,
            D.mainPoint,
        };

            List<Vector3> normals = new List<Vector3>
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
        };

            vertices.Add(C.subPoint2);
            vertices.Add(D.subPoint1);
            vertices.AddRange(A.bevelPoints);
            vertices.AddRange(B.bevelPoints);










            for (int i = 0; i < vertices.Count - 4; i++)
            {
                normals.Add(Vector3.up);
            }



            List<int> triangles = new List<int>();



            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(4);

            triangles.Add(6);
            triangles.Add(0);
            triangles.Add(4);


            triangles.Add(0);
            triangles.Add(6 + resolutionCurve);
            triangles.Add(1);


            triangles.Add(6 + resolutionCurve);
            triangles.Add(7 + resolutionCurve);
            triangles.Add(1);



            triangles.Add(1);
            triangles.Add(7 + resolutionCurve * 2);
            triangles.Add(3);

            triangles.Add(3);
            triangles.Add(7 + resolutionCurve * 2);
            triangles.Add(5);


            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);

            triangles.Add(3);
            triangles.Add(2);
            triangles.Add(1);




            for (int i = 0; i < 2; i++)
            {

                for (int j = 0; j < resolutionCurve; j++)
                {

                    triangles.Add(i);
                    triangles.Add(6 + j + (i * resolutionCurve));
                    triangles.Add(6 + j + (i * resolutionCurve) + 1);

                }
                if (i == 1)
                {
                    triangles.Add(1);
                    triangles.Add(6 + resolutionCurve * 2);
                    triangles.Add(7 + resolutionCurve * 2);

                }

            }




            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, vertices.Select(vertex => new Vector2(vertex.x, vertex.z) * uvSize).ToList());





        }

        [SerializeField] List<int> debugTriangles;
    }



    [System.Serializable]
    struct LSideIntersection
    {
        public Vector3 mainPoint;
        public Vector3 subPoint1;
        public Vector3 subPoint2;
        public List<Vector3> bevelPoints;

        public void SetBevelPoints(List<Vector3> points)
        {
            bevelPoints = points;
        }
    }




    [System.Serializable]


    public struct IntersectionAttachment
    {

        public Vector3 middlePoint;

        public Vector3 leftPoint;
        public Vector3 rightPoint;

        public bool inUse;

    }

}