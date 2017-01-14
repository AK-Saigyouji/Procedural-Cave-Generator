/* A MonoBehaviour that allows the visualization of a heightmap. Any changes to the height map component
 will be immediately updated by the visualizer, allowing for an exploration of how the properties of a given
 height map affect the final result.*/

using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration.Modules
{
    [ExecuteInEditMode]
    public class HeightMapVisualizer : MonoBehaviour
    {
        [SerializeField] HeightMapModule heightMapComponent;
        [SerializeField] Material material;
        [SerializeField] int size;
        [SerializeField] int scale;
        
        Mesh mesh;

        const int MIN_SIZE = 10;
        const int MAX_SIZE = 200;
        const int DEFAULT_SIZE = 75;

        const int MIN_SCALE = 1;
        const int DEFAULT_SCALE = 1;

        void CreateMesh()
        {
            var wallGrid = new WallGrid(new byte[size, size], Vector3.zero, scale);
            IHeightMap heightMap = heightMapComponent.GetHeightMap();
            mesh = MeshBuilder.BuildFloor(wallGrid, heightMap).CreateMesh();
        }

        void Reset()
        {
            size = DEFAULT_SIZE;
            scale = DEFAULT_SCALE;
        }

        void Update()
        {
            if (!Application.isPlaying) // Only draw in editor.
            {
                if (heightMapComponent != null)
                {
                    CreateMesh();
                    Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0);
                }
            }
        }

        void OnValidate()
        {
            size = Mathf.Clamp(size, MIN_SIZE, MAX_SIZE);
            scale = Mathf.Max(scale, MIN_SCALE);
        }
    } 
}