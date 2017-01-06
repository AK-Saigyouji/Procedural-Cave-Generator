# Procedural Cave Generator

## 0. Introduction

This is a set of scripts that allow the creation of randomized 3D cave terrain in Unity with collision detection and full texture support. Once configured in the editor, caves can be generated with the click of a button. Features that can be configured include length and width, density, height, boundary size, scale, and heightmaps.

![3D cave with height map](http://i.imgur.com/sBi6T2U.jpg)

![Enclosed cave](http://i.imgur.com/GS2n1Nu.jpg)

Note: The textures themselves are from an excellent free asset pack called [Natural Tiling Textures](https://www.assetstore.unity3d.com/en/#!/content/35173) by Terramorph Workshop. 

## 1. Using the procedural cave generator

### In editor

Create a new empty game object, and attach the CaveGenerator script. Configure its properties in the inspector. Run the scene, and you will see two buttons appear in the inspector: Generate New Map and Create Prefab. Generating a new map will create a new cave, overwriting any previously generated cave. Creating a prefab will convert the current cave into a prefab and save it into your directory in a folder called "GeneratedCave" along with the meshes. This allows you to exit play mode, drag the cave into your scene, and work with it in the editor. Once a cave has been converted to a prefab, it retains no dependency on the CaveGenerator, nor on any script from this project: it's composed entirely of core Unity objects. 

### In code

Note: the API for working with the Cave Generator and its subsystems can and will change in future versions, so be cautious about swapping in a new version into your project. 

You can also create caves entirely through code by adding the CaveGenerator namespace. The "Configuration" property on the CaveGenerator holds all the properties that can be configured, and mostly match what you see in the inspector. The Generate method will then produce your cave just as the inspector button did. You can access the generated cave by using the ExtractCave method, ideally in a callback supplied to Generate (see the example below) since Generate is asynchronous. ExtractCave returns a Cave object, which contains the cave GameObject, a CollisionTester object which can be used to dynamically determine if an object will fit inside the cave without colliding with walls (useful for placing content at run-time), and a copy of the configuration used to generate that cave.

Here's a quick example of how to Generate a cave and consume the result properly using a callback. 

```
using CaveGeneration;

public class Example : MonoBehaviour
{
    // Either drag a CaveGenerator into this slot in the inspector, or assign one using AddComponent.
    public CaveGenerator caveGenerator; 
    
    void Start()
    {
        caveGenerator.Generate(Extract);
    }
    
    void Extract()
    {
        Cave cave = caveGenerator.ExtractCave();
        DoStuff(cave);
    }
    
    void DoStuff(Cave cave)
    {
        // Your logic can begin here.
    }
    
    // In particular, don't do the following:
    // void Start()
    // {
    //     caveGenerator.Generate();
    //     cave = caveGenerator.ExtractCave(); // InvalidOperationException, since cave is still being generated
    // }
}
```
  
## 2. Brief overview of how the generator works

This section contains some technical information for those curious how things work under the hood, or who want to modify the internals for their own purposes. There are two major subsystems in this project: MapGeneration and MeshGeneration. These have been written to be completely modular, and could be used or exported on their own. The HeightMap system is written to implement the IHeightMap interface in MeshGeneration. CaveGeneration is the system that puts it all together, and is driven by the class CaveGenerator.

### Map generation

The purpose of map generation is to produce a Map object, which is a grid of tiles (internally, a 2d byte array of 0s and 1s) corresponding to floors and walls. I've created an application that offers a visualization of the map generator and a fairly detailed overview of the underlying algorithms and their run times. It can be accessed [here](https://ak-saigyouji.github.io).

MapBuilder offers a library of methods for building a Map. See MapGenerator, the default generator, for an example of how it can be used. The methods in MapBuilder can be easily used in conjunction with your own methods to produce your own map generators.

### Mesh generation

The primary algorithm driving mesh generation is Marching Squares, which offers way to triangulate a 2d grid into a smoother mesh. This is used to generate flat ceiling and wall meshes. Walls are built by attaching the outlines of the walls and ceilings with a series of quads. 

### Height maps

The variations in height are by default produced using height maps based multiple layers of perlin noise. 

HeightMapFactory is a Factory class with a single method Build. This has several overloads corresponding to different types of HeightMaps, which all return an IHeightMap object. This includes an overload that takes a Func<float, float, float> object allowing you to create a heightmap based on an any function of your choice, as long as it takes two floats as parameters and returns a float. If creating custom height maps like this, they can be passed into the CaveGenerator's Generate method. 

## 3. Improvements to original project

This project started with Sebastian Lague's tutorial on Procedural Cave Generation found both on the Unity site and on his Youtube channel. It has been completely rewritten and greatly expanded. For those familiar with that project, here are some of the changes:

* Completely new cave type (enclosed) offers a fully enclosed cave terrain type that is difficult to produce using the terrain tools built into Unity. 
* Entire algorithm is asynchronous, so the editor will be more responsive while generating a larger map. 
* Wrote a system to facilitate the placement of content at run-time for arbitrary caves.
* Height maps offer more convincing terrain without increasing vertex or triangle counts. 
* It is now possible to convert maps into prefabs with the press of a button, allowing content to be placed in the editor. This eliminates the need to generate all game content dynamically, a difficult task for more complicated games. This also makes it easier to use Unity's lighting and navigation features.
* Substantially increased modularity throughout the project making it easier to extract individual components or replace them with customizations.
* The original project broke for maps much larger than 200 by 200, due to Unity's built in limitations on the number of vertices permitted in a single mesh. To fix this, the meshes are now generated in chunks, allowing for the creation of arbitrarily large maps. Additionally, the chunks can be dynamically toggled on/off during runtime so that only nearby chunks are rendered, significantly improving performance.
* Assigned uvs to the wall mesh, allowing textures to be applied. 
* Assigned tangents to all meshes, supporting advanced shader features.
* Eliminated seams in the wall by sharing vertices rather than doubling up. This dramatically improves appearance of textures and lighting on walls.
* Dramatically reduced asymptotic run time. The entire generator has a runtime that is now approximately linear (it is O(n * a(n)), where a(n) is the inverse ackermann function) in the number of tiles, i.e. length * width, and impractical maps as large as 1000 by 1000 can be generated in seconds. 
