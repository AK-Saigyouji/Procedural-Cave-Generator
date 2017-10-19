# Modules

Modules are customizable Scriptable Objects that define core parts of the cave generators. There are three types of modules: map generators, height maps, and outlines. They can be used by plugging them into the appropriate slots into the cave generator's inspector. This readme covers the process of defining new types of modules. 

The map generator module defines the locations of walls and walkable areas. The provided module uses cellular automata, but any algorithm used to generate 2D dungeons/levels could be used to define a module. 

Height maps define the height of each vertex in the floor and ceiling of the cave. The provided modules are fairly conventional perlin-noise based height maps typically used for terrain. 

Outlines are used by the outline cave generator to generate a boundary out of prefabs along the outline of a floor.

## Table of Contents
1. [Map modules](#map-modules)
2. [Heightmap modules](#heightmap-modules)
3. [Outline modules](#outline-modules)
4. [CaveWall modules]($cave-wall-modules)
5. [Compound modules](#compound-modules)

### <a name="map-modules"></a>1. Map modules

Defining a custom map module gives you complete control over the structure of the cave, allowing you to plug seamlessly into the engine for mesh generation. First we'll go over the bare minimum to implement them, then build up the default map module to illustrate a variety of ideas and tools. Here's a basic template for a custom map module:

```cs
using System;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MyCustomMapGen : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "My Custom Generator";

        // Insert properties to be set in the inspector here

        public override Map Generate()
        {
            throw new NotImplementedException();
        }

        public override Coord GetMapSize()
        {
            throw new NotImplementedException();
        }
    }
}
```

Note four things: first, we're extending MapGenModule, which is what allows this module to be plugged into the generator. Second, we're overriding the Generate method, which returns a Map object. This is the Map that will be used to produce the cave. Third, we're overriding the GetMapSize method, which should return the length and width of the maps produced by this module. If the length and width cannot be determined without computing the map, they should offer an upper bound instead. Fourth, the class has a CreateAssetMenu attribute. This will allow us to create instances of this new module in the editor by using the Asset->Create menu.

You're free to choose your own class name instead of MyCustomMapGen (make sure the class name matches the name of that class file), and also to choose a new menuName. 

Next we'll start with a very simple implementation.

#### 1.1 A trivial example

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

public override Coord GetMapSize()
{
    return new Coord(10, 10);
}
```

If you're unfamiliar with scriptable objects, note that they're used differently from MonoBehaviours. The script we wrote is a template, which can produce instances, which act as permanent assets. To create an instance, we can return to the editor and go to Assets -> Create -> Cave Generation -> Map Generators -> My Custom Generator. We can name the instance anything we like. Note that the instance should have the Unity logo instead of the C# icon as its image. 

Now that we have an instance of a map generator module, we can plug it into a CaveGeneratorUI in the appropriate slot to be consumed. If we then generate using the sample modules provided for floor and ceiling height maps, we get something like this (textures are from Natural Tiling Textures, see footnote on images in the main readme):

![Diagonal Cave](http://imgur.com/eeANGGC.jpeg)

This covers the basics of creating a new module. Next we'll walk through an example that builds up the default map generator, to illustrate additional useful tools, and also to better understand the default generator and its customizable properties. We'll cover methods on the Map class, as well as the MapBuilder class (found in the AKSaigyouji.MapGeneration namespace) which provides some more advanced functionality. Note that this example may become out of date if features are added to the default generator, but you can always inspect the source code for this generator directly to see the most up to date version.

#### 1.2 Building the default generator

The default generator, in a nutshell, does the following: set tiles randomly, use a smoothing function to cluster walls/floors together, then connect all the floors.

##### 1.2.1 A completely random generator
So to start, let's create a random map generator. We can use the following MapBuilder method:

```cs
public static Map InitializeRandomMap(int length, int width, float mapDensity, int seed);
```

As we go along, we'll also expose the properties we use in the inspector so they can be customized. Here are the updated parts (also add "using AKSaigyouji.MapGeneration;" alongside the other using statements):

```cs
public int length = 50;
public int width = 50;
public float mapDensity = 0.5f;
public int seed = 0;

public override Map Generate()
{
    Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
    return map;
}

public override Coord GetMapSize()
{
    return new Coord(length, width);
}
```

This produces the following cave:

![Random mess of a cave](http://imgur.com/iIMW0hO.jpg)

##### 1.2.2 Get structure out of noise

It's certainly random, but not very structured. We can apply a smoothing function to make the regions more regular. Let's use the following method in the MapBuilder class:

```cs
public static void Smooth(Map inputMap, int iterations = 5);
```

This method is based on cellular automata, a well known technique in procedural generation for producing cavernous terrain. It has the benefit of looking more natural than many other techniques, with the downside that the final result is difficult to control.

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

##### 1.2.3 Connect the floors and seal the boundary

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

We'll add an inspector variable for the border size, and also update GetMapSize to include the extra length and width introduced by the border.

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

public override Coord GetMapSize()
{
    int borderContribution = 2 * borderSize;
    return new Coord(length + borderContribution, width + borderContribution);
}
```

The result:

![Cavernous cave with thin, rectangular boundary](http://imgur.com/mfdhLcs.jpg)

##### 1.2.4 Build a better boundary

Much better - we can't walk off into the void, and we can get to any open area from any other open area. There is a visual issue, however - the regions alongside the boundary look rather boxy. This is because we're adding a thin rectangular strip alongside the border, resulting in a long, smooth border. This will look especially jarring in the enclosed variation of the generator. 

Fortunately there's a simple fix: right at the start, we can initialize the boundary tiles to be all wall tiles. That way, when we perform smoothing, we'll get a more natural cavern-like appearance.

We could write a few loops to manually set map[x, y] = Tile.Wall at the appropriate places. But Map has a few extension methods that make such transformations simpler: they are Transform, TransformBoundary, and TransformInterior, and they take a Func<int, int, Tile>: i.e. any function that takes two integers and outputs a Tile. They can also take lambdas, which is what we'll use:

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

##### 1.2.5 Prune small regions

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

##### 1.2.6 Ensuring minimum width

We know that the map will contain a tile path from any two tiles. But how large of an object will this accommodate? As it turns out, in the worst, case, a tunnel can be exactly 1 game unit wide, and an object of exactly that size may have some trouble fitting through such an opening. It would be good if we could expand small tunnels to ensure that the map can be navigated without having to shrink the navigators. We could iterate over all floor tiles and expand the floors in every direction by a given radius. While this works, it would dramatically reduce the proportion of walls in the map. I've implemented a far less aggressive expansion algorithm which identifies likely passages and expands only those. The end result is that the map's passages have a minimum  width of 2 squares. 


```cs
public static void WidenTunnels(Map inputMap);
```

```cs
public bool expandTunnels = true;

public override Map Generate()
{
    Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
    map.TransformBoundary((x, y) => Tile.Wall);
    MapBuilder.RemoveSmallFloorRegions(map, minFloorSize);
    MapBuilder.Smooth(map);
    MapBuilder.ConnectFloors(map);
    if (expandTunnels)
    {
        MapBuilder.WidenTunnels(map);
    }
    MapBuilder.RemoveSmallWallRegions(map, minWallSize);
    map = MapBuilder.ApplyBorder(map, borderSize);
    return map;
}
```

##### 1.2.7 Permitting automatic randomization

One final important change is to allow the cave generator to hook into this module's seed to automatically randomize it. This is done by overriding the base class Seed property.

```cs
public override int Seed { get { return seed; } set { seed = value; } }
```

##### 1.2.8 Final result:

The final, complete script for reference:

```cs
using UnityEngine;
using AKSaigyouji.Maps;
using AKSaigyouji.MapGeneration;

namespace AKSaigyouji.Modules.MapGeneration
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName)]
    public class MyCustomMapGen : MapGenModule
    {
        // This string determines the name of this type of map generator in the menu.
        const string menuName = "My Custom Generator";

        // Insert properties to be set in the inspector here
        public int length = 50;
        public int width = 50;
        public float mapDensity = 0.5f;
        public int seed = 0;
        public int borderSize = 1;
        public bool expandTunnels = true;
        public int minWallSize = 50;
        public int minFloorSize = 50;

        public override int Seed { get { return seed; } set { seed = value; } }
        
        public override Map Generate()
        {
            Map map = MapBuilder.InitializeRandomMap(length, width, mapDensity, seed);
            map.TransformBoundary((x, y) => Tile.Wall);
            MapBuilder.RemoveSmallFloorRegions(map, minFloorSize);
            MapBuilder.Smooth(map);
            MapBuilder.ConnectFloors(map);
            if (expandTunnels)
            {
                MapBuilder.WidenTunnels(map);
            }
            MapBuilder.RemoveSmallWallRegions(map, minWallSize);
            map = MapBuilder.ApplyBorder(map, borderSize);
            return map;
        }

        public override Coord GetMapSize()
        {
            int borderContribution = 2 * borderSize;
            return new Coord(length + borderContribution, width + borderContribution);
        }
    }
}
```

#### 1.3 Build your own map generator

##### 1.3.1 Limitations of the default generator

Depending on what type of game you're trying to make, the default generator may not suit your purposes. As an example, if you're trying to build a rogue-like or ARPG (action role playing game) and want to use procedural generation to produce a new cave automatically on each play-through, then you're going to have to come up with algorithms to add all content at run-time. But the output of the default generator gives you very little structure to work with. Maybe it will produce a single large room, or maybe a dozen small ones with tunnels connecting them. Maybe the rooms will be thin and tunnel-like, maybe they'll be round and wide. Maybe there will be many paths to any exit you place, maybe there will be just one. All these possibilities make controlling or even constraining the user experience very difficult. 

This is why cellular automata is not typically used for games whose content is generated at run-time, at least not by itself - it's too difficult to control. See the section on compound modules for further information.

### <a name="heightmap-modules"></a>2. Height map modules

Defining a height map module is similar to defining a map generator module. We'll give a quick example of a height map based on Perlin Noise to illustrate the key ideas and tools. 

#### 2.1 A simple example: Perlin Noise height map

##### 2.1.1 Template for height map modules

Here's a template for a general height map module:

```cs
using UnityEngine;
using AKSaigyouji.HeightMaps;

namespace AKSaigyouji.Modules.HeightMaps
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

##### 2.1.2 Producing IHeightMaps with HeightMapFactory

Unlike map generator modules which requires a Map object, height map modules require an IHeightMap object. While it's possible to define a class to implement this interface directly, it's easier to make use of the HeightMapFactory class. This has a method Build, overloaded to take several different sets of parameters depending on what kind of height map you want. Here are three examples:

```cs
public static IHeightMap Build(float height); // constant 
public static IHeightMap Build(float minHeight, float MaxHeight, float scale, int seed); // perlin noise
public static IHeightMap Build(Func<float, float, float> heightFunction, float minHeight, float maxHeight); // custom
```

The third method listed here is extremely flexible, as it allows you to pass in any function that takes two floats (an x and a z) and outputs a float (the corresponding y value, i.e. height). Let's build a simple perlin noise height map generator with the second listed method. Perlin noise is a special type of random number generator which produces continuous random values based on two input floats. scale determines how compressed / spread out the height map will be, and minHeight/maxHeight define the boundary values. 

##### 2.1.3 Complete example

Here's the complete example:

```cs
using AKSaigyouji.HeightMaps;
using UnityEngine;

namespace AKSaigyouji.Modules.HeightMaps
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + menuName)]
    public sealed class HeightMapPerlinNoise : HeightMapModule
    {
        const string menuName = "Perlin Noise";
        
        public float scale = 10;
        public float minHeight = 3;
        public float maxHeight = 5;
        public int seed = 0;

        public override int Seed { get { return seed; } set { seed = value; } }
        
        public override IHeightMap GetHeightMap()
        {
            return HeightMapFactory.Build(minHeight, maxHeight, scale, seed);
        }
    } 
}
```

### <a name="outline-modules"></a>3. Rock outline modules

The rock outline cave generator lays down a floor, and then instantiates rocks along the outlines of the floor. The outline module (OutlineModule) takes an outline in the form of a sequence of Vector3s, and instantiates rocks along it: the default (OutlineEdgeAligned) allows an array of prefabs to be assigned. It instantiates rocks randomly on the midpoint of each edge in the outline, rotating them so that the long side runs along the edge. For this to work correctly, the prefabs need to be rotated so that their long side runs down the z-axis, if applicable (i.e. if it has a longer side).

To create a custom outline module, you'll need to extend the OutlineModule class, and override one method:

```cs
public abstract void ProcessOutlines(IEnumerable<Outline> outlines, Transform parent);
```

Outline implements ```IList<Vector3>```, and you can get the corners of the outlines by indexing, or like so:

```cs
foreach (Outline outline in outlines){
    foreach (Vector3 corner in outline){
        // ...
    }
}
```

Alternatively, you can iterate over the edges of the outlines:

```cs
foreach (Outline outline in outlines){
    foreach (Edge edge in outline.GetEdges()){
        // ...
    }
}
```

Edge is a simple struct with the following readonly properties:
* Vector3 StartPoint;
* Vector3 EndPoint;
* Vector3 MidPoint;
* Vector3 Direction; (from start to end)
* float Length; (only takes on two values: 1 or about 0.7071)

The intended idea behind an OutlineModule is that rocks (or other objects) should somehow be placed (instantiated) along the provided outlines. The provided default module (OutlineEdgeAligned) takes a list of rock prefabs, and randomly instantiates a rock on the midpoint of each outline edge, rotating the rock so that it's z-axis runs along the direction of the edge.

Some other ideas would be to instantiate objects along the corners of the outlines rather than the edges, or perhaps both: we could instantiate walls at the midpoints of edges, and some kind of post/connector on the corners. Note that since the Length of each edge can only take one of two values as indicated above, this can be used to carefully fit walls of an exact length together. 

Here's the essence of what the implementation of OutlineEdgeAligned looks like (I've omitted validation and accessors for simplicity):

```cs
[SerializeField] WeightedPrefab[] rockPrefabs;
[SerializeField] int seed;

public override int Seed { get { return seed; } set { seed = value; } }

public override void ProcessOutlines(IEnumerable<Outline> outlines, Transform parent)
{
    var prefabPicker = new WeightedPrefabPicker(rockPrefabs, seed);
    var prefabber = new EdgePrefabber(prefabPicker);
    foreach (Outline outline in outlines)
    {
        prefabber.ProcessOutline(outline, parent);
    }
}
```

WeightedPrefabPicker performs a weighted random choice among a list of prefabs. EdgePrefabber instantiates rocks along the midpoints of the edges. ProcessOutline is implemented inside EdgePrefabber as follows:

```cs
public void ProcessOutline(Outline outline, Transform parent)
{
    foreach (Edge edge in outline.GetEdges())
    {
        GameObject rockPrefab = prefabPicker.PickPrefab();
        Vector3 position = edge.MidPoint;
        Vector3 direction = edge.Direction;
        Quaternion prefabRotation = rockPrefab.transform.rotation;
        GameObject rockInstance = GameObject.Instantiate(rockPrefab, position, prefabRotation, parent);
        if (!IsParallelToTarget(direction))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            rockInstance.transform.rotation = rotation * prefabRotation;
        }
        rockInstance.name = string.Format("{0} ({1})", rockPrefab.name, rockCounter);
        rockCounter++; // used only for the name
    }
}

static bool IsParallelToTarget(Vector3 direction)
{
    return direction == Vector3.forward || direction == Vector3.back;
}
```

The only non-trivial part is ensuring the prefab is rotated correctly.

### <a name="cave-wall-modules"></a>4. CaveWall Modules

This module is designed to allow customization of the geometry of the walls in the cave. By default, the walls are entirely flat. This generally works fine for short walls, but is unappealing for higher walls. 

Normally, walls are created as follows: a 2D outline is created in the xz plane. Along each edge, a quad is placed: these quads form the walls of the caves. Another way to think of this is that for each (x, 0, z) point in the outline, a point (x, y, z) is added above it, and used to form quads that run along the outline. What this module allows is for additional points (x, y_i, z) to be added such that 0 < y_i < y. Then, these additional points can be moved around to give the walls a more interesting shape. A simple approach to create more interesting walls is to add a little bit noise to each extra vertex: (x, y_i, z) + (e_x, e_y, e_z), where e is a small, random vector. 

To define your own cave wall module, here's a template:

```cs
using System;
using UnityEngine;

namespace AKSaigyouji.Modules.CaveWalls
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + menuName]
    sealed class MyCaveWallModule : CaveWallModule
    {
        const string menuName = "My Cave Wall Module";
        
        public override int ExtraVerticesPerCorner { get { return 0; } }

        public override Vector3 GetAdjustedCorner(VertexContext context)
        {
            return context.Vertex;
        }
    }
}
```

First, determine how many vertices should be added to the wall at each corner. This will increase the size of the mesh, so use the smallest number of vertices that will achieve the effect you want. 

Next, implement GetAdjustedCorner. This will receive every vertex in the new wall one by one, along with some extra information, packaged together into a VertexContext object. The value you return will be used to replace the original vertex. 

VertexContext has the following properties:
* Vector3 Vertex;
* Vector3 Normal;
* bool IsFloor;
* bool IsCeiling;
* int InterpolationIndex;

And the following methods:
* float GetFloorHeightAt(float x, float z);
* float GetCeilingHeightAt(float x, float z);

For most purposes, you only need to use Vertex, Normal, IsFloor and IsCeiling. 

Vertex is the original vertex from the wall.

Normal is a unit vector that points out of the wall: when adjusting the position of the vertex, it's a good idea to adjust in the direction of normal: vertex + coefficient * normal. 

IsFloor and IsCeiling indicate whether the current vertex is on the floor or ceiling respectively. Messing with these vectors runs the risk of creating a visible gap between the floor/wall or wall/ceiling, so you may wish to skip the floor and ceiling by returning the unaltered vertex.

InterpolationIndex indicates how many vertices down the wall that particular vertex is. e.g. if there are four vertices per corner, then the ceiling vertex is 0, the next one down is 1, then 2, and the floor is 3. This isn't needed for most purposes, but can be useful in some cases.

GetFloorHeightAt and GetCeilingHeightAt give the height of the floor and ceiling at a particular point (x, z). You probably won't need to use them unless you move the floor/ceiling vertices. If you move the ceiling vertex, then updating the height to match the ceiling height will ensure the wall lines up with the ceiling.

To give an example of an implementation, here's the overriden method in the CaveWallPerlin module:

```cs
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
```

The implementation of ComputeAdjustment is omitted as the details are not the point of the example. This example leaves the floor and ceiling vertices unaltered, and otherwise returns the original vertex perturbed along its normal by a magnitude of 'adjustment', which is a small, random float. 

### <a name="compound-modules"></a>5. Compound Modules

Compound modules are modules that make use of other modules (usually as exposed fields). This simple but powerful idea opens up a lot of design patterns, such as decoration. An example of decoration is the MapGenEntranceCarver module. It takes an arbitrary MapGenModule as an exposed field, carves out entrances at the points specified in the inspector, and then connects them to the internals of the module. This pattern can be used to add additional customizable properties to a variety of modules without having to modify each one. 

Another use for compound modules is to create a module that specifies the large scale structure of an environment/dungeon/cave/etc. but delegates the local details to other modules. As an example, we could write a simple maze-building module to generate mazes like this:

```
0-0
|
0-0
  |
0-0
```

Each 0 is a room, and each dash is a connection: one or more submodules are then used to fill in each room. 

This approach is common among successful ARPGs such as Diablo 2 and Path of Exile, and is something I'm exploring in greater generality in my Atlas Generation project (Atlas-Chart PCG): an atlas is the entire large-scale map, composed of charts (the individual rooms) mapped out locally by submodules. This atlas-chart framework generalizes the maze-room design seen in the above mentioned games. 
