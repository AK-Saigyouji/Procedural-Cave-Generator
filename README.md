# Procedural Cave Generator

## 0. Introduction

This is a set of scripts that allow the creation of randomized 3D cave terrain in Unity with collision detection and full texture support. Once configured in the editor, caves can be generated with the click of a button. Features that can be configured include length and width, density, height, boundary size, scale, and heightmaps.

![3D cave with height map](http://i.imgur.com/sBi6T2U.jpg)

![Enclosed cave](http://i.imgur.com/GS2n1Nu.jpg)

Note: The textures themselves are from an excellent free asset pack called [Natural Tiling Textures](https://www.assetstore.unity3d.com/en/#!/content/35173) by Terramorph Workshop. 

## 1. Using the procedural cave generator

### In editor

Create a new empty game object, and attach one of the cave generator scripts: CaveGeneratorIsometric, or CaveGeneratorEnclosed. Configure the parameters as desired in the inspector (tooltips have been implemented to give additional information on what each parameter does). Run the scene, and you will see two buttons appear in the inspector: Generate New Map and Create Prefab. Generating a new map will create a new cave, and attach it as a child to the empty game object, overwriting any previously generated cave. Creating a prefab will convert the current cave into a prefab and save it into your directory in a folder called "GeneratedCave" along with the meshes. This allows you to exit play mode, drag the cave into your scene, and work with it in the editor. 

### In code

Note: the API for working with the Cave Generator and its subsystems can and probably will change in future versions, so be cautious about swapping in a new version into your project. 

You can also create caves entirely through code. Add the CaveGenerator namespace to your script, and use AddComponent to attach the cave generator to an object. Assign its parameters through the exposed properties. The Generate method will then produce your cave just as the inspector button did. You can access the generated cave as a gameobject by using the ExtractCave method. Additionally, the Grid property gives you access to an object that can be indexed like a 2d array, and tells you whether a coordinate holds a wall or floor. This can be used to dynamically place content based on the resulting cave. Producing content this way is challenging, however, as everything has to be generated and placed programmatically. In the future I intend to offer substantially more support for pure dynamic generation of content for unlimited replayability common in roguelikes.

### Using height maps

The other two attachable scripts provided are for varying the height in the terrain. HeightMapFloor will offer variation in the height of the floor, and HeightMapMain depends on what type of cave you're generating, but will affect the raised part of the terrain (the ceiling mesh for CaveGeneratorIsometric, and the enclosure mesh for CaveGeneratorEnclosed). Given that the parameters are a bit difficult to understand, height maps have a "Visualize" feature that allows you to see what the resulting floor or ceiling would look like in the scene. When enabled, it will draw a simple mesh that automatically updates in response to changes in the parameters. Play around with it until you get the height map you want. 
  
## 2. Brief overview of how the generator works

This section contains some technical information for those curious how things work under the hood, or who want to modify the internals for their own purposes. There are two major subsystems in this project: MapGeneration and MeshGeneration. These have been written to be completely modular, and could be used or exported on their own. The HeightMap system is written to implement an interface for MeshGeneration. CaveGeneration is the system that puts it all together, and is driven by the class CaveGenerator.

### Map generation

The purpose of map generation is to produce a Map object, which is a grid of tiles (internally, a 2d byte array of 0s and 1s) corresponding to floors and walls. I've created an application that offers a visualization of the map generator and a fairly detailed overview of the underlying algorithms and their run times. It can be accessed [here](https://ak-saigyouji.github.io).

MapBuilder offers a library of methods for building a Map. See MapGenerator, the default generator, for an example of how it can be used. By implementing the IMapGenerator interface, you can build your own generator fairly easily by using the methods provided by MapBuilder and then swap out the default one in CaveGenerator.

### Mesh generation

The primary algorithm driving mesh generation is Marching Squares, which offers way to triangulate a boxy grid into a smoother mesh. For CaveGeneratorIsometric, this algorithm is used to produce the ceiling mesh by triangulating the walls in the grid, and the floor mesh by triangulating the floors in the grid. The ceiling is raised, and a series of quads are built along the outlines, connecting the floor and ceiling. For CaveGeneratorEnclosed, we instead use two copies of the floor mesh to create completely closed off geometry. 

### Height maps

The variations in height are produced using a height map generator that is based on perlin noise. Unlike a random number generator, perlin noise produces continuous values more suitable for specifying terrain height. Multiple levels of perlin noise are layered on top of each other to give fine control over the final height map. 

HeightMapBuilder is a MonoBehaviour that can be customized in the inspector, and then outputs a suitable object implementing the IHeightMap interface required by the MeshGenerator. 

## 3. Improvements to original project

This project started with Sebastian Lague's tutorial on Procedural Cave Generation found both on the Unity site and on his Youtube channel. It has been completely rewritten and greatly expanded. For those familiar with that project, here are some of the changes:

* Completely new cave type (enclosed) offers a fully enclosed cave terrain type that is difficult to produce using the terrain tools built into Unity. 
* Entire algorithm is asynchronous, so the editor won't freeze while generating a larger map. 
* Height maps offer more convincing terrain without increasing vertex or triangle counts. 
* It is now possible to convert maps into prefabs with the press of a button, allowing content to be placed in the editor. This eliminates the need to generate all game content dynamically, a difficult task for more complicated games. This also makes it easier to use Unity's lighting and navigation features.
* The original project broke for maps much larger than 200 by 200, due to Unity's built in limitations on the number of vertices permitted in a single mesh. To fix this, the meshes are now generated in chunks, allowing for the creation of arbitrarily large maps. Additionally, the chunks can be dynamically toggled on/off during runtime so that only nearby chunks are rendered, significantly improving performance.
* Assigned uvs to the wall mesh, allowing textures to be applied. 
* Assigned tangents to all meshes, supporting advanced shader features.
* Eliminated seams in the wall by sharing vertices rather than doubling up. This dramatically improves appearance of textures and lighting on walls.
* Created a custom inspector permitting map creation with the press of a button in the inspector. 
* Fixed a bug with the smoothing function in the map generator. In the original project, smoothing was done in place. This meant once a cell is changed, this change affected its neighbors when it's their turn. By copying changes into a new map, this issue is avoided.
* Dramatically reduced asymptotic run time. The entire generator has a runtime that is now approximately linear (it is O(n * a(n)), where a(n) is the inverse ackermann function) in the number of tiles, i.e. length * width, and enormous maps as large as 1000 by 1000 can be generated in seconds. 
