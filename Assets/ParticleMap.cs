using System;
using UnityEngine;

public static class ParticleMap
{
    public static byte[,,] particleMap;

    public static void InitParticleMap()
    {
        particleMap = new byte[1000, 1000, 1000];
        for (int x = 0; x < 1000; x++)
        {
            for (int z = 0; z < 1000; z++)
                particleMap[x, 0, z] = 1;
        }
    }

    public static int[] GetParticleMapCoords(Vector3 vector)
    {
        int[] array = new int[] {
            Convert.ToInt32((vector.x + 5) * 100),
            Convert.ToInt32(vector.y * 100),
            Convert.ToInt32((vector.z + 5) * 100)
        };
        return (array[0] < 0 || array[0] > 999 || array[1] < 0 || array[1] > 999 || array[2] < 0 || array[2] > 999)
            ? null : array;
    }
}
