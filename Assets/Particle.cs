using static ParticleMap;
using static Spawn;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections.Generic;
using System.Linq;

public class Particle : MonoBehaviour
{
    List<int[]> dirs = new List<int[]>
    {
        new int[] { -1, -1 }, new int[] { -1, 0 }, new int[] { -1, 1 },
        new int[] { 0, -1 }, new int[] { 0, 0 }, new int[] { 0, 1 },
        new int[] { 1, -1 }, new int[] { 1, 0 }, new int[] { 1, 1 }
    };
    public byte type;
    byte cloneType;
    int life;

    // Start is called before the first frame update
    void Start()
    {
        life = Random.Range(125, 175);
    }

    // Update is called once per frame
    void Update()
    {
        var space = GetParticleMapCoords(transform.position);
        if (particleMap[space[0], space[1], space[2]] == 0)
        {
            Destroy(gameObject);
            UpdateNumParticles(-1);
            return;
        }

        if (type == 2 && particleMap[space[0], space[1], space[2]] == 5)
            AdjustParticleBehavior(gameObject, 5);
        else if (type == 3 && particleMap[space[0], space[1], space[2]] == 12)
            AdjustParticleBehavior(gameObject, 12);
        if (type == 5 || type == 6)
            GetComponent<Light>().enabled = !lightOff;

        if (!paused || Input.GetKeyDown(KeyCode.F))
        {
            try
            {
                switch (type)
                {
                    case 2:
                        PowderUpdate(space);
                        break;
                    case 3:
                        LiquidUpdate(space);
                        break;
                    case 4:
                    case 12:
                        GasUpdate(space);
                        break;
                    case 5:
                    case 11:
                        FireUpdate(space);
                        break;
                    case 7:
                        CloneUpdate(space);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Destroy(gameObject);
                UpdateNumParticles(-1);
            }
        }
    }

    void PowderUpdate(int[] space)
    {
        byte spaceBelow = particleMap[space[0], space[1] - 1, space[2]];
        if (spaceBelow == 0 || spaceBelow == 3)
        {
            particleMap[space[0], space[1], space[2]] = spaceBelow;
            particleMap[space[0], space[1] - 1, space[2]] = 2;
            transform.position += new Vector3(0, -.01f, 0);
        }
        else
        {
            int[] dir = dirs[Random.Range(0, 9)];
            if (particleMap[space[0] + dir[0], space[1] - 1, space[2] + dir[1]] == 0)
            {
                particleMap[space[0], space[1], space[2]] = 0;
                particleMap[space[0] + dir[0], space[1] - 1, space[2] + dir[1]] = 2;
                transform.position += new Vector3(.01f * dir[0], -.01f, .01f * dir[1]);
            }
        }
    }

    void LiquidUpdate(int[] space)
    {
        if (particleMap[space[0], space[1] - 1, space[2]] == 0)
        {
            particleMap[space[0], space[1], space[2]] = 0;
            particleMap[space[0], space[1] - 1, space[2]] = 3;
            transform.position += new Vector3(0, -.01f, 0);
        }
        else
        {
            var range = Enumerable.Range(0, 9).OrderBy(i => Random.value);
            for (int i = -1; i < 1; i++)
            {
                foreach (int j in range)
                {
                    int[] dir = dirs[j];
                    if (particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] == 0)
                    {
                        particleMap[space[0], space[1], space[2]] = 0;
                        particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] = type;
                        transform.position += new Vector3(.01f * dir[0], .01f * i, .01f * dir[1]);
                        return;
                    }
                }
            }
        }
    }

    void GasUpdate(int[] space)
    {
        var yRange = new List<int>() { -1, 0, 1, 1, 1 }.OrderBy(i => Random.value);
        var dirRange = Enumerable.Range(0, 9).OrderBy(i => Random.value);
        foreach (int i in yRange)
        {
            foreach (int j in dirRange)
            {
                int[] dir = dirs[j];
                if (particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] == 0)
                {
                    particleMap[space[0], space[1], space[2]] = 0;
                    particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] = type;
                    transform.position += new Vector3(.01f * dir[0], .01f * i, .01f * dir[1]);
                    return;
                }
            }
        }
    }

    void FireUpdate(int[] space)
    {
        if (life == 75)
        {
            particleMap[space[0], space[1], space[2]] = 11;
            AdjustParticleBehavior(gameObject, 11);
        }
        else if (life == 0)
        {
            particleMap[space[0], space[1], space[2]] = 0;
            Destroy(gameObject);
            UpdateNumParticles(-1);
            return;
        }
        life--;

        var dirRange = Enumerable.Range(0, 9).OrderBy(i => Random.value);
        for (int i = 1; i >= 0; i--)
        {
            foreach (int j in dirRange)
            {
                int[] dir = dirs[j];
                if (particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] == 0)
                {
                    particleMap[space[0], space[1], space[2]] = 0;
                    particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] = type;
                    transform.position += new Vector3(.01f * dir[0], .01f * i, .01f * dir[1]);
                    goto Ignite;
                }
            }
        }

        Ignite:
        if (type == 5)
        {
            var yRange = Enumerable.Range(-1, 3).OrderBy(i => Random.value);
            foreach (int i in yRange)
            {
                foreach (int j in dirRange)
                {
                    int[] dir = dirs[j];
                    var spaceNearby = particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]];
                    if (spaceNearby == 2)
                    {
                        particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] = type;
                        return;
                    }
                    else if (spaceNearby == 3)
                    {
                        particleMap[space[0] + dir[0], space[1] + i, space[2] + dir[1]] = 12;
                        return;
                    }
                }
            }
        }
    }

    void CloneUpdate(int[] space)
    {
        byte spaceAbove = particleMap[space[0], space[1] + 1, space[2]];
        if (spaceAbove != 0)
            cloneType = spaceAbove;

        if (cloneType > 1 && cloneType < 6 || cloneType > 10)
        {
            int i = 0;
            byte nextSpace = 7;
            while (nextSpace == 7)
            {
                i++;
                nextSpace = particleMap[space[0], space[1] - i, space[2]];
            }

            if (nextSpace == 0)
            {
                particleMap[space[0], space[1] - i, space[2]] = cloneType;
                var newPosition = transform.position - new Vector3(0, .01f * i, 0);
                var particleObj = Instantiate(Resources.Load("Particle"), newPosition, Quaternion.Euler(0, 0, 0));
                AdjustParticleBehavior((GameObject)particleObj, cloneType);
                UpdateNumParticles(1);
            }
        }
    }
}
