using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HanaRoads/CustomMesh", menuName = "CustomMesh", order = 0)]
public class CustomMesh : ScriptableObject
{
    public List<HannaVertex> vertices = new List<HannaVertex>();


    public Color midMatchColor = Color.red;

    public Mesh mesh;

    [ContextMenu("CustomMesh/LoadDetails")]
    public void LoadMeshDetails()
    {
        List<HannaVertex> tvertices = new List<HannaVertex>();
        List<Vector3> meshVertices = new List<Vector3>();
        List<Vector3> meshNormals = new List<Vector3>();

        List<Color> meshVertexColors = new List<Color>();



        mesh.GetVertices(meshVertices);
        mesh.GetNormals(meshNormals);
        mesh.GetColors(meshVertexColors);

        Debug.Log(mesh.name);
        for (int i = 0; i < meshVertexColors.Count; i++)
        {
            if (meshVertexColors[i] == midMatchColor)
            {
                tvertices.Add(new HannaVertex()
                {
                    point = new Vector3(
                        meshVertices[i].x,
                        meshVertices[i].y,
                        meshVertices[i].z
                    ),
                    normal = meshNormals[i]
                });
            }

        }
        vertices = tvertices;


    }
}


[System.Serializable]
public struct HannaVertex
{
    public Vector3 point;
    public Vector3 normal;
    public float u;
    public Color vertexColor;
}