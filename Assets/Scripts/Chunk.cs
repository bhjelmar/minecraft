using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    int vertexIndex = 0;

    public Blocks.BlockType[,,] voxelMap = new Blocks.BlockType[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    World world;

    private bool _isActive;
    public bool isVoxelMapPopulated = false;

    public Chunk(World world, ChunkCoord coord, bool generateOnLoad)
    {
        this.world = world;
        this.coord = coord;
        isActive = true;

        if (generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = this.world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = $"Chunk:({coord.x},{coord.z})";

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + chunkPos);
                }
            }
        }
        isVoxelMapPopulated = true;
    }

    void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (Blocks.blockTypes[voxelMap[x, y, z]].isSolid)
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public Vector3 chunkPos
    {
        get { return chunkObject.transform.position; }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    bool IsVoxelFaceHidden(Vector3 voxelPos, int faceIdx)
    {
        var voxelNeighborPos = voxelPos + VoxelData.voxelFaceDirection[faceIdx];

        int x = Mathf.FloorToInt(voxelNeighborPos.x);
        int y = Mathf.FloorToInt(voxelNeighborPos.y);
        int z = Mathf.FloorToInt(voxelNeighborPos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return world.CheckForVoxel(voxelNeighborPos + chunkPos);
        }
        else
        {
            var blockType = voxelMap[x, y, z];
            return Blocks.blockTypes[blockType].isSolid;
        }
    }

    public Blocks.BlockType GetVoxelFomGLobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int faceIdx = 0; faceIdx < 6; faceIdx++)
        {
            // if the current voxel has no solid neighboor in the current faceIdx direction then draw it
            // else, it is being hidden and we do not need to draw it
            if (!IsVoxelFaceHidden(pos, faceIdx))
            {
                // add vertex data
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIdx, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIdx, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIdx, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIdx, 3]]);

                // add texture map data
                Blocks.BlockType blockType = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
                AddTexture(Blocks.blockTypes[blockType].GetTextureId(faceIdx));

                // add triangles
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureId)
    {
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

}

public struct ChunkCoord
{
    public int x;
    public int z;

    // public ChunkCoord()
    // {
    //     x = 0;
    //     z = 0;
    // }

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public override bool Equals(object obj)
    {
        return obj is ChunkCoord coord &&
               x == coord.x &&
               z == coord.z;
    }

    public override int GetHashCode()
    {
        int hashCode = 1553271884;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + z.GetHashCode();
        return hashCode;
    }
};