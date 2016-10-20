/* Height map builders are MonoBehaviours that produce an IHeightMap object based on parameters set in the inspector.*/

using UnityEngine;

using IHeightMap = CaveGeneration.MeshGeneration.IHeightMap;

namespace CaveGeneration.HeightMaps
{
    /// <summary>
    /// Builds a height map out of one or more layers of perlin noise.
    /// </summary>
    public abstract class HeightMapBuilder : MonoBehaviour
    {
        // This represents how much the height map can deviate from the base height of the walls
        public float maxHeight;

        [Tooltip(Tooltips.HEIGHT_MAP_SCALE)]
        public float scale;

        [Tooltip(Tooltips.HEIGHT_MAP_NUM_LAYERS)]
        public int numLayers;

        [Tooltip(Tooltips.HEIGHT_MAP_AMP_DECAY)]
        [Range(MIN_AMPLITUDE_DECAY, MAX_AMPLITUDE_DECAY)]
        public float amplitudeDecay;

        [Tooltip(Tooltips.HEIGHT_MAP_FREQ_GROWTH)]
        public float frequencyGrowth;

        [Tooltip(Tooltips.HEIGHT_MAP_VISUALIZE)]
        public bool visualize = false;

        const int MIN_NUM_LAYERS = 1;
        const float MIN_MAX_HEIGHT = 0.001f;
        const float MIN_SCALE = 0.001f;
        const int MIN_FREQUENCY_GROWTH = 1;
        const float MIN_AMPLITUDE_DECAY = 0.001f;
        const float MAX_AMPLITUDE_DECAY = 1f;

        HeightMapDrawer drawer;

        /// <summary>
        /// Once parameter selection is finalized, use this method to produce the height map.
        /// </summary>
        public IHeightMap Build(int seed, int baseHeight)
        {
            float amplitudePersistance = 1 - amplitudeDecay;
            LayeredNoise noise = new LayeredNoise(numLayers, amplitudePersistance, frequencyGrowth, scale, seed);
            return new HeightMap(noise, baseHeight, maxHeight);
        }

        void OnValidate()
        {
            if (numLayers < MIN_NUM_LAYERS)
            {
                numLayers = MIN_NUM_LAYERS;
            }
            if (maxHeight <= MIN_MAX_HEIGHT)
            {
                maxHeight = MIN_MAX_HEIGHT;
            }
            if (scale <= MIN_SCALE)
            {
                scale = MIN_SCALE;
            }
            if (frequencyGrowth < MIN_FREQUENCY_GROWTH)
            {
                frequencyGrowth = MIN_FREQUENCY_GROWTH;
            }
            if (visualize)
            {
                UpdateDrawer();
            }
        }

        void UpdateDrawer()
        {
            IHeightMap heightMap = Build(seed: 0, baseHeight: 0);
            if (drawer == null)
            {
                drawer = new HeightMapDrawer();
            }
            drawer.BuildMesh(heightMap);
        }

        void OnDrawGizmos()
        {
            if (visualize)
            {
                Gizmos.DrawMesh(drawer.mesh);
            }
        }
    } 
}
