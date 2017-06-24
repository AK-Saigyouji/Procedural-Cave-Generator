/* This class offers methods for testing whether objects will intersect the walls of a cave. Its primary purpose
 is to allow placing content at run-time without knowing ahead of time the structure of a cave. To this end
 it supports various tests for collision which are optimized to allow thousands of executions 
 per frame. To achieve this performance, collisions use axis-aligned bounding boxes (AABB), represented in
 Unity by the Bounds struct. These are found on every mesh and collider, and can also be built independently, 
 offering good flexibility and performance at the expense of some accuracy for objects not shaped like 
 boxes parallel to the axes.
 
 Note that we use the efficient primitives from the FloorTester class as the basis behind the functionality in this
 class. */

using UnityEngine;
using AKSaigyouji.MeshGeneration;

namespace AKSaigyouji.CaveGeneration
{
    public sealed class CollisionTester
    {
        FloorTester tester;

        internal CollisionTester(FloorTester tester)
        {
            this.tester = tester;
        }

        /// <summary>
        /// Will the box fit in the cave without intersecting any walls? Box is specified by its four coordinates.
        /// </summary>
        /// <param name="x1">Either the leftmost or rightmost x coordinate of the box.</param>
        /// <param name="x2">The other x coordinate of the box.</param>
        /// <param name="z1">Either the leftmost or rightmost z coordinate of the box.</param>
        /// <param name="z2">The other z coordinate of the box.</param>
        public bool CanFitBox(float x1, float x2, float z1, float z2)
        {
            return tester.CanFitBox(x1, x2, z1, z2);
        }

        /// <summary>
        /// Tests whether the object will fit within the walls of the cave at the given position, using the 
        /// gameObject's MeshRenderer component.
        /// </summary>
        /// <param name="gameObject">The game object to be placed. Must have a MeshRenderer component.</param>
        /// <param name="position">The location for the object to be placed.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public bool CanFitObject(GameObject gameObject, Vector3 position)
        {
            Bounds bounds = ExtractBounds(gameObject);
            return CanFitBoundingBox(bounds, position);
        }

        /// <summary>
        /// Can the bounding box fit within the walls at the given position? Uses the Bound's size, but not
        /// its center - uses the position instead.
        /// </summary>
        /// <param name="bounds">Specificies a bounding box.</param>
        /// <param name="position">Test position - will attempt to center the object at this position.</param>
        public bool CanFitBoundingBox(Bounds bounds, Vector3 position)
        {
            Vector3 extents = bounds.extents;

            float left = position.x - extents.x;
            float right = position.x + extents.x;
            float bot = position.z - extents.z;
            float top = position.z + extents.z;

            return CanFitBox(left, right, bot, top);
        }

        /// <summary>
        /// Does the cave have a floor at these coordinates? Out of range coordinates return false.
        /// </summary>
        public bool IsFloor(float x, float z)
        {
            return tester.IsFloor(x, z);
        }

        static Bounds ExtractBounds(GameObject gameObject)
        {
            if (gameObject == null)
                throw new System.ArgumentNullException("gameObject");

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
                throw new System.ArgumentException("Must have MeshRenderer component.", "gameObject");

            return meshRenderer.bounds;
        }
    }
}