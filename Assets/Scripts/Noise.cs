using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    //TODO convert to simplex noise for better performance
    // smaller scale, less variance
    // larger scale, more variance
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        // bug in unity perline noise where you get the same value if you pass in a whole number, so we add 0.1f
        float x = (position.x + 0.1f) / VoxelData.ChunkWidth * scale + offset;
        float z = (position.y + 0.1f) / VoxelData.ChunkWidth * scale + offset;
        return Mathf.PerlinNoise(x, z);
    }

    // https://www.youtube.com/watch?v=Aga0TBJkchM
    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + 1.0f) * scale;
        float y = (position.y + offset + 1.0f) * scale;
        float z = (position.z + offset + 1.0f) * scale;

        // get all 3 permutations of nouse for x, y, and z
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        
        // and their inverse...
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        // derive 3d perlin from their average
        return (AB + BC + AC + BA + CB + CA) / 6f > threshold;
    }

}