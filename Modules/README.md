# Modules

This folder contains modules: customizable Scriptable Objects that define core parts of the cave generator. Currently there are two types of modules: map generators and height maps.

## Usage

Either use a sample module provided, or create one using the menu. Samples can be found in the relevant folders in this directory. To create one using the menu, select Assets -> Create -> Cave Generation and select the relevant module. Modules offer a number of customizable properties unique to their type. 

You may wish to create several versions of the same type of module, and name them to reflect their distinct customization. As a simple example, you could have multiple map generators of the same type named Dense, Standard, and Sparse, configured with relatively high, balanced and low wall densities respectively. They can then be loaded dynamically at run-time to generate caves on demand, rather than generating all the caves before-hand. Saving a cave as a prefab requires saving their meshes, and meshes are very large objects (i.e. take a lot of memory). 

## Defining your own modules

In addition to using and customizing the provided modules, you can write your own based on custom logic and plug them into the cave generator. The rest of this readme covers this process. 

### Define a new height map module

In this example, we'll write a new height map module to illustrate how it's done. We'll need to create a new C# script and do three things: extend the HeightMapModule class, implement the GetHeightMap() method, and add a CreateAssetMenu attribute.

As a simple example to illustrate the process, we'll create a sinusoidal height map (i.e. based on the Sin function). 

Here's a template for implementing a new height map module:

```
using System;
using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + menuName)]
    public class HeightMapNewType : HeightMapModule
    {
        // This string determines the name of this type of height map in the menu.
        const string menuName = "NewType";

        // Insert properties to be set in the inspector here

        public override IHeightMap GetHeightMap()
        {
            throw new NotImplementedException();
        }
    }
}
```

To build the heightmap, we need a height function that will take two floats and return a float: i.e. a function that determines a height value at every (x, z) coordinate pair. We can use the following for our sin-based height map:

```
float GetHeight(float x, float z)
{
    return 0.5f * (Mathf.Sin(x) + Mathf.Sin(z));
}
```

We could construct an IHeightMap directly by implementing a class that implements the interface, but it's easier to use the HeightMapFactory class which will take care of that for us. Its Build method has the following overload:

```
public IHeightMap Build(Func<float, float, float> heightFunction, float minHeight, float maxHeight);
```

Min and max height should be set to the lower and upper bounds of our function: the result of the height function will be clamped between those values.

Naming our class more appropriately (note: script file must match class name) and using this helper class, we have the following:

```
namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + menuName)]
    public class HeightMapSinusoidal : HeightMapModule
    {
        // This string determines the name of this type of height map in the menu.
        const string menuName = "Sinusoidal";

        // Insert properties to be set in the inspector here

        public override IHeightMap GetHeightMap()
        {
            return HeightMapFactory.Build(GetHeight, -1, 1);
        }

        float GetHeight(float x, float z)
        {
            return 0.5f * (Mathf.Sin(x) + Mathf.Sin(z));
        }
    }
}
```

We can now return to the editor and select Assets -> Create -> Cave Generation -> Height Maps -> Sinusoidal and this will create a new height map for us, which can be plugged into the Cave Generator.

This is pretty limited, however, as we can't configure any of this height map's properties through the inspector. To illustrate how to do that, let's add three features to our height map: base height, which will shift the height map up or down. Amplitude, which will stretch our height map in the y-direction. And frequency, which will compress our height map in the xz plane. The way we do this is no different from exposing properties on a MonoBehaviour. Here is a simple implementation:

```
using System;
using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + menuName)]
    public class HeightMapSinusoidal : HeightMapModule
    {
        // This string determines the name of this type of height map in the menu.
        const string menuName = "Sinusoidal";

        // Insert properties to be set in the inspector here
        public float baseHeight = 0f;
        public float amplitude = 1f;
        public float frequency = 1f;

        public override IHeightMap GetHeightMap()
        {
            return HeightMapFactory.Build(GetHeight, baseHeight - amplitude, baseHeight + amplitude);
        }

        float GetHeight(float x, float z)
        {
            x *= frequency;
            z *= frequency;
            return baseHeight + amplitude * 0.5f * (Mathf.Sin(x) + Mathf.Sin(z));
        }
    }
}
```

Now each height map of this type that we create through the Assets menu can be independently configured with its own base height, amplitude and frequency. 

### Define a new map generator module

Defining a map generator module is very similar to defining a new height map module, but instead of implementing HeightMapModule you need to implement MapGenModule, and instead of overriding GetHeightMap, you need to override the Generate method, which returns a Map object.

The Map class is a 2D array of Tiles, where Tile is an enum taking the values of either Wall or Floor. A number of helper methods have been provided to facilitate their creation.

