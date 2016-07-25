# Procedural Cave Generator

## 0. Introduction

This is a set of scripts that allow the creation of both 2D and 3D cave terrain in Unity with collision detection and texture support. They can be created either on the fly during gameplay through code, or saved as prefabs and built upon in the editor without touching a line of code. This started with Sebastian Lague's tutorial on [procedural cave generation](https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9), but has grown substantially from there. 

![3D cave with height map](http://i.imgur.com/c2aGDLO.jpg)

![Enclosed cave](http://i.imgur.com/ktE29Pv.jpg)

Note: The textures themselves are from a free asset pack called [Natural Tiling Textures](https://www.assetstore.unity3d.com/en/#!/content/35173) by Terramorph Workshop. 

## 1. Using the procedural cave generator

### In editor

Create a new empty game object, and attach one of the cave generator scripts: CaveGenerator2D, CaveGenerator3D, or CaveGeneratorEnclosed. Configure the parameters as desired in the inspector (tooltips have been implemented to give additional information on what each parameter does). Then run the scene, and you will see two buttons appear in the inspector to generate a new map or to create a prefab. Generating a new map will create a new cave, and attach it as a child to the empty game object, overwriting any previously generated cave. Creating a prefab will turn the current cave into a prefab and save it into your directory in a folder called "GeneratedCave". This allows you to exit play mode, drag the cave into your scene, and work with it in the editor. 

### In code

You can also create caves entirely through code. Add the CaveGenerator namespace to your script, and use AddComponent to attach the cave generator to an object. Assign its parameters through the exposed properties. The GenerateCave method will then produce your cave just as the inspector button did. You can access the generated cave as a gameobject through the Cave property (note: be sure to change its parent if you plan to generate another cave and don't want the current one destroyed). Additionally, the Map property gives you access to a Map object that can be indexed like a grid, and tells you whether a coordinate holds a wall or not. This can be used to dynamically place content based on the resulting cave. Additional Map functionality will be implemented in the future to make this process easier. 

### Using height maps

The other two attachable scripts provided are for varying the height in the terrain. HeightMapFloor will offer variation in the height of the floor, and HeightMapMain depends on what type of cave you're generating, but will affect the raised part of the terrain (the ceiling mesh for CaveGenerator3D, and the enclosure mesh for CaveGeneratorEnclosed). The parameters are a bit complicated, but the idea is to stack a number of layers of noise to produce the height map. NumLayers determines how many layers are stacked. AmplitudeDecay determines how much the contribution of each layer drops, while FrequencyGrowth determines how compressed each layer will be (i.e. the rate of change of height in the layer). As an example, values of 3, 0.75, and 2 mean that there are 3 layers. Each layer contributes 50% of the previous layer, and is 2x compressed relative to the previous layer. Intuitively, the first layer determines the overall distribution of mountains, the second layer contributes peaks/valleys, and the third contributes jaggedness. 

Ultimately the best way to understand the parameters is to play with them and see the results.
  
## 2. Brief overview of how the generator works

The following is a quick look under the hood of how the generator works.

### Map generation

I've created an application that offers a visualization of the map generator and an overview of the underlying algorithms. It can be accessed [here](https://ak-saigyouji.github.io).

### Mesh generation

The primary algorithm driving mesh generation is Marching Squares, which offers way to triangulate a boxy grid into a smoother mesh. For CaveGenerator3D, this algorithm is used to produce the ceiling mesh by triangulating the walls in the grid, and the floor mesh by triangulating the floors in the grid. The ceiling is raised, and a series of quads are built along the outlines, connecting the floor and ceiling. For CaveGeneratorEnclosed, we instead use two copies of the floor mesh to create completely closed off geometry. 

The variations in height are produced using a height map generator that is based on perlin noise. Unlike a random number generator, perlin noise produces continuous values more suitable for specifying terrain height. Multiple levels of perlin noise are layered on top of each other to give fine control over the final height map. 

Marching squares is trivially parallelizable, so the majority of the mesh generation is multithreaded.

## 3. Improvements to original project

* Completely new cave type (enclosed) offers a fully enclosed cave terrain type that is normally very difficult to produce using the terrain tools built into Unity. 
* Height maps offer much more convincing terrain without increasing vertex or triangle counts. 
* It is now possible to convert maps into prefabs with the press of a button, allowing content to be placed in the editor. This eliminates the need to generate all game content dynamically, a difficult task for more complicated games. This also makes it easier to use Unity's lighting and navigation features. 
* The original project broke for maps much larger than 200 by 200, due to Unity's built in limitations on the number of vertices permitted in a single mesh. To fix this, the meshes are now generated in chunks, allowing for the creation of arbitrarily large maps. Additionally, the chunks can be dynamically toggled on/off during runtime to improve performance in large maps.
* Assigned uvs to the wall mesh, allowing textures to be applied. 
* Eliminated seams in the wall by sharing vertices rather than doubling up. This dramatically improves appearance of textures and lighting on walls.
* Created a custom inspector permitting map creation with the press of a button in the inspector. 
* Fixed a bug with the smoothing function. In the original project, smoothing was done in place. This meant once a cell is changed, this change affected its neighbors when it's their turn. By copying changes into a new map, this issue is avoided.
* Improved run time by over 99% and memory use by over 80%. The entire process has a runtime that is now approximately linear in the number of tiles in the map (i.e. linear in length * width), and enormous maps as large as 1000 by 1000 can be generated in seconds. 
* Large-scale reorganization of the code to be modular and extendable. 
