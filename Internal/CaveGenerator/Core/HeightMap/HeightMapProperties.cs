using UnityEngine;

namespace CaveGeneration
{
    /// <summary>
    /// Used to configure height maps through the inspector. 
    /// </summary>
    [System.Serializable]
    public sealed class HeightMapProperties
    {
        [Tooltip(Tooltips.HEIGHT_MAP_CONSTANT)]
        [SerializeField]
        bool constant;

        [Tooltip(Tooltips.HEIGHT_MAP_MIN_HEIGHT)]
        [SerializeField]
        float minHeight;

        [Tooltip(Tooltips.HEIGHT_MAP_MAX_HEIGHT)]
        [SerializeField]
        float maxHeight;

        [Tooltip(Tooltips.HEIGHT_MAP_SMOOTHNESS)]
        [SerializeField]
        float smoothness;

        [Tooltip(Tooltips.HEIGHT_MAP_NUM_LAYERS)]
        [SerializeField]
        int numLayers;

        [Tooltip(Tooltips.HEIGHT_MAP_AMP_DECAY)]
        [Range(MIN_CONTRIBUTION_MULT, MAX_CONTRIBUTION_MULT)]
        [SerializeField]
        float contributionMult;

        [Tooltip(Tooltips.HEIGHT_MAP_COMPRESSION_MULT)]
        [SerializeField]
        float compressionMult;

        const int MIN_NUM_LAYERS = 1;
        const int MAX_NUM_LAYERS = 10;
        const int MIN_COMPRESSION_MULT = 1;
        const float MIN_COMPRESSION = 1f;
        const float MAX_COMPRESSION = 100f;
        const float MIN_CONTRIBUTION_MULT = 0f;
        const float MAX_CONTRIBUTION_MULT = 1f;

        const float DEFAULT_COMPRESSION = 10f;
        const float DEFAULT_CONTRIBUTION_MULT = 0.5f;
        const int DEFAULT_COMPRESSION_MULT = 2;
        const int DEFAULT_NUM_LAYERS = 1;

        internal HeightMapProperties(int startHeight = 0)
        {
            minHeight = startHeight;
            maxHeight = startHeight;
            smoothness = DEFAULT_COMPRESSION;
            contributionMult = DEFAULT_CONTRIBUTION_MULT;
            compressionMult = DEFAULT_COMPRESSION_MULT;
            numLayers = DEFAULT_NUM_LAYERS;
            constant = true;
        }

        public void OnValidate()
        {
            maxHeight        = Mathf.Max(minHeight, maxHeight); 
            compressionMult  = Mathf.Max(compressionMult, MIN_COMPRESSION_MULT);
            smoothness      = Mathf.Clamp(smoothness, MIN_COMPRESSION, MAX_COMPRESSION);
            numLayers        = Mathf.Clamp(numLayers, MIN_NUM_LAYERS, MAX_NUM_LAYERS);
            contributionMult = Mathf.Clamp(contributionMult, MIN_CONTRIBUTION_MULT, MAX_CONTRIBUTION_MULT);
            if (constant)
            {
                maxHeight = minHeight;
            }
        }

        /// <summary>
        /// Construct a heightmap from these properties.
        /// </summary>
        public MeshGeneration.IHeightMap ToHeightMap(int seed)
        {
            if (constant)
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