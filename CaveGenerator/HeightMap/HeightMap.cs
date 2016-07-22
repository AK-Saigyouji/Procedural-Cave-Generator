using UnityEngine;
using System.Collections;

namespace CaveGeneration
{
    public abstract class HeightMap : MonoBehaviour, IHeightMap
    {
        public float maxHeight;

        [Tooltip("Level of smoothness of height map - think rolling hills vs jagged mountains.")]
        public float scale;

        [Tooltip("The number of height maps to stack onto eachother, each more compressed and with smaller contribution.")]
        public int numLayers;

        [Tooltip(@"How quickly the contribution of subsequent layers is reduced. 1 yields full contribution for each layer,
        0 yields no contribution after the first layer")]
        [Range(0.001f, 1f)]
        public float amplitudeDecay;

        [Tooltip("How much more compressed each subsequent layer becomes.")]
        public float frequencyGrowth;

        protected Noise noise;

        public void Create(int seed)
        {
            noise = new Noise(numLayers, amplitudeDecay, frequencyGrowth, scale, seed);
        }

        public virtual float GetHeight(float x, float y)
        {
            return maxHeight * noise.GetHeight(x, y);
        }

        void OnValidate()
        {
            if (numLayers < 1)
            {
                numLayers = 1;
            }
            if (maxHeight <= 0)
            {
                maxHeight = 0.001f;
            }
            if (scale <= 0)
            {
                scale = 0.0001f;
            }
            if (frequencyGrowth < 1)
            {
                frequencyGrowth = 1;
            }
        }
    } 
}
