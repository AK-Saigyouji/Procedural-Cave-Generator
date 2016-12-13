/* This simple object serves as a package for the result of the cave generator, containing the generated cave itself,
 information about how it was configured, and any additional utility objects tied to the instance itself.*/

using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// A cave produced by the CaveGeneration system. 
    /// </summary>
    public sealed class Cave
    {
        public GameObject      GameObject      { get; private set; }
        public CollisionTester CollisionTester { get; private set; }
        public MapParameters   MapParameters   { get; private set; }

        internal Cave(GameObject caveGameObject, CollisionTester collisionTester, MapParameters mapParameters)
        {
            GameObject = caveGameObject;
            CollisionTester = collisionTester;
            MapParameters = mapParameters;
        }
    } 
}