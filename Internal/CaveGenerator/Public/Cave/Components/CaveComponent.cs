using UnityEngine;

public abstract class CaveComponent
{
    public string Name { get; private set; }
    public GameObject GameObject { get; private set; }

    public Mesh Mesh { get { return meshFilter.mesh; } }

    public Material Material
    {
        get { return meshRenderer.material; }
        set { meshRenderer.material = value; }
    }

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    internal CaveComponent(Mesh mesh, string name)
    {
        Name = name;
        mesh.name = name;

        GameObject = BuildGameObject(mesh);
    }

    GameObject BuildGameObject(Mesh mesh)
    {
        var gameObject = new GameObject(Name);
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return gameObject;
    }

    protected void AddMeshCollider()
    {
        MeshCollider collider = GameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = Mesh;
    }
}