As a sample, we'll go over the creation of a new type of map generator. Instead of being based on cellular automata like the default one, we'll create a map generator based on perlin noise. Here's our starting point:

```
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MapGenPerlinNoise : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "Perlin Noise";

        // Insert properties to be set in the inspector here
        
        public override Map Generate()
        {
            throw new System.NotImplementedException();
        }
    }
}
```

Given the simple nature of Maps, we'll create one directly rather than delegating to a factory method. We can work with the Map much like we would with an ordinary 2D array, starting by initializing its length and width (we'll set them to 50 for now):

```
Map map = new Map(50, 50);
```

Perlin Noise takes two parameters (an x and a y) and returns a float between 0 and 1. For now, let's say that floats above 0.5 are walls, and below 0.5 are floors. For the x and y, we can use the integer coordinates of the map, but we do need to scale them: Perlin Noise is intended
to be sampled at values that are much closer together. 

```
float scale = 0.1f;
for (int y = 0; y < map.Width; y++)
{
    for (int x = 0; x < map.Length; x++)
    {
        float noiseValue = Mathf.PerlinNoise(scale * x, scale * y);
        map[x, y] = noiseValue < 0.5f ? Tile.Wall : Tile.Floor;
    }
}
```

Putting this together, we have:

```
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MapGenPerlinNoise : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "Perlin Noise";

        // Insert properties to be set in the inspector here
        
        public override Map Generate()
        {
            Map map = new Map(50, 50);
            float scale = 0.1f;
            for (int y = 0; y < map.Width; y++)
            {
                for (int x = 0; x < map.Length; x++)
                {
                    float noiseValue = Mathf.PerlinNoise(scale * x, scale * y);
                    map[x, y] = noiseValue < 0.5f ? Tile.Wall : Tile.Floor;
                }
            }
            return map;
        }
    }
}
```

At this point we can go to Assets -> Create -> Cave Generation -> Map Generators -> Perlin Noise to create a Perlin Noise map generator, and plug it into the cave generator. Enough has been covered thus far to build your own map generators. The rest of the section will be spent on improving this example and covering additional functionality you may wish to use.

First, we probably want to put some kind of boundary around our map so that when a player reaches an edge, they don't stare off into the void (or the defualt Unity skybox, in this case). We could do something like this:

```
for (int x = 0; x < map.Length; x++)
{
    map[x, 0] = Tile.Wall;
    map[x, map.Width - 1] = Tile.Wall;
}
```

And analogously for y from 0 to map.Width. But a few methods have been implemented in the map class to make such logic easier to implement. In this case, we can use the following:

```
public void TransformBoundary(Func<float, float, Tile> transformation);
```

There are also Transform (for all tiles) and TransformInterior (for just non-boundary tiles). Thus it suffices to write this:

```
map.TransformBoundary((x, y) => Tile.Wall);
```

We can use Transform to clean up our existing logic as well. 

```
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MapGenPerlinNoise : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "Perlin Noise";

        // Insert properties to be set in the inspector here
        
        public override Map Generate()
        {
            Map map = new Map(50, 50);
            float scale = 0.1f;
            map.Transform((x, y) =>
            {
                float noiseValue = Mathf.PerlinNoise(scale * x, scale * y);
                return noiseValue < 0.5f ? Tile.Wall : Tile.Floor;
            });
            map.TransformBoundary((x, y) => Tile.Wall);
            return map;
        }
    }
}
```

If you're not familiar with lambdas, LINQ or functional programming, this might seem completely alien, and you may wish to stick with the explicit looping logic. There's nothing you can do with these functional methods that cannot be done with loops and conditionals.

If we now generate a cave, we see that there is a thin boundary around the cave, ensuring players don't wander out into the void. 

Next, we're using quite a few constants that would make for sensible parameters for our new map generator: length, width, scale, and the threshold 0.5f. Let's expose them to the inspector. 

```
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MapGenPerlinNoise : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "Perlin Noise";

        // Insert properties to be set in the inspector here
        public int length = 50;
        public int width = 50;
        public float scale = 0.1f;
        public float density = 0.5f;
        
        public override Map Generate()
        {
            Map map = new Map(length, width);
            map.TransformInterior((x, y) =>
            {
                float noiseValue = Mathf.PerlinNoise(scale * x, scale * y);
                return noiseValue < density ? Tile.Wall : Tile.Floor;
            });
            map.TransformBoundary((x, y) => Tile.Wall);
            return map;
        }
    }
}
```

Now we can set the size, density and scale of each instance of the Perlin Noise map generator in the inspector. 

