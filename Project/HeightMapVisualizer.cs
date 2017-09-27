/* A MonoBehaviour that allows the visualization of a heightmap. Any changes to the height map component
 will be immediately updated by the visualizer, allowing for a rapid exploration of how the properties of a given
 height map affect the final result. 
 
  This takes an arbitrary HeightMapModule, which means it will work for custom modules as well. 
 
  Note that this script will destroy itself if it's used in a live build (i.e. it's editor-only)*/

using AKSaigyouji.MeshGeneration;
using AKSaigyouji.HeightMaps;
using UnityEngine;

namespace AKSaigyouji.Modules.HeightMaps
{
    [ExecuteInEditMode]
    public sealed class HeightMapVisualizer : MonoBehaviour
    {
        [SerializeField] HeightMapModule heightMapModule;
        [SerializeField] Material material;
        [SerializeField] int size;
        [SerializeField] int scale;

        readonly MeshGenerator meshGenerator = new MeshGenerator();

        const int MIN_SIZE = 10;
        const int MAX_SIZE = 200;
        const int DEFAULT_SIZE = 75;

        const int MIN_SCALE = 1;
        const int DEFAULT_SCALE = 1;

        Mesh CreateMesh()
        {
            var wallGrid = new WallGrid(new byte[size, size], Vector3.zero, scale);
            IHeightMap heightMap = heightMapModule.GetHeightMap();
            MeshData preMesh = meshGenerator.BuildFloor(wallGrid, heightMap);
            return preMesh.CreateMesh();
        }

        void Awake()
        {
            // if this class somehow ends up in a build, then it will only waste resources, so it automatically 
            // destroys itself. 
            #if !UNITY_EDITOR
                Destroy(this);
            #endif
        }

        void Update()
        {
            if (CanDraw()) 
            {
                Mesh mesh = CreateMesh();
                Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0);
            }
        }

        void Reset()
        {
            size = DEFAULT_SIZE;
            scale = DEFAULT_SCALE;
        }

        bool CanDraw()
        {
            return !Application.isPlaying && heightMapModule != null && material != null;
        }

        void OnValidate()
        {
            size = Mathf.Clamp(size, MIN_SIZE, MAX_SIZE);
            scale = Mathf.Max(scale, MIN_SCALE);
        }
    } 
}