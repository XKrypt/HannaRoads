using System;
using HannaRoads;
using UnityEngine;


[ExecuteInEditMode]
public class HannaTerrainController : MonoBehaviour
{

    public Terrain terrain;           // Referência para o objeto Terrain 


    void OnDrawGizmos()
    {

    }


    public void RampTerrain(Vector3 worldPoint, float radius, float maxDistance, float minDistance, AnimationCurve animationCurve, float bottomMargin)
    {

        if (animationCurve == null)
        {
            animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        if (!terrain) return;

        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;

        // Altura normalizada
        float normalizedHeight = ((worldPoint - terrain.transform.position).y - bottomMargin) / data.size.y;

        Vector3 center = worldPoint;


        TerrainVector2 posInTerrainSpace = ConvertFromWordPositionToTerrainPosition(terrain, center);

        // Define tamanho do quadrado em volta do ponto central
        int halfSize = Mathf.RoundToInt((radius / data.size.x) * (resolution - 1));


        int width = halfSize * 2 + 1;
        int height = halfSize * 2 + 1;

        //Move para que base x e y para que o ponto  fique bem no meio.
        int xBase = Mathf.Clamp(posInTerrainSpace.x - halfSize, 0, resolution - width);
        int zBase = Mathf.Clamp(posInTerrainSpace.z - halfSize, 0, resolution - height);

        float[,] heights = data.GetHeights(xBase, zBase, width, height);


        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 worldPos = ConvertFromTerrainPositionToWordPosition(xBase + x, zBase + z, terrain);

                float dist = Vector2.Distance(new Vector2(worldPos.x, worldPos.z), new Vector2(center.x, center.z));

                if (dist <= minDistance)
                {
                    heights[z, x] = normalizedHeight;
                }
                else if (dist <= maxDistance)
                {
                    float t = Mathf.InverseLerp(maxDistance, minDistance, dist); // suavizado
                    heights[z, x] = Mathf.Lerp(heights[z, x], normalizedHeight, animationCurve.Evaluate(t));
                }
            }
        }

