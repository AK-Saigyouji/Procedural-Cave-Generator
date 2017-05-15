# Modules

This folder contains modules: customizable Scriptable Objects that define core parts of the cave generator. Currently there are two types of modules: map generators and height maps.

## Usage

### Introduction

Either use a sample module provided, or create one using the menu. Samples can be found in the relevant folders in this directory. To create one using the menu, select Assets -> Create -> Cave Generation and select the relevant module. Modules offer a number of customizable properties unique to their type. Alternatively, you can duplicate an existing module of the correct type directly. 

Each module can be plugged into the CaveGeneratorUI inspector in the appropriately marked spot, or passed as an argument to the CaveGenerator.Generate method if working through code. 

## Define your own modules

In addition to using and customizing the provided modules, you can write your own based on custom logic and plug them into the cave generator. The rest of this readme covers this process, and the tools provided to make it easier. 

### (1) Map modules

Defining a custom map module gives you considerable control over the structure of the cave, allowing you to plug seamlessly into the engine for mesh generation. First we'll go over the bare minimum to implement them, then build up the default map module to illustrate a variety of ideas and tools. Here's a basic template for a custom map module:

```cs
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MyCustomMapGen : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "My Custom Generator";

        // Insert properties to be set in the inspector here
        
        public override Map Generate()
        {
            throw new System.NotImplementedException();
        }
    }
}
```

Note three things: first, we're extending MapGenModule, which is what allows this module to be plugged into the generator. Second, we're overriding the Generate method, which returns a Map object. This is the Map that will be used to produce the cave. Third, the class has a CreateAssetMenu attribute. This will allow us to create instances of this new module in the editor by using the Asset->Create menu.

You're free to choose your own class name instead of MyCustomMapGen (make sure the class name matches the name of that class file), and also to choose a new menuName. 

Now, let's explore what we can do with this.

#### (1.1) A trivial example

The most direct way to create a custom map module is to define a map manually. The Map type behaves a lot like a 2D array, with each element being a Tile. Tile is an enum that can take the values of Wall or Floor. 

As an example, let's see what happens if we make a map with a simple diagonal path from the bottom left to the top right:

```cs
public override Map Generate()
{
    int length = 10;
    int width = 10;
    Map map = new Map(length, width);

    // first let's set all tiles to walls
    for (int y = 0; y < length; y++)
    {
        for (int x = 0; x < width; x++)
        {
            map[x, y] = Tile.Wall;
        }
    }

    // next let's carve a diagonal path from bot left to top right
    for (int i = 1; i < length; i++)
    {
        map[i, i] = Tile.Floor;
        map[i, i - 1] = Tile.Floor;
    }
    return map;
}
```

If you're unfamiliar with scriptable objects, note that they're used differently from MonoBehaviours. The script we wrote is a template, which can produce instances, which act as permanent assets. To create an instance, we can return to the editor and go to Assets -> Create -> Cave Generation -> Map Generators -> My Custom Generator. We can name the instance anything we like. Note that the instance should have the Unity logo instead of the C# icon as its image. 

Now that we have an instance of a map generator module, we can plug it into a CaveGeneratorUI in the appropriate slot to be consumed. If we then generate using the sample modules provided for floor and ceiling height maps, we get something like this (textures are from Natural Tiling Textures, see footnote on image in the main readme):

