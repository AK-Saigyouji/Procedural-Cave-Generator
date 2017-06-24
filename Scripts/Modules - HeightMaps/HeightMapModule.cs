﻿using AKSaigyouji.HeightMaps;

namespace AKSaigyouji.Modules.HeightMaps
{
    public abstract class HeightMapModule : Module
    {
        protected const string fileName = "HeightMap";
        protected const string rootMenuPath = "Cave Generation/Height Maps/";

        public abstract IHeightMap GetHeightMap();

        public IHeightMap GetHeightMap(int seed)
        {
            Seed = seed;
            return GetHeightMap();
        }
    } 
}
