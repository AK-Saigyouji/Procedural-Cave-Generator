using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.Modules.CaveWalls
{
    [CreateAssetMenu(fileName = "Noisy Cave Wall", menuName = rootMenupath + "Perlin Cave Wall")]
    /// <summary>
    /// Vertices along cave wall vertices are randomly pushed in and out using lines of perlin noise.
    /// </summary>
    public sealed class CaveWallPerlin : CaveWallModule
    {
        public override int ExtraVerticesPerCorner { get { return extraVertices; } }
        [SerializeField] int extraVertices = 2;

        /// <summary>
        /// How closely together the values will be sampled from the noise distribution. Smaller magnitude means points
        /// will vary more slowly. 
        /// </summary>
        public float Scale { get { return scale; } set { scale = value; } }

        [Tooltip("How closely together the values will be sampled from the noise distribution. "
            + "Smaller magnitude means points will vary more slowly.")]
        [Range(0.2f, 4f)]
        [SerializeField] float scale = 1f;

        // chosen to make the user-selected range of scales more intuitive. 
        const float BASE_SCALE = 0.1f;

        public float MaxPushOut
        {
            get { return maxPushOut; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                maxPushOut = value;
            }
        }

        [Tooltip("How far a vertex can be pushed out along its normal. A large value may affect connectivity.")]
        [SerializeField] float maxPushOut = 1f;

        public float MaxPushIn
        {
            get { return maxPushIn; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                maxPushIn = value;
            }
        }

        [Tooltip("How far a vertex can be pushed in along its normal. A large value may result in a wall collapsing "
            + "in on itself.")]
        [SerializeField] float maxPushIn = 0f;

        public override Vector3 GetAdjustedCorner(VertexContext context)
        {
            if (context.IsCeiling || context.IsFloor)
            {
                return context.Vertex;
            }
            else
            {
                float adjustment = ComputeAdjustment(context.Vertex);
                return context.Vertex + adjustment * context.Normal;
            }
        }

        float ComputeAdjustment(Vector3 vertex)
        {
            int offset = ComputeHash(vertex);
            float range = maxPushOut + maxPushIn;
            float adjustment = Mathf.PerlinNoise(offset, offset + vertex.y * scale * BASE_SCALE) * range - maxPushIn;
            return adjustment;
        }

        static int ComputeHash(Vector3 original)
        {
            unchecked
            {
                // 929 was chosen to be a prime number, to be large enough to accommodate a wide range of values,
                // and small enough to not cause rounding errors when adding something in the range of 0.01 to 1.
                return (((int)(original.x * 10) << 16) + (int)(original.z * 10)) % 929;
            }
        }

        void OnValidate()
        {
            maxPushIn = Mathf.Max(0, maxPushIn);
            maxPushOut = Mathf.Max(0, maxPushOut);
        }
    }
}