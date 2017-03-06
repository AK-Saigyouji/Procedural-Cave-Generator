# Procedural Cave Generator

## 0. Introduction

This is a highly customizable, scalable system for generating randomized 3D cave terrain in Unity. 

![3D cave with height map](http://i.imgur.com/sBi6T2U.jpg)

![Enclosed cave](http://i.imgur.com/GS2n1Nu.jpg)

Note: The textures themselves are from an excellent free asset pack called [Natural Tiling Textures](https://www.assetstore.unity3d.com/en/#!/content/35173) by Terramorph Workshop. 

## 1. Overview of contents

At the root level of the project are the attachable components in the project. This includes CaveGeneratorUI which is your interface to the generator in the editor.

The Modules folder contains the customizable components of the generator, in the form of Scriptable Objects. These can be independently customized and then plugged into the appropriate slots in the CaveGeneratorUI. Furthermore, you can write your own modules to replace the ones provided to customize the structure of the cave using your own algorithms. See the readme in the Modules folder for details on how to do that.

The Internals folder contains the source code for the bulk of the project. You do not need to use or alter anything in this folder to use the cave generator, nor to write your own modules.

## 2. Quickstart

### 2.1 In editor

Create a new empty game object, and attach the CaveGeneratorUI script. Set the properties in the inspector. You can find sample modules for the Map Generator, Floor Height Map and Ceiling Height Map slots in the Modules folder.

Run the scene, and you will see two buttons appear in the inspector: Generate New Cave and Create Prefab. Generating a new map will create a new cave, overwriting any previously generated cave. Creating a prefab will convert the current cave into a prefab and save it into your directory in a folder called "GeneratedCave" along with the meshes. This allows you to exit play mode, drag the cave into your scene, and work with it in the editor. Once a cave has been converted to a prefab, it retains no dependency on the CaveGenerator, nor on any script from this project: it's composed entirely of core Unity objects. 

The modules required by the generator are Scriptable Objects and can themselves be configured through their inspectors. You can also create additional ones by going through the menu: Assets -> Create -> Cave Generation and selecting the appropriate options. This allows you to define specific configurations of the various modules and save them as independent assets. 

### 2.2 In code

Note: the API for the cave generator has gone through a complete redesign several times already, and this may happen again in the future. As such, backward compatibility should not be expected in a given version.

You can also create caves entirely through code by adding the CaveGeneration namespace. The simple way is to configure a CaveGeneratorUI in the editor, then call its Generate method through code. 

To customize and build caves entirely through code, you can use the CaveGenerator (not CaveGeneratorUI) static class. It has a method Generate which takes a CaveConfiguration object. This object's properties match those seen in the inspector for CaveGeneratorUI and can be configured through its exposed properties. 

## 3. Writing your own modules

This system was built with modularity in mind. You can write your own map generator and height maps, which will plug easily into CaveGeneratorUI without having to touch any of the source code. See the readme in the Modules folder for details on how to do this. 

## 4. Brief overview of how the generator works

This section contains some technical information for those curious how things work under the hood, or who want to modify the internals for their own purposes beyond what's possible through the modules.

The two major subsystems are Map Generation and Mesh Generation. Map Generation is responsible for producing a randomized Map (2D array of 0s and 1s) which dictates where the walls should be. Mesh Generation then deterministically converts that into the 3D cave geometry. 

### 4.1 Map generation

The purpose of map generation is to produce a Map object, which is a grid of tiles (internally, a 2d byte array of 0s and 1s) corresponding to floors and walls. The overall structure is dictated by cellular automata, and connectivity is enforced using a standard minimal spanning tree algorithm (Kruskal's). 

### 4.2 Mesh generation

The primary algorithm driving mesh generation is Marching Squares, which offers way to triangulate a 2d grid into a smoother mesh. This is used to generate flat ceiling and floor meshes. Walls are built by attaching the outlines of the walls and ceilings with a series of quads. 

Outline generation, map triangulation, and even collision detection all use algorithms based on marching squares. 

### 4.3 Height maps

The variations in height are by default generated using height maps based on multiple layers of perlin noise. These are supplied to the Mesh Generation system in the form of objects implementing the IHeightMap interface. 

## 5. Acknowledgements

A number of core algorithms in this project (namely cellular automata, marching squares and the noise functions used for height maps) were learned from Sebastian Lagues videos on Procedural Cave Generation and Procedural Landmass Generation. Those videos and others can be found on his youtube channel [here](https://www.youtube.com/user/Cercopithecan). He has put together some remarkable visualizations of these and other algorithms in his tutorials. 