![Diagonal Cave](http://imgur.com/eeANGGC.jpeg)

This covers the basics of creating a new module. Next we'll walk through an example that builds up the default map generator, to illustrate additional useful tools, and also to better understand the default generator and its customizable properties. We'll cover methods on the Map class, as well as the MapBuilder class which provides some more advanced functionality. 

#### (1.2) Building the default generator

The default generator, in a nutshell, does the following: set tiles randomly, use a smoothing function to cluster walls/floors together, then connect all the floors.

##### (1.2.1) A completely random generator
So to start, let's create a random map generator. We can use the following MapBuilder method:

```cs
public static Map InitializeRandomMap(int length, int width, float mapDensity, int seed);
```

As we go along, we'll also expose the properties we use in the inspector so they can be customized. Here are the updated parts:

```cs
// Insert properties to be set in the inspector here
public int length = 50;
public int width = 50;
public float mapDensity = 0.5f;
public int seed = 0;

public override Map Generate()
{
    Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
    return map;
}
```

This produces the following cave:

![Random mess of a cave](http://imgur.com/iIMW0hO.jpg)

##### (1.2.2) Get structure out of noise

It's certainly random, but not very structured. We can apply a smoothing function to make the regions more regular. Let's use the following method in the MapBuilder class:

```cs
public static void Smooth(Map inputMap, int iterations = 5);
```

This method is based on cellular automata, a well known technique in procedural generation for producing cavernous terrain. It has the benefit of looking more a lot more natural than a lot of other techniques, with the downside that the final result is difficult to control.

```cs
public override Map Generate()
{
    Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
    MapBuilder.Smooth(map);
    return map;
}
```

The result:

![Smooth, cavernous cave](http://imgur.com/vMxjadk.jpg)

##### (1.2.3) Connect the floors and seal the boundary

Already we have a cavernous structure, albeit with several issues. One is that not all regions are connected. Another is that open spaces lead into the void - our caves should be enclosed to prevent stepping off the map. 

Efficient connectivity algorithms are not trivial to implement. An efficient implementation is provided in another MapBuilder method:

```cs
public static void ConnectFloors(Map inputMap, int tunnelRadius = 1);
```

This method will carve out paths in a minimalistic fashion to ensure that every tile is reachable from every other tile. tunnelRadius determines how wide these tunnels will be. This method has a few overloads, but we'll ignore those for now. 

To fix the other issue, we can apply a border to our map with the following method:

```cs
public static Map ApplyBorder(Map inputMap, int borderSize);
```

We'll add an inspector variable for the border size.

```cs
public int borderSize = 1;

public override Map Generate()
{
    Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
    MapBuilder.Smooth(map);
    MapBuilder.ConnectFloors(map);
    map = MapBuilder.ApplyBorder(map, borderSize);
    return map;
}
```

The result:

![Cavernous cave with thin, rectangular boundary](http://imgur.com/mfdhLcs.jpg)

##### (1.2.4) Build a better boundary

Much better - we can't walk off into the void, and we can get to any open area from any other open area. There is a visual issue, however - the regions alongside the boundary look rather boxy. This is because we're adding a thin rectangular strip alongside the border, resulting in a long, smooth border. This will look especially jarring in the enclosed variation of the generator. 

Fortunately there's a simple fix: right at the start, we can initialize the boundary tiles to be all wall tiles. That way, when we perform smoothing, we'll get a more natural cavern-like appearance.

We could write a few loops to manually set map[x, y] = Tile.Wall at the appropriate places. But Map has a few extension methods that make such transformations simpler and more compact: they are Transform, TransformBoundary, and TransformInterior, and they take a Func<int, int, Tile>: i.e. any function that takes two integers and outputs a Tile. They can also take lambdas, which is what we'll use:

```cs
public override Map Generate()
{
    Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
    map.TransformBoundary((x, y) => Tile.Wall);
    MapBuilder.Smooth(map);
    MapBuilder.ConnectFloors(map);
    map = MapBuilder.ApplyBorder(map, borderSize);
    return map;
}
```

Use of such methods can eliminate a lot of unnecessary looping boilerplate, and to avoid a lot of silly off-by-one errors. To see more examples, you can check the MapBuilders script, which uses them extensively. 

![Cavernous cave with better boundary](http://imgur.com/xoYUFeq.jpg)

As we can see, the inside of the boundaries now looks much more cavern-like, as desired. 

##### (1.2.5) Prune small regions

If we increase the size or scroll through some examples, we see that sometimes we get isolated wall regions of very small size. These look out of place, so we may wish to remove them. We will also do the same with small floor regions, which force the connectivity algorithm to dig tunnels to tiny rooms. As a side note, there's also a huge performance reason to prune very small floor regions, which becomes important when producing giant maps. 

Once again, we turn to MapBuilder:

```cs
public static void RemoveSmallFloorRegions(Map inputMap, int threshold);
public static void RemoveSmallWallRegions(Map inputMap, int threshold);
```

threshold is how many tiles in the Map object a region must occupy to survive the pruning. We'll remove floor regions before running the connect method, and then remove wall regions near the end after executing all the steps that may produce small wall regions.

```cs
public int minWallSize = 50;
public int minFloorSize = 50;

public override Map Generate()
{
    Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
    map.TransformBoundary((x, y) => Tile.Wall);
    MapBuilder.RemoveSmallFloorRegions(map, minFloorSize);
    MapBuilder.Smooth(map);
    MapBuilder.ConnectFloors(map);
    MapBuilder.RemoveSmallWallRegions(map, minWallSize);
    map = MapBuilder.ApplyBorder(map, borderSize);
    return map;
}
```

##### (1.2.6) Permitting automatic randomization with IRandomizable

One final important change is to allow the cave generator to hook into this module's seed to automatically randomize it. This is done by having the module implement the IRandomizable interface. This interface has a single setter for a Seed, and that's it. Have the class extend IRandomizable:

```cs
public class MyCustomMapGen : MapGenModule, IRandomizable
```

and include a setter for the seed within the class:

```cs
public int Seed { set { seed = value; } }
```

##### (1.2.7) Final result:

The final, complete script for reference:

```cs
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MyCustomMapGen : MapGenModule, IRandomizable
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "My Custom Generator";

        // Insert properties to be set in the inspector here
        public int length = 50;
        public int width = 50;
        public float mapDensity = 0.5f;
        public int seed = 0;
        public int borderSize = 1;
        public int minWallSize = 50;
        public int minFloorSize = 50;

        public int Seed { set { seed = value; } }

        public override Map Generate()
        {
            Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
            map.TransformBoundary((x, y) => Tile.Wall);
            MapBuilder.RemoveSmallFloorRegions(map, minFloorSize);
            MapBuilder.Smooth(map);
            MapBuilder.ConnectFloors(map);
            MapBuilder.RemoveSmallWallRegions(map, minWallSize);
            map = MapBuilder.ApplyBorder(map, borderSize);
            return map;
        }
    }
}
```

#### (1.3) Build your own map generator

##### (1.3.1) Limitations of the default generator

Depending on what type of game you're trying to make, the default generator may not suit your purposes. As an example, if you're trying to build a rogue-like or ARPG (action role playing game) and want to use procedural generation to produce a new cave each play-through, then you're going to have to come up with algorithms to add all content at run-time. But the output of the default generator gives you very little structure to work with. Maybe it will produce a single large room, or maybe a dozen small ones with tunnels connecting them. Maybe the rooms will be thin and tunnel-like, maybe they'll be round and wide. Maybe there will be many paths to any exit you place, maybe there will be just one. All these possibilities make controlling or even constraining the user experience very difficult. 

This is why cellular automata is not typically used for games whose content is generated at run-time - it's too difficult to control. In the future, I intend to flesh this section out, which will likely include one or more new modules more conducive to run-time randomization, and some suggestions for adding content at run-time.

### (2) Height map modules

Defining a height map module is similar to defining a map generator module. We'll give a quick example of a height map based on Perlin Noise to illustrate the key ideas and tools. 

#### (2.1) A simple example: Perlin Noise height map

##### (2.1.1) Template for height map modules

Here's the template:

```cs
using CaveGeneration.MeshGeneration;
using CaveGeneration.HeightMaps;
using UnityEngine;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + menuName)]
    public sealed class MyCustomHeightMap : HeightMapModule
    {
        // This string determines the name of this type of height map in the menu.
        const string menuName = "My Custom Height Map";

        // Insert properties to be set in the inspector here

        public override IHeightMap GetHeightMap()
        {
            throw new System.NotImplementedException();
        }
    } 
}
```

##### (2.1.2) Producing IHeightMaps with HeightMapFactory

Unlike map generator modules which requires a Map object, height map modules require an IHeightMap object. While it's possible to define a class to implement this interface directly, it's easier to make use of the HeightMapFactory class. This has a method Build, overloaded to take several different sets of parameters depending on what kind of height map you want. Here are three examples:

```cs
public static IHeightMap Build(float height); // constant 
public static IHeightMap Build(float minHeight, float MaxHeight, float scale, int seed); // perlin noise
public static IHeightMap Build(Func<float, float, float> heightFunction, float minHeight, float maxHeight); // custom
```

The third method listed here is extremely flexible, as it allows you to pass in any function that takes two floats (an x and a z) and outputs a float (the correspond y value, i.e. height). Let's build a simple perlin noise height map generator with the second listed method. Perlin noise is a type of random number generator that differs from UnityEngine.Random in two important ways: it varies continuously, making it useful for gradual changes, and it takes two parameters, not one, making it ideal for randomly picking a height based on the (x,z) coordinates. scale determines how compressed / spread out the height map will be, and minHeight/maxHeight define the boundary values. 

##### (2.1.3) Complete example

Here's the complete example:

```cs
using CaveGeneration.MeshGeneration;
using CaveGeneration.HeightMaps;
using UnityEngine;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + menuName)]
    public sealed class MyCustomHeightMap : HeightMapModule, IRandomizable
    {
        const string menuName = "My Custom Height Map";
        public float scale = 10;
        public float minHeight = 3;
        public float maxHeight = 5;
        public int seed = 0;

        public int Seed { set { seed = value; } }
        
        public override IHeightMap GetHeightMap()
        {
            return HeightMapFactory.Build(minHeight, maxHeight, scale, seed);
        }
    } 
}
```