        data.SetHeights(xBase, zBase, heights);
    }


    public void RampTerrainAcumulative(Vector3 worldPoint, float radius, float maxDistance, float minDistance, AnimationCurve animationCurve, float bottomMargin)
    {

        if (animationCurve == null)
        {
            animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        if (!terrain) return;

        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;

        // Altura normalizada
        float normalizedHeight = ((worldPoint - terrain.transform.position).y - bottomMargin) / data.size.y;

        Vector3 center = worldPoint;


        TerrainVector2 posInTerrainSpace = ConvertFromWordPositionToTerrainPosition(terrain, center);

        // Define tamanho do quadrado em volta do ponto central
        int halfSize = Mathf.RoundToInt((radius / data.size.x) * (resolution - 1));


        int width = halfSize * 2 + 1;
        int height = halfSize * 2 + 1;

        //Move para que base x e y para que o ponto  fique bem no meio.
        int xBase = Mathf.Clamp(posInTerrainSpace.x - halfSize, 0, resolution - width);
        int zBase = Mathf.Clamp(posInTerrainSpace.z - halfSize, 0, resolution - height);

        float[,] heights = data.GetHeights(xBase, zBase, width, height);


        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Converte índice da grade para posição no mundo
                Vector3 worldPos = ConvertFromTerrainPositionToWordPosition(xBase + x, zBase + z, terrain);

                float dist = Vector2.Distance(new Vector2(worldPos.x, worldPos.z), new Vector2(center.x, center.z));

                float targetHeight = heights[z, x]; // valor padrão é o atual

                if (dist <= minDistance)
                {
                    targetHeight = normalizedHeight;
                }
                else if (dist <= maxDistance)
                {
                    float t = Mathf.InverseLerp(maxDistance, minDistance, dist); // 0 → longe, 1 → perto
                    float blend = animationCurve.Evaluate(t);
                    targetHeight = Mathf.Lerp(heights[z, x], normalizedHeight, blend);
                }

                // ✅ Aplica apenas se for maior
                if (targetHeight > heights[z, x])
                {
                    heights[z, x] = targetHeight;
                }
            }
        }

        data.SetHeights(xBase, zBase, heights);
    }

    public void RampTerrainNoCurve(Vector3 worldPoint, float radius, float bottomMargin)
    {
        if (!terrain) return;

        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;
        Vector3 terrainPos = terrain.transform.position;

        // Altura normalizada baseada na posição do ponto menos a margem
        float normalizedHeight = ((worldPoint.y - terrainPos.y) - bottomMargin) / data.size.y;

        Vector3 center = worldPoint;

        // Converte a posição do mundo para a grade do terreno
        TerrainVector2 posInTerrainSpace = ConvertFromWordPositionToTerrainPosition(terrain, center);

        // Define tamanho da área a ser afetada
        int halfSize = Mathf.RoundToInt((radius / data.size.x) * (resolution - 1));
        int width = halfSize * 2 + 1;
        int height = halfSize * 2 + 1;

        int xBase = Mathf.Clamp(posInTerrainSpace.x - halfSize, 0, resolution - width);
        int zBase = Mathf.Clamp(posInTerrainSpace.z - halfSize, 0, resolution - height);

        // Pega a altura atual da área
        float[,] heights = data.GetHeights(xBase, zBase, width, height);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 worldPos = ConvertFromTerrainPositionToWordPosition(xBase + x, zBase + z, terrain);

                float dist = Vector2.Distance(new Vector2(worldPos.x, worldPos.z), new Vector2(center.x, center.z));

                // Aplica altura diretamente se dentro do raio
                if (dist <= radius && normalizedHeight > heights[z, x])
                {
                    heights[z, x] = normalizedHeight;
                }
            }
        }

        data.SetHeights(xBase, zBase, heights);
    }


    public void RampTerrainAlongBezier(
        float[,] accumulatedHeights,
        bool[,] affectedCells,
        float width,
        OrientedPoint bezierPoint,
        float bottomMargin,
        Vector3 terrainSize,
        int heightmapRes,
        Vector3 terrainPosition
        )
    {

        Vector3 leftWorld = bezierPoint.LocalSpace(Vector3.left * (width / 2f));
        Vector3 rightWorld = bezierPoint.LocalSpace(Vector3.right * (width / 2f));

        int sampleCount = Mathf.CeilToInt(width);
        for (int j = 0; j <= sampleCount; j++)
        {
            float lerpT = j / (float)sampleCount;
            Vector3 worldPoint = Vector3.Lerp(leftWorld, rightWorld, lerpT);

            int x = Mathf.RoundToInt(((worldPoint.x - terrainPosition.x) / terrainSize.x) * (heightmapRes - 1));
            int z = Mathf.RoundToInt(((worldPoint.z - terrainPosition.z) / terrainSize.z) * (heightmapRes - 1));

            if (x >= 0 && x < heightmapRes && z >= 0 && z < heightmapRes)
            {
                float rawHeight = worldPoint.y - terrainPosition.y;

                float normalizedHeight = (rawHeight - bottomMargin) / terrainSize.y;


                if (!affectedCells[z, x] || normalizedHeight < accumulatedHeights[z, x])
                {
                    accumulatedHeights[z, x] = normalizedHeight;
                    affectedCells[z, x] = true;
                }
            }
        }
    }


    public void FlattenRectangleAlongBezier(float width, float thickness, float bottomMargin, OrientedPoint bezierPoint)
    {
        TerrainData data = terrain.terrainData;

        Vector3 terrainPos = terrain.transform.position;

        int heightmapRes = data.heightmapResolution;


        int lateralSegments = Mathf.CeilToInt(width / 1.0f);

        for (int j = -lateralSegments / 2; j <= lateralSegments; j++)
        {
            Vector3 offset = Vector3.right * j * (width / lateralSegments);

            Vector3 worldPoint = bezierPoint.LocalSpace(offset);

            float normalizedHeight = (worldPoint.y - terrainPos.y - bottomMargin) / data.size.y;


            int x = Mathf.RoundToInt(((worldPoint.x - terrainPos.x) / data.size.x) * (heightmapRes - 1));
            int z = Mathf.RoundToInt(((worldPoint.z - terrainPos.z) / data.size.z) * (heightmapRes - 1));

            int half = Mathf.CeilToInt(thickness);
            int size = half * 2 + 1;
            int xBase = Mathf.Clamp(x - half, 0, heightmapRes - size);
            int zBase = Mathf.Clamp(z - half, 0, heightmapRes - size);

            float[,] heights = data.GetHeights(xBase, zBase, size, size);

            for (int xi = 0; xi < size; xi++)
            {
                for (int zi = 0; zi < size; zi++)
                {
                    heights[zi, xi] = Mathf.Max(heights[zi, xi], normalizedHeight);
                }
            }
            data.SetHeights(xBase, zBase, heights);
        }

    }


    public TerrainVector2 ConvertFromWordPositionToTerrainPosition(Terrain terrain, Vector3 center)
    {
        Vector3 terrainPos = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        int xCenter = Mathf.RoundToInt(((center.x - terrainPos.x) / terrainData.size.x) * (resolution - 1));
        int zCenter = Mathf.RoundToInt(((center.z - terrainPos.z) / terrainData.size.z) * (resolution - 1));

        return new TerrainVector2()
        {
            x = xCenter,
            z = zCenter
        };
    }
    public Vector3 ConvertFromTerrainPositionToWordPosition(int xPos, int zPox, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        Vector3 terrainPos = terrain.transform.position;
        // Converte (x, z) da grade local para coordenada mundial real
        float worldX = (xPos / (float)(resolution - 1)) * terrainData.size.x + terrainPos.x;
        float worldZ = (zPox / (float)(resolution - 1)) * terrainData.size.z + terrainPos.z;

        return new Vector3(worldX, 0, worldZ);
    }


}
public struct TerrainVector2
{
    public int x;
    public int z;
}
