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

namespace CaveGeneration
{
    /// <summary>
    /// Specifies the component to use for determining whether an object will collide with cave walls. 
    /// </summary>
    public enum ComponentType
    {
        Mesh,
        Collider
    }

    public sealed class CollisionTester
    {
        MeshGeneration.FloorTester tester;

        internal CollisionTester(MeshGeneration.FloorTester tester)
        {
            this.tester = tester;
        }

        /// <summary>
        /// Will the box fit in the cave without intersecting any walls?
        /// </summary>
        /// <param name="botLeft">Bottom left corner of box.</param>
        /// <param name="topRight">Top right corner of box.</param>
        public bool CanFitBox(Vector2 botLeft, Vector2 topRight)
        {
            return tester.CanFitBox(botLeft, topRight);
        }

        // There is an implicit conversion from Vector3 to Vector2, but it works by converting 
        // (x, y, z) to (x, y) instead of (x, z). Hence the need for the following overload. 

        /// <summary>
        /// Will the box fit in the cave without intersecting any walls? Ignores y-axis entirely.
        /// </summary>
        /// <param name="botLeft">Bottom left corner of box.</param>
        /// <param name="topRight">Top right corner of box.</param>
        public bool CanFitBox(Vector3 botLeft, Vector3 topRight)
        {
            return tester.CanFitBox(RemoveYComponent(botLeft), RemoveYComponent(topRight));
        }

        /// <summary>
        /// Tests whether the object will fit within the walls of the cave at the given position, using the 
        /// specified component's Bounds property (axis-aligned bounding box). Note that the game object's scale
        /// will be factored into the computation, but rotation will not be. 
        /// </summary>
        /// <param name="gameObject">The game object to be placed.</param>
        /// <param name="position">The location for the object to be placed.</param>
        /// <param name="component">What component to use to determine whether the object will fit, e.g.
        /// mesh, or collider. Must have such component on the object.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public bool CanFitObject(GameObject gameObject, Vector3 position, ComponentType component)
        {
            Bounds bounds = ExtractBounds(gameObject, component);
            bounds = ScaleBounds(bounds, gameObject.transform.localScale);
            return CanFitBoundingBox(bounds, position);
        }

        /// <summary>
        /// Can the bounding box fit within the walls at the given position? 
        /// </summary>
        /// <param name="bounds">Specificies a bounding box. Every mesh has one.</param>
        /// <param name="position">Test position - will attempt to center the object at this position.</param>
        public bool CanFitBoundingBox(Bounds bounds, Vector3 position)
        {
            Vector3 extents = bounds.extents;
            Vector3 center = bounds.center + position;

            Vector2 botLeft = new Vector2(center.x - extents.x, center.z - extents.z);
            Vector2 topRight = new Vector2(center.x + extents.x, center.z + extents.z);

            return CanFitBox(botLeft, topRight);
        }

        /// <summary>
        /// Does the cave have a floor at these coordinates? Out of range coordinates return false.
        /// </summary>
        public bool IsFloor(float x, float z)
        {
            return tester.IsFloor(x, z);
        }

        static Bounds ExtractBounds(GameObject gameObject, ComponentType source)
        {
            switch (source)
            {
                case ComponentType.Mesh:
                    return ExtractMesh(gameObject).bounds;
                case ComponentType.Collider:
                    return ExtractCollider(gameObject).bounds;
                default:
                    throw new System.ArgumentException("Unrecognized ComponentType.");
            }
        }

        static Bounds ScaleBounds(Bounds bounds, Vector3 scale)
        {
            Vector3 oldSize = bounds.size;
            bounds.size = new Vector3(oldSize.x * scale.x, oldSize.y * scale.y, oldSize.z * scale.z);
            return bounds;
        }

        static Mesh ExtractMesh(GameObject gameObject)
        {
            // This could be greatly simplified with the null propagating operator in C# 6, but alas, Unity...

            if (gameObject == null)
                throw new System.ArgumentNullException("gameObject");

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter == null)
                throw new System.ArgumentException("gameObject has no MeshFilter component.");

            Mesh mesh = meshFilter.sharedMesh;

            if (mesh == null)
                throw new System.ArgumentException("gameObject has MeshFilter with null mesh.");

            return mesh;
        }

        static Collider ExtractCollider(GameObject gameObject)
        {
            if (gameObject == null)
                throw new System.ArgumentNullException("gameObject");

            Collider collider = gameObject.GetComponent<Collider>();

            if (collider == null)
                throw new System.ArgumentException("gameObject has no Collider.");

            return collider;
        }

        static Vector2 RemoveYComponent(Vector3 original)
        {
            return new Vector2(original.x, original.z);
        }
    }
}