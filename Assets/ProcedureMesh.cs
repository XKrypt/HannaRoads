using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedureMesh : MonoBehaviour
{
    private void Awake() {
        Mesh mesh = new Mesh();

        mesh.name = "Procedural Quad";

        List<Vector3> points = new List<Vector3>(){
            new Vector3(-1,1),
            new Vector3(1,1),
            new Vector3(-1,-1),
            new Vector3(1,-1),
        };

        int[] triIndices = new int[]{
            1,0,2,
            3,1,2
        };

        List<Vector3> normal = new List<Vector3>(){
            new Vector3(0,0,1),
            new Vector3(0,0,1),
            new Vector3(0,0,1),
            new Vector3(0,0,1),
        };


        mesh.SetVertices(points);
        mesh.triangles = triIndices;
        mesh.SetNormals(normal);

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
