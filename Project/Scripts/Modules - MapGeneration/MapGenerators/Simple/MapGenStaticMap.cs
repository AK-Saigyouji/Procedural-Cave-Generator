using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    /// <summary>
    /// A generator that holds a single map. It will always generate that same map.
    /// </summary>
    [CreateAssetMenu(fileName = "Static Map Holder", menuName = rootMenupath + "Static Map Holder")]
    public sealed class MapGenStaticMap : MapGenModule
    {
        [SerializeField] Texture2D texture;

        public override Map Generate()
        {
            Validate();
            return Map.FromTexture(texture);
        }

        public override Coord GetMapSize()
        {
            Validate();
            return new Coord(texture.width, texture.height);
        }

        public void AssignMap(Map map)
        {
            texture = map.ToTexture();
        }

        public void AssignMap(Texture2D map)
        {
            texture = map;
        }

        public static implicit operator MapGenStaticMap(Map map)
        {
            return Construct(map);
        }

        public static implicit operator MapGenStaticMap(Texture2D map)
        {
            return Construct(map);
        }

        public static MapGenStaticMap Construct(Texture2D map)
        {
            var module = CreateInstance<MapGenStaticMap>();
            module.AssignMap(map);
            return module;
        }

        public static MapGenStaticMap Construct(Map map)
        {
            var module = CreateInstance<MapGenStaticMap>();
            module.AssignMap(map);
            return module;
        }

        void Validate()
        {
            if (texture == null)
            {
                throw new InvalidOperationException("Map not assigned.");
            }
        }
    }
}