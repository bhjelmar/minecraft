using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Jobs;
using System.Linq;

// TODO: generating cave systems dont draw voxel correctly on the -x axis
// TODO: fov calculation is off by 1

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;
    public ChunkCoord playerCurrChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private JobHandle simpleJobHandle;

    public Material material;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    private bool isCreatingChunks;

    public GameObject debugScreen;

    private void Start()
    {
        Random.InitState(seed);
        GenerateWorld();

        Cursor.lockState = CursorLockMode.Locked;

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        player.transform.position = spawnPosition;
        playerLastChunkCoord = GetChunkCoordFromVector3(player.transform.position);
    }

    private void Update()
    {
        playerCurrChunkCoord = GetChunkCoordFromVector3(player.position);

        if (!playerCurrChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if(chunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine("CreateChunks");

        playerLastChunkCoord = playerCurrChunkCoord;

        if(Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(this, new ChunkCoord(x, z), true);
                activeChunks.Add(new ChunkCoord(x, z));
            }

        }
    }

    // chunk creation handled via coroutine
    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    void CheckViewDistance()
    {
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        activeChunks.Clear();

        for (int x = this.playerCurrChunkCoord.x - VoxelData.ViewDistanceInChunks; x < this.playerCurrChunkCoord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = this.playerCurrChunkCoord.z - VoxelData.ViewDistanceInChunks; z < this.playerCurrChunkCoord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                var chunkCoord = new ChunkCoord(x, z);
                if (IsChunkInWorld(chunkCoord))
                {
                    // if chunk has not yet been computed create a new one
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(this, new ChunkCoord(x, z), false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    // else, grab it from the stored chunks and enable it
                    else if (!chunks[x, z].isActive)
                        chunks[x, z].isActive = true;
                    activeChunks.Add(chunkCoord);
                }
            }
        }

        var inactiveChunks = previouslyActiveChunks.Except(activeChunks);
        foreach (ChunkCoord c in inactiveChunks)
            chunks[c.x, c.z].isActive = false;
    }


    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return Blocks.blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFomGLobalVector3(pos)].isSolid;

        return Blocks.blockTypes[GetVoxel(pos)].isSolid;
    }


    // World generation algorithm
    public Blocks.BlockType GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /* IMMUTABLE PASS */
        // If out of world, always return air
        if (!IsVoxelInWorld(pos))
            return Blocks.BlockType.AIR;
        // Bottom is always bedrock
        if (yPos == 0)
            return Blocks.BlockType.BEDROCK;

        /* BASIC TERRAIN PASS */
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        Blocks.BlockType blockType = Blocks.BlockType.AIR;

        if (yPos == terrainHeight)
        {
            blockType = Blocks.BlockType.GRASS;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            blockType = Blocks.BlockType.DIRT;
        }
        else if (yPos > terrainHeight)
        {
            return Blocks.BlockType.AIR;
        }
        else
        {
            blockType = Blocks.BlockType.STONE;
        }


        /* SECOND PASS */
        if (blockType == Blocks.BlockType.STONE)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        blockType = lode.blockType;
                    }
                }
            }
        }
        return blockType;
    }

    // Checks if coord position is within defined bounds of world edge
    bool IsChunkInWorld(ChunkCoord coord)
    {
        return (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1);
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        return pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels;
    }
}