An important issue that needs to be addressed is connectivity. Perlin Noise does not produce connected regions: it produces 'islands' instead. It's certainly possible to author your own logic to connect regions, but it's not trivial. Instead, you can use the MapBuilder static class to access a number of helper methods. This includes MapBuilder.ConnectFloors, which takes a Map and an int specifying the radius of any tunnels that need to be carved to ensure connectivity between all floors. It uses an algorithm that minimizes the amount of tunneling that needs to be done to ensure connectivity between all regions.

In addition to this, we'll replace the TransformBoundary method with another helper, ApplyBorder. The reason is that transforming the boundary could block a tunnel created by ConnectFloors: by applying a boundary we're adding walls around the existing map, instead of replacing floors. We'll include the boundary width as a configurable property.

The result:

```
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MapGenPerlinNoise : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "Perlin Noise";
        
        const int tunnelWidth = 2;

        // Insert properties to be set in the inspector here
        public int length = 50;
        public int width = 50;
        public float scale = 0.1f;
        public float density = 0.5f;
        public int borderWidth = 1;
        
        public override Map Generate()
        {
            Map map = new Map(length, width);
            map.Transform((x, y) =>
            {
                float noiseValue = Mathf.PerlinNoise(scale * x, scale * y);
                return noiseValue < density ? Tile.Wall : Tile.Floor;
            });
            MapBuilder.ConnectFloors(map, tunnelWidth);
            return MapBuilder.ApplyBorder(map, borderWidth);
        }
    }
}
```

Finally, let's add some randomization. PerlinNoise is deterministic: a given pair (x, y) will always produce the exact same return value. To add randomness, we can generate a pair of offset values so we take (a + x, b + y) for a fixed pair (a, b). We can control this randomization using a seed value. 

```
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MapGenPerlinNoise : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "Perlin Noise";
        
        const int tunnelWidth = 2;
        const float offsetRange = 1000;

        // Insert properties to be set in the inspector here
        public int length = 50;
        public int width = 50;
        public float scale = 0.1f;
        public float density = 0.5f;
        public int borderWidth = 1;
        public int seed = 0;
        
        public override Map Generate()
        {
            Random.InitState(seed);
            float xOffset = Random.Range(-offsetRange, offsetRange);
            float yOffset = Random.Range(-offsetRange, offsetRange);
            
            Map map = new Map(length, width);
            map.Transform((x, y) =>
            {
                float noiseValue = Mathf.PerlinNoise(xOffset + scale * x, yOffset + scale * y);
                return noiseValue < density ? Tile.Wall : Tile.Floor;
            });
            MapBuilder.ConnectFloors(map, tunnelWidth);
            return MapBuilder.ApplyBorder(map, borderWidth);
        }
    }
}
```

Now we get a random cave depending on the choice of seed value. See the section on IRandomizable on how to allow the Cave Generator to hook into a module's randomization. 

### Using IRandomizable

The inspector for CaveGeneratorUI reveals a flag for "Randomize Seeds", and the CaveGenerator itself likewise has a similar boolean argument. This allows the generator itself to automatically choose a random seed if you don't want to set the seeds on each randomizable component yourself. 

In order for the generator to do so, however, it's necessary to implement a very simple interface:

```
interface IRandomizable
{
    int Seed { set; }
}
```

To implement this interface for the map generator created in the previous section, only two additions need to be made: first, the class must extend IRandomizable. Second, it must implement a setter for Seed. 

```
public sealed class MapGenPerlinNoise : MapGenModule, IRandomizable
{
    //...
    
    int Seed { set { seed = value; } }
}
```

Here's the complete example for reference:

```
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MapGenPerlinNoise : MapGenModule, IRandomizable
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "Perlin Noise";
        
        const int tunnelWidth = 2;
        const float offsetRange = 1000;

        // Insert properties to be set in the inspector here
        public int length = 50;
        public int width = 50;
        public float scale = 0.1f;
        public float density = 0.5f;
        public int borderWidth = 1;
        public int seed = 0;
        
        public int Seed { set { seed = value; } }
        
        public override Map Generate()
        {
            Random.InitState(seed);
            float xOffset = Random.Range(-offsetRange, offsetRange);
            float yOffset = Random.Range(-offsetRange, offsetRange);
            
            Map map = new Map(length, width);
            map.Transform((x, y) =>
            {
                float noiseValue = Mathf.PerlinNoise(xOffset + scale * x, yOffset + scale * y);
                return noiseValue < density ? Tile.Wall : Tile.Floor;
            });
            MapBuilder.ConnectFloors(map, tunnelWidth);
            return MapBuilder.ApplyBorder(map, borderWidth);
        }
    }
}
```

Now the Cave Generator can automatically randomize the seed for this module without you being forced to manually change it each time.