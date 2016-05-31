using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CaveGenerator : MonoBehaviour
{
    [SerializeField]
    protected int length = 50;
    [SerializeField]
    protected int width = 50;
    [SerializeField]
    [Range(0.4f, 0.6f)]
    protected float initialMapDensity = 0.5f;
    [SerializeField]
    protected string seed;
    [SerializeField]
    protected bool useRandomSeed = true;
    [SerializeField]
    protected int borderSize = 0;
    [SerializeField]
    protected int squareSize = 1;
    [SerializeField]
    protected Vector2 ceilingTextureDimensions = new Vector2(100f, 100f);

    public GameObject cave { get; protected set; }
    public List<MapMeshes> generatedMeshes { get; protected set; }

    public CaveGenerator(int length, int width, float initialMapDensity = 0.5f, string seed = "", 
        bool useRandomSeed = true, int borderSize = 0, int squareSize = 1)
    {
        this.length = length;
        this.width = width;
        this.initialMapDensity = initialMapDensity;
        this.seed = seed;
        this.useRandomSeed = useRandomSeed;
        this.borderSize = borderSize;
        this.squareSize = squareSize;
    }

    /// <summary>
    /// Generates cavernous terrain and stores it in a child game object. 
    /// </summary>
    public void GenerateCave()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        DestroyChildren();
        IMapGenerator mapGenerator = GetMapGenerator();
        Map map = mapGenerator.GenerateMap();
        double time = sw.Elapsed.TotalSeconds;
        GenerateMeshFromMap(map);
        double timeTwo = sw.Elapsed.TotalSeconds - time;
        sw.Stop();
        Debug.Log(time);
        Debug.Log(timeTwo);
    }

    virtual protected IMapGenerator GetMapGenerator()
    {
        return new MapGenerator(
            length: length, 
            width: width, 
            mapDensity: initialMapDensity, 
            seed: seed, 
            useRandomSeed: useRandomSeed,
            squareSize: squareSize, 
            borderSize: borderSize
            );
    }

    abstract protected void GenerateMeshFromMap(Map map);

    protected MeshGenerator[] PrepareMeshGenerators(IList<Map> submaps)
    {
        MeshGenerator[] meshGenerators = InitializeMeshGenerators(submaps.Count);
        System.Action[] actions = new System.Action[meshGenerators.Length];
        for (int i = 0; i < meshGenerators.Length; i++)
        {
            int indexCopy = i;
            actions[i] = (() => PrepareMeshGenerator(meshGenerators[indexCopy], submaps[indexCopy]));
        }
        Utility.Threading.ParallelExecute(actions);
        return meshGenerators;
    }

    protected MeshGenerator[] InitializeMeshGenerators(int count)
    {
        MeshGenerator[] meshGenerators = new MeshGenerator[count];
        for (int i = 0; i < count; i++)
        {
            meshGenerators[i] = new MeshGenerator();
        }
        return meshGenerators;
    }

    /// <summary>
    /// Generate all the data in the MeshGenerator in preparation for the creation of meshes. Each call
    /// of this method will get distributed across threads, so override with care.
    /// </summary>
    virtual protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
    {
        meshGenerator.GenerateCeiling(map, ceilingTextureDimensions);
    }

    protected GameObject CreateObjectFromMesh(Mesh mesh, string name, GameObject parent, Material material)
    {
        GameObject newObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
        newObject.transform.parent = parent.transform;
        newObject.GetComponent<MeshFilter>().mesh = mesh;
        newObject.GetComponent<MeshRenderer>().material = material;
        return newObject;
    }

    protected GameObject CreateSector(int sectorIndex)
    {
        return CreateChild(name: "Sector " + sectorIndex, parent: cave.transform);
    }

    protected GameObject CreateChild(string name, Transform parent)
    {
        GameObject child = new GameObject(name);
        child.transform.parent = parent;
        return child;
    }

    void DestroyChildren()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
        foreach (Transform child in children)
        {
            child.parent = null;
            Destroy(child.gameObject);
        }
    }
}

/// <summary>
/// Storage class to hold generated meshes.
/// </summary>
public class MapMeshes
{
    public Mesh wallMesh { get; private set; }
    public Mesh ceilingMesh { get; private set; }

    private MapMeshes() { }

    public MapMeshes(Mesh ceilingMesh = null, Mesh wallMesh = null)
    {
        this.ceilingMesh = ceilingMesh;
        this.wallMesh = wallMesh;
    }
}