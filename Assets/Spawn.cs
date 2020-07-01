using System;
using static ParticleMap;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Spawn : MonoBehaviour
{
    byte particleType = 2;
    float brushSize = 0;
    public static bool paused = false;
    public static bool lightOff = false;
    static int numParticles = 0;

    // Start is called before the first frame update
    void Start()
    {
        InitParticleMap();
        GameObject.Find("Type2").GetComponent<Button>().interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (Input.GetKeyDown(KeyCode.P))
        {
            paused = !paused;
            GameObject.Find("Paused").GetComponent<Text>().enabled = paused;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            InitParticleMap();
            var particles = Resources.FindObjectsOfTypeAll<Particle>()
                .Where(particle => particle.gameObject.name.Contains("Clone"));
            foreach (var particle in particles)
                Destroy(particle.gameObject);
            UpdateNumParticles(-numParticles);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            lightOff = !lightOff;
            GameObject.Find("LightOff").GetComponent<Text>().enabled = lightOff;
        }

        byte origParticleType = particleType;
        for (byte i = 1; i <= 7; i++)
        {
            if (Input.GetKeyDown((KeyCode)i + 48))
                particleType = i;
        }
        if (particleType != origParticleType)
        {
            GameObject.Find("Type" + origParticleType.ToString()).GetComponent<Button>().interactable = true;
            GameObject.Find("Type" + particleType.ToString()).GetComponent<Button>().interactable = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0 && brushSize < .04f)
            brushSize += .01f;
        else if (scroll < 0 && brushSize > 0)
            brushSize -= .01f;

        var position = Camera.main.transform.position;
        var cameraFwd = Camera.main.transform.forward;
        for (int i = 0; i <= 200; i++)
        {
            position += cameraFwd * .01f;
            var space = GetParticleMapCoords(position);
            if (space == null || particleMap[space[0], space[1], space[2]] == 1 && space[1] == 0)
            {
                position -= cameraFwd * (brushSize + .01f);
                break;
            }
        }

        var ghost = GameObject.Find("Ghost");
        ghost.transform.position = position;
        ghost.transform.localScale = Vector3.one * (brushSize * 2 + .01f);

        position = new Vector3(
            (float)Math.Round(position.x, 2),
            (float)Math.Round(position.y, 2),
            (float)Math.Round(position.z, 2)
        );

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            for (float x = position.x - brushSize; x <= position.x + brushSize; x += .01f)
            {
                for (float y = position.y - brushSize; y <= position.y + brushSize; y += .01f)
                {
                    for (float z = position.z - brushSize; z <= position.z + brushSize; z += .01f)
                    {
                        var newPosition = new Vector3(x, y, z);
                        var space = GetParticleMapCoords(newPosition);

                        if (space != null)
                        {
                            if (Input.GetMouseButton(0))
                            {
                                if (particleMap[space[0], space[1], space[2]] == 0)
                                {
                                    particleMap[space[0], space[1], space[2]] = particleType;
                                    var particleObj = Instantiate(Resources.Load("Particle"), newPosition, Quaternion.Euler(0, 0, 0));
                                    AdjustParticleBehavior((GameObject)particleObj, particleType);
                                    UpdateNumParticles(1);
                                }
                            }
                            else if (particleMap[space[0], space[1], space[2]] > 0 && space[1] != 0)
                                particleMap[space[0], space[1], space[2]] = 0;
                        }
                    }
                }
            }
        }
    }

    public static Color GetParticleColor(byte particleType)
    {
        switch (particleType)
        {
            case 1:
                return new Color(.5f, .5f, .5f);
            case 2:
                return new Color(1, 1, .6f);
            case 3:
                return new Color(.4f, .4f, 1, .5f);
            case 4:
                return new Color(.9f, .8f, .4f, .2f);
            case 5:
                return new Color(1, .7f, 0, .5f);
            case 6:
                return new Color(0, .5f, 1);
            case 7:
                return new Color(.7f, 0, 0);
            case 11:
                return new Color(.5f, .5f, .5f, .2f);
            case 12:
                return new Color(.4f, .4f, 1, .2f);
            default:
                return Color.clear;
        }
    }

    public static void AdjustParticleBehavior(GameObject particleObj, byte particleType)
    {
        particleObj.GetComponent<Particle>().type = particleType;
        var renderer = particleObj.GetComponent<Renderer>();

        if (particleType == 4 || particleType == 12)
            particleObj.GetComponent<Collider>().enabled = false;

        var color = GetParticleColor(particleType);
        if (particleType == 5 || particleType == 6)
        {
            particleObj.GetComponent<Collider>().enabled = particleType == 6;
            particleObj.GetComponent<Light>().color = color;
            renderer.material = Resources.Load<Material>("LightMaterial");
            renderer.material.SetColor("_EmissionColor", color);
        }
        else
        {
            renderer.material = Resources.Load<Material>("Material");
            renderer.material.color = color;
        }
    }

    public static void UpdateNumParticles(int n)
    {
        numParticles += n;
        GameObject.Find("NumParticles").GetComponent<Text>().text = "# Particles: " + numParticles.ToString();
    }
}
