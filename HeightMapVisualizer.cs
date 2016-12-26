/* A component that allows the visualization of heightmaps, updating instantly in response to changes
 in the parameters. The relationship between a height map's properties and the final result is quite complicated,
 so this class allows the visual exploration of that relationship.
 
  On a side note, this visualizer illustrates a benefit of completely decoupling the MeshGenerator from the 
 MapGenerator. We're able to use the MeshGenerator to visualize the mesh without having to touch anything
 in the MapGeneration namespace.*/

using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    [ExecuteInEditMode]
    public class HeightMapVisualizer : MonoBehaviour
    {
        [SerializeField] HeightMapProperties parameters;
        [SerializeField] Material material;
        [SerializeField] int size;

        Mesh mesh;

        const int MIN_SIZE = 10;
        const int MAX_SIZE = 200;
        const int DEFAULT_SIZE = 75;

        void CreateMesh()
        {
            var wallGrid = new WallGrid(new byte[size, size], Vector3.zero);
            IHeightMap heightMap = parameters.ToHeightMap();
            CaveMeshes meshes = MeshGenerator.GenerateCaveMeshes(wallGrid, CaveType.Isometric, heightMap, heightMap);
            mesh = meshes.Floor;
        }

        void Reset()
        {
            parameters = new HeightMapProperties(0);
            size = DEFAULT_SIZE;
        }

        void Update()
        {
            if (mesh != null)
            {
                Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0);
            }
        }

        void OnValidate()
        {
            parameters.OnValidate();
            size = Mathf.Clamp(size, MIN_SIZE, MAX_SIZE);
            CreateMesh();
        }
    } 
}