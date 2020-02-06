using Assets.Scripts.Terrain;
using Assets.Scripts.Terrain.Biomes;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public float heightScale = 5.0f;
    public float detailScale = 2.0f;
    public int vertexCountRadius = 100;

    [SerializeField]
    public Biome[] biomes = new Biome[] { };
    public Texture2D biomeMap;

    //World scale is 1m = 1 world unit

    private Mesh mesh;

    private int GetVertexIndexNearestToPoint(float x, float z)
    {
        Vector2Int subindices = GetVertexSubindicesNearestToPoint(x, z);

        int index = (subindices.x) * (vertexCountRadius * 2 + 1) + (subindices.y);
        return index;
    }

    private Vector2Int GetVertexSubindicesNearestToPoint(float x, float z)
    {
        int xVertex = Mathf.RoundToInt(x * vertexCountRadius) + vertexCountRadius;
        int zVertex = Mathf.RoundToInt(z * vertexCountRadius) + vertexCountRadius;

        xVertex = Mathf.Clamp(xVertex, 0, vertexCountRadius * 2);
        zVertex = Mathf.Clamp(zVertex, 0, vertexCountRadius * 2);

        return new Vector2Int(xVertex, zVertex);
    }

    private float GetHeightNearestToPoint(float x, float z)
    {
        Vector3[] vertices = mesh.vertices;
        int index = GetVertexIndexNearestToPoint(x, z);
        Vector3 vertex = vertices[index];

        //Debug.Log("" + x + ", " + z + " => " + vertex);
        return vertex.y;
    }

    private Vector4 GetPlaneEquationFromThreePoints(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1);
        //normal = normal.normalized;
        float d = (-normal.x * p1.x - normal.y * p1.y - normal.z * p1.z);

        //returns ax + by + cz + d = 0
        return new Vector4(normal.x, normal.y, normal.z, d);
    }

    public float GetHeightAtPoint(float x, float z)
    {
        x /= transform.localScale.x;
        z /= transform.localScale.z;

        int nearestIndex = GetVertexIndexNearestToPoint(x, z);
        Vector3 nearestVector = mesh.vertices[nearestIndex];
        if (Mathf.Approximately(nearestVector.x, Mathf.Round(x)) && Mathf.Approximately(nearestVector.z, Mathf.Round(z)))
        {
            //point is approximately on a vertex
            return GetHeightNearestToPoint(x, z);
        }

        //get vertex positions (floor to get the lower-left always)
        int xVertex = Mathf.FloorToInt(x * vertexCountRadius) + vertexCountRadius;
        int zVertex = Mathf.FloorToInt(z * vertexCountRadius) + vertexCountRadius;
        xVertex = Mathf.Clamp(xVertex, 0, vertexCountRadius * 2);
        zVertex = Mathf.Clamp(zVertex, 0, vertexCountRadius * 2);

        //locate quad
        int quadIndex = (xVertex) * (vertexCountRadius * 2) + (zVertex);
        int firstTriangleIndex = quadIndex * 6;

        //get quad bounds
        Vector3 lowerLeft = mesh.vertices[mesh.triangles[firstTriangleIndex]];
        Vector3 upperRight = mesh.vertices[mesh.triangles[firstTriangleIndex + 3]];

        //determine if point is on left or right triangle in quad
        float xPercent = (x - lowerLeft.x) / (upperRight.x - lowerLeft.x);
        float zPercent = (z - lowerLeft.z) / (upperRight.z - lowerLeft.z);
        bool onLeftTriangle = zPercent <= 1 - xPercent;
        int triangleIndex = firstTriangleIndex + (onLeftTriangle ? 0 : 3);

        //get plane equation then evaluate point
        //form: y = (-d - ax - cz) / b
        Vector4 planeEqn = GetPlaneEquationFromThreePoints(mesh.vertices[mesh.triangles[triangleIndex]],
            mesh.vertices[mesh.triangles[triangleIndex + 1]], mesh.vertices[mesh.triangles[triangleIndex + 2]]);
        float y = (-planeEqn[3] - planeEqn[0] * x - planeEqn[2] * z) / planeEqn[1];

        //Debug.Log("" + x + ", " + z + " => " + lowerLeft + " | " + upperRight + " => " + y);

        return y;
    }

    private HashSet<Vector2> searchedPositions;

    public float GetMinimumHeightNear(float x, float z, float radius)
    {
        if (Mathf.Approximately(radius, 0f))
        {
            return GetHeightAtPoint(x, z);
        }

        //perform recursive breadth-first search
        searchedPositions = new HashSet<Vector2>();
        float minHeight = GetMinimumHeightNearRecursive(x, z, radius, x, z, float.MaxValue);
        //Debug.Log("" + x + ", " + z + " within " + radius + ": " + minHeight);
        return minHeight;
    }

    private float GetMinimumHeightNearRecursive(float x, float z, float radius, float originalX, float originalZ, float minHeight)
    {
        Vector2 pos = new Vector2(x, z);
        Vector2[] DIRECTIONS = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        //make the operation take at most SEARCH_DEPTH steps (simplifies search depth)
        const int SEARCH_DEPTH = 3;
        float stepSize = radius / SEARCH_DEPTH;

        minHeight = Mathf.Min(GetHeightAtPoint(x, z), minHeight);

        foreach (Vector2 dir in DIRECTIONS)
        {
            Vector2 point = pos + dir * stepSize;
            if (!searchedPositions.Contains(point))
            {
                searchedPositions.Add(point);

                float d = Mathf.Sqrt((point.x - originalX) * (point.x - originalX) + (point.y - originalZ) * (point.y - originalZ));
                if (d <= radius)
                {
                    minHeight = Mathf.Min(GetMinimumHeightNearRecursive(point.x, point.y, radius, originalX, originalZ, minHeight), minHeight);
                }
            }
        }

        //TODO this algorithm does not account for areas where the edge of a vertex decreases just outside the radius

        return minHeight;
    }

    public GameObject PlaceObjectAt(GameObject obj, float x, float z, float radius=0.0f, float offset=0.0f)
    {
        float minHeight = GetMinimumHeightNear(x, z, radius);

        GameObject newObj = Instantiate(obj);
        newObj.transform.position = new Vector3(x, minHeight - 0.001f + offset, z);
        return newObj;
    }

    private int GetNearestBiomeIndex(float biomeValue)
    {
        int biomeIndex = -1;
        float biomeDist = float.MaxValue;

        for (int i=0; i < biomes.Length; i++)
        {
            float dist = Mathf.Abs(biomes[i].biomeMapHeight - biomeValue);
            if (dist < biomeDist)
            {
                biomeDist = dist;
                biomeIndex = i;
            }
        }

        return biomeIndex;
    }

    private Biome[] GetLerpBiomes(float biomeValue)
    {
        int nearestBiomeIndex = GetNearestBiomeIndex(biomeValue);
        if (Mathf.Approximately(biomeValue, biomes[nearestBiomeIndex].biomeMapHeight))
            return new Biome[] { biomes[nearestBiomeIndex], biomes[nearestBiomeIndex] };

        if (biomeValue < biomes[nearestBiomeIndex].biomeMapHeight)
        {
            if (nearestBiomeIndex <= 0)
            {
                return new Biome[] { biomes[nearestBiomeIndex], biomes[nearestBiomeIndex] };
            } else
            {
                return new Biome[] { biomes[nearestBiomeIndex - 1], biomes[nearestBiomeIndex] };
            }
        } else
        {
            if (nearestBiomeIndex >= biomes.Length - 1)
            {
                return new Biome[] { biomes[nearestBiomeIndex], biomes[nearestBiomeIndex] };
            }
            else
            {
                return new Biome[] { biomes[nearestBiomeIndex], biomes[nearestBiomeIndex + 1] };
            }
        }
    }

    public struct BiomeResult
    {
        public float lerpPosition;
        public Biome[] lerpBiomes;

        public BiomeResult(Biome[] lerpBiomes, float lerpPosition)
        {
            this.lerpBiomes = lerpBiomes;
            this.lerpPosition = lerpPosition;
        }
    }

    private BiomeResult GetBiomeInterpolationValues(float u, float v)
    {
        float biomeValue = biomeMap.GetPixelBilinear(u, v).grayscale;
        Biome[] lerpBiomes = GetLerpBiomes(biomeValue);
        float biomeWidth = Mathf.Abs(lerpBiomes[0].biomeMapHeight - lerpBiomes[1].biomeMapHeight);
        float lerpPosition = Mathf.Approximately(biomeWidth, 0f) ? 0f : biomeValue / biomeWidth;

        return new BiomeResult(lerpBiomes, lerpPosition);
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Procedural Terrain";

        //Generate vertices
        int vertexCount = (vertexCountRadius * 2 + 1) * (vertexCountRadius * 2 + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        for (int x = -vertexCountRadius, i = 0; x <= vertexCountRadius; x++)
        {
            for (int z = -vertexCountRadius; z <= vertexCountRadius; z++)
            {
                //Get nearest biome(s) and crossfade values
                float u = (float)(x + vertexCountRadius) / (2 * vertexCountRadius);
                float v = (float)(z + vertexCountRadius) / (2 * vertexCountRadius);
                var biomeData = GetBiomeInterpolationValues(u, v);

                //Crossfade values
                float regionDetailScale = Mathf.Lerp(biomeData.lerpBiomes[0].biomeDetailScale, biomeData.lerpBiomes[1].biomeDetailScale, biomeData.lerpPosition);
                float regionHeightScale = Mathf.Lerp(biomeData.lerpBiomes[0].biomeHeightScale, biomeData.lerpBiomes[1].biomeHeightScale, biomeData.lerpPosition) * heightScale;
                float regionHeightOffset = Mathf.Lerp(biomeData.lerpBiomes[0].biomeHeightOffset, biomeData.lerpBiomes[1].biomeHeightOffset, biomeData.lerpPosition);

                //Generate vertex
                vertices[i] = new Vector3();
                vertices[i].x = (float)x / vertexCountRadius;
                vertices[i].z = (float)z / vertexCountRadius;

                //Generate height from noise
                float height = Mathf.PerlinNoise(
                    (vertices[i].x + transform.position.x) * regionDetailScale,
                    (vertices[i].z + transform.position.z) * regionDetailScale);

                //Evaluate against height curves
                height = regionHeightOffset + regionHeightScale *
                    Mathf.Lerp(biomeData.lerpBiomes[0].heightCurve.Evaluate(height),
                    biomeData.lerpBiomes[1].heightCurve.Evaluate(height),
                    biomeData.lerpPosition);

                //Set vertex height
                vertices[i].y = height;

                //Debug.Log("" + vertices[i].x + ", " + vertices[i].y + ", " + vertices[i].z);
                i++;
            }
        }

        //Generate triangles (quads)
        int verticesPerRow = vertexCountRadius * 2 + 1;
        int quadCount = (verticesPerRow - 1) * (verticesPerRow - 1);
        int[] triangles = new int[quadCount * 6];
        for (int ti = 0, vi = 0, x = -vertexCountRadius; x < vertexCountRadius; x++, vi++)
        {
            for (int z = -vertexCountRadius; z < vertexCountRadius; z++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 1;
                triangles[ti + 2] = vi + verticesPerRow;
                triangles[ti + 3] = vi + verticesPerRow + 1;
                triangles[ti + 4] = vi + verticesPerRow;
                triangles[ti + 5] = vi + 1;
                //Debug.Log("" + x + ", " + z + ": " + vertices[triangles[ti]] + ", " + vertices[triangles[ti + 1]] + ", " + vertices[triangles[ti + 2]]);
                //Debug.Log("" + x + ", " + z + ": " + vertices[triangles[ti + 3]] + ", " + vertices[triangles[ti + 4]] + ", " + vertices[triangles[ti + 5]]);
            }
        }

        //Generate u,v (texture) coordinates
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int x = 0, i = 0; x < verticesPerRow; x++)
        {
            for (int z = 0; z < verticesPerRow; z++)
            {
                uvs[i] = new Vector2((float)x / verticesPerRow, (float)z / verticesPerRow);
                i++;
            }
        }

        //Update mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshFilter>().mesh = mesh;

        //Update mesh collider
        GetComponent<MeshCollider>().sharedMesh = mesh;

        //Set initial cube to the position
        GameObject referenceHuman = GameObject.Find("Reference Human");
        Vector3 cubePos = referenceHuman.transform.position;
        referenceHuman.transform.position = new Vector3(cubePos.x, GetMinimumHeightNear(cubePos.x, cubePos.z, Mathf.Sqrt(2)) + referenceHuman.transform.localScale.y / 2, cubePos.z);

        //Set house position
        float approximateHeightAtZero = GetHeightAtPoint(0f, 0f);
        GameObject smallHouse = GameObject.Find("Small House");
        GameObject largeHouse = GameObject.Find("Large House");
        smallHouse.transform.position = new Vector3(smallHouse.transform.position.x, smallHouse.transform.position.y + approximateHeightAtZero, smallHouse.transform.position.z);
        largeHouse.transform.position = new Vector3(largeHouse.transform.position.x, largeHouse.transform.position.y + approximateHeightAtZero, largeHouse.transform.position.z);

        //Reset preview scale
        transform.localScale = new Vector3(transform.localScale.x, 1.0f, transform.localScale.z);

        //Enable water
        //GameObject.Find("Water").SetActive(true);

        //Hide reference human
        GameObject.Find("Reference Human").SetActive(false);

        //Generate biome
        for (float x = -transform.localScale.x; x <= transform.localScale.x; x += Constants.BLOCK_SIZE_METERS)
        {
            for (float z = -transform.localScale.z; z <= transform.localScale.z; z += Constants.BLOCK_SIZE_METERS)
            {
                //Get nearest biome and calculate biome strength
                float u = (x + transform.localScale.x) / (2f * transform.localScale.x);
                float v = (z + transform.localScale.z) / (2f * transform.localScale.z);
                var biomeData = GetBiomeInterpolationValues(u, v);
                Biome primaryBiome = biomeData.lerpPosition >= 0.5f ? biomeData.lerpBiomes[1] : biomeData.lerpBiomes[0];
                Biome secondaryBiome = biomeData.lerpPosition < 0.5f ? biomeData.lerpBiomes[1] : biomeData.lerpBiomes[0];
                float primaryBiomeStrength = 1.0f - biomeData.lerpPosition;
                float secondaryBiomeStrength = biomeData.lerpPosition;

                //Calculate block positions
                Vector2 blockLowerLeft = new Vector2(x, z);
                Vector2 blockUpperRight = new Vector2(x + Constants.BLOCK_SIZE_METERS, z + Constants.BLOCK_SIZE_METERS);

                //Generate biome
                if (primaryBiomeStrength > Constants.BIOME_GENERATION_STRENGTH_THRESHOLD)
                {
                    primaryBiome.Generate(this, blockLowerLeft, blockUpperRight, primaryBiomeStrength);
                }
                if (secondaryBiomeStrength > Constants.BIOME_GENERATION_STRENGTH_THRESHOLD)
                {
                    secondaryBiome.Generate(this, blockLowerLeft, blockUpperRight, secondaryBiomeStrength);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
