using System;
using UnityEngine;

namespace AKSaigyouji.HeightMaps
{
    [Serializable]
    public sealed class LayeredNoiseParameters
    {
        [Tooltip(MIN_HEIGHT_TOOLTIP)]
        [SerializeField]
        float minHeight;
        public float MinHeight { get { return minHeight; } }

        [Tooltip(MAX_HEIGHT_TOOLTIP)]
        [SerializeField]
        float maxHeight;
        public float MaxHeight { get { return maxHeight; } }

        [Tooltip(SMOOTHNESS_TOOLTIP)]
        [Range(MIN_SMOOTHNESS, MAX_SMOOTHNESS)]
        [SerializeField]
        float smoothness;
        public float Smoothness { get { return smoothness; } }

        [Tooltip(NUM_LAYERS_TOOLTIP)]
        [Range(MIN_NUM_LAYERS, MAX_NUM_LAYERS)]
        [SerializeField]
        int numLayers;
        public int NumLayers { get { return numLayers; } }

        [Tooltip(CONTRIBUTION_MULT_TOOLTIP)]
        [Range(MIN_CONTRIBUTION_MULT, MAX_CONTRIBUTION_MULT)]
        [SerializeField]
        float contributionMult;
        public float ContributionMult { get { return contributionMult; } }

        [Tooltip(HEIGHT_MAP_COMPRESSION_MULT)]
        [Range(MIN_COMPRESSION_MULT, MAX_COMPRESSION_MULT)]
        [SerializeField]
        float compressionMult;
        public float CompressionMult { get { return compressionMult; } }

        const string MIN_HEIGHT_TOOLTIP =
            @"Minimum y-value for the height map.";

        const string MAX_HEIGHT_TOOLTIP =
            @"Maximum y-value for the height map.";

        const string SMOOTHNESS_TOOLTIP =
            @"How smooth the height map is initially - think rolling hills vs jagged mountains.";

        const string NUM_LAYERS_TOOLTIP =
            @"The number of height maps to stack onto eachother, each rockier and with smaller contribution to add
finer variations. Set the next two properties to control the contribution of each layer.";

        const string CONTRIBUTION_MULT_TOOLTIP =
            @"What proportion of each subsequent layer to use. e.g. 0.5 means each layer provides half the contribution
 of the previous layer.";

        const string HEIGHT_MAP_COMPRESSION_MULT =
            @"How much rockier (less smooth) each layer becomes.";

        const int MIN_NUM_LAYERS = 1;
        const int DEFAULT_NUM_LAYERS = 1;
        const int MAX_NUM_LAYERS = 10;

        const float MIN_SMOOTHNESS = 5f;
        const float DEFAULT_SMOOTHNESS = 10f;
        const float MAX_SMOOTHNESS = 100f;

        const float MIN_COMPRESSION_MULT = 1f;
        const float DEFAULT_COMPRESSION_MULT = 2f;
        const float MAX_COMPRESSION_MULT = 5f;

        const float MIN_CONTRIBUTION_MULT = 0f;
        const float DEFAULT_CONTRIBUTION_MULT = 0.5f;
        const float MAX_CONTRIBUTION_MULT = 1f;

        const float DEFAULT_MIN_HEIGHT = 3f;
        const float DEFAULT_MAX_HEIGHT = 5f;

        public LayeredNoiseParameters()
        {
            minHeight        = DEFAULT_MIN_HEIGHT;
            maxHeight        = DEFAULT_MAX_HEIGHT;
            smoothness       = DEFAULT_SMOOTHNESS;
            contributionMult = DEFAULT_CONTRIBUTION_MULT;
            compressionMult  = DEFAULT_COMPRESSION_MULT;
            numLayers        = DEFAULT_NUM_LAYERS;
        }

        /// <summary>
        /// Set the min and max heights. Min height can't be greater than max height.
        /// </summary>
        public void SetHeightRange(float minHeight, float maxHeight)
        {
            if (minHeight > maxHeight)
                throw new ArgumentException("minHeight cannot be greater than maxHeight");

            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        /// <summary>
        /// The height map is defined by stacking multiple layers of perlin noise on top of each other. This method 
        /// sets the number of layers and how each layer changes. 1 layer is normal perlin noise, and will result 
        /// in the other two parameters being ignored. A contribution multiplier of 0.5f 
        /// and a compression multiplier of 2f are recommended.
        /// </summary>
        /// <param name="numLayers">How many layers to use. Value will be clamped to between 1 and 10.</param>
        /// <param name="contributionMult">What proportion of the contribution from subsequent layers to use.
        /// e.g. 0.5 means each layer contributes half of the previous layer. Value will be clamped to between 0 and 1.</param>
        /// <param name="compressionMult">How much more compressed/rocky each layer becomes. Value will be clamped
        /// to between 1 and 5. </param>
        public void SetLayers(int numLayers, float contributionMult, float compressionMult)
        {
            this.numLayers = Mathf.Clamp(numLayers, MIN_NUM_LAYERS, MAX_NUM_LAYERS);
            this.contributionMult = Mathf.Clamp(contributionMult, MIN_CONTRIBUTION_MULT, MAX_CONTRIBUTION_MULT);
            this.compressionMult = Mathf.Clamp(compressionMult, MIN_COMPRESSION_MULT, MAX_COMPRESSION_MULT);
        }

        /// <param name="smoothness">Clamps to between 5 and 100.</param>
        public void SetSmoothness(float smoothness)
        {
            this.smoothness = Mathf.Clamp(smoothness, MIN_SMOOTHNESS, MAX_SMOOTHNESS);
        }

        public void OnValidate()
        {
            maxHeight = Mathf.Max(minHeight, maxHeight); 
        }
    } 
}