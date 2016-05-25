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

    public GameObject cave { get; protected set; }
    public List<MapMeshes> generatedMeshes { get; protected set; }

    public CaveGenerator(int length, int width, float mapDensity = 0.5f, string seed = "", bool useRandomSeed = true, 
        int borderSize = 0, int squareSize = 1)
    {
        this.length = length;
        this.width = width;
        this.initialMapDensity = mapDensity;
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
        DestroyChildren();
        IMapGenerator mapGenerator = GetMapGenerator();
        Map map = mapGenerator.GenerateMap();
        GenerateMeshFromMap(map);
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

    protected MeshGenerator[] GetMeshGenerators(IList<Map> submaps)
    {
        MeshGenerator[] meshGenerators = InitializeMeshGenerators(submaps.Count);
        System.Action[] actions = new System.Action[meshGenerators.Length];
        for (int i = 0; i < meshGenerators.Length; i++)
        {
            int indexCopy = i;
            actions[i] = (() => meshGenerators[indexCopy].Generate(submaps[indexCopy]));
        }
        Utility.Threading.ParallelExecute(actions);
        return meshGenerators;
    }

    MeshGenerator[] InitializeMeshGenerators(int count)
    {
        MeshGenerator[] meshGenerators = new MeshGenerator[count];
        for (int i = 0; i < count; i++)
        {
            meshGenerators[i] = new MeshGenerator();
        }
        return meshGenerators;
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