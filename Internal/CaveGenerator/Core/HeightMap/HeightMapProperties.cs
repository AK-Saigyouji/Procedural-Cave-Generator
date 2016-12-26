/* This is a container for the properties needed to build a height map, intended to be configured solely
 through the inspector (hence why the only exposed property or method is the method to convert to a height map). 
 Height maps are unlikely candidates for objects that need to be configured dynamically at run-time, and it's 
 very easy to screw something up when doing so. For those reasons, they're not configurable. It's still possible
 to build height maps directly however.
 */

using UnityEngine;

namespace CaveGeneration
{
    /// <summary>
    /// Used to configure height maps through the inspector. 
    /// </summary>
    [System.Serializable]
    public sealed class HeightMapProperties
    {
        [Tooltip(Tooltips.HEIGHT_MAP_MIN_HEIGHT)]
        [SerializeField]
        float minHeight;

        [Tooltip(Tooltips.HEIGHT_MAP_MAX_HEIGHT)]
        [SerializeField]
        float maxHeight;

        [Tooltip(Tooltips.HEIGHT_MAP_SMOOTHNESS)]
        [Range(MIN_SMOOTHNESS, MAX_SMOOTHNESS)]
        [SerializeField]
        float smoothness;

        [Tooltip(Tooltips.HEIGHT_MAP_NUM_LAYERS)]
        [Range(MIN_NUM_LAYERS, MAX_NUM_LAYERS)]
        [SerializeField]
        int numLayers;

        [Tooltip(Tooltips.HEIGHT_MAP_CONTRIBUTION_MULT)]
        [Range(MIN_CONTRIBUTION_MULT, MAX_CONTRIBUTION_MULT)]
        [SerializeField]
        float contributionMult;

        [Tooltip(Tooltips.HEIGHT_MAP_COMPRESSION_MULT)]
        [Range(MIN_COMPRESSION_MULT, MAX_COMPRESSION_MULT)]
        [SerializeField]
        float compressionMult;

        [SerializeField]
        int seed;

        [Tooltip(Tooltips.HEIGHT_MAP_CONSTANT)]
        [SerializeField]
        bool isConstant;

        const int MIN_NUM_LAYERS = 1;
        const int MAX_NUM_LAYERS = 10;
        const float MIN_SMOOTHNESS = 5f;
        const float MAX_SMOOTHNESS = 100f;
        const float MIN_COMPRESSION_MULT = 1f;
        const float MAX_COMPRESSION_MULT = 5f;
        const float MIN_COMPRESSION = 1f;
        const float MAX_COMPRESSION = 100f;
        const float MIN_CONTRIBUTION_MULT = 0f;
        const float MAX_CONTRIBUTION_MULT = 1f;

        const int DEFAULT_NUM_LAYERS = 1;
        const bool DEFAULT_IS_CONSTANT = true;
        const float DEFAULT_COMPRESSION = 10f;
        const float DEFAULT_CONTRIBUTION_MULT = 0.5f;
        const float DEFAULT_COMPRESSION_MULT = 2f;

        internal HeightMapProperties(int startHeight = 0)
        {
            minHeight = startHeight;
            maxHeight = startHeight;

            smoothness       = DEFAULT_COMPRESSION;
            contributionMult = DEFAULT_CONTRIBUTION_MULT;
            compressionMult  = DEFAULT_COMPRESSION_MULT;
            numLayers        = DEFAULT_NUM_LAYERS;
            isConstant       = DEFAULT_IS_CONSTANT;

            seed = System.Guid.NewGuid().GetHashCode();
        }

        public void OnValidate()
        {
            maxHeight = Mathf.Max(minHeight, maxHeight); 
        }

        /// <summary>
        /// Construct a heightmap from these properties.
        /// </summary>
        public MeshGeneration.IHeightMap ToHeightMap()
        {
            if (isConstant)
            {
                return HeightMapFactory.Build(minHeight);
            }
            else
            {
                return HeightMapFactory.Build(
                    minHeight: minHeight, 
                    maxHeight: maxHeight, 
                    scale: smoothness, 
                    seed: seed, 
                    numLayers: numLayers, 
                    amplitudeFactor: contributionMult, 
                    frequencyGrowth: compressionMult
                    );
            }
        }
    } 
}