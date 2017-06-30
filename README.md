# Procedural Cave Generator

## 0. Introduction

This is a highly customizable and scalable system for generating randomized 3D cave terrain in Unity. 

![3D cave with height map](http://i.imgur.com/sBi6T2U.jpg)

![Rock outline cave](http://i.imgur.com/U93AITz.jpg)

![Enclosed cave](http://i.imgur.com/GS2n1Nu.jpg)

Note: The textures themselves are from an excellent free asset pack called [Natural Tiling Textures](https://www.assetstore.unity3d.com/en/#!/content/35173) by Terramorph Workshop. 

The generator makes use of several systems: 

The Module system is an extension of ScriptableObject with improved support for composition. 

The Map Generation system provides a wealth of functionality for generating modules capable of producing 2D grids of binary tiles (floors and walls) called Maps: a visual editor for stitching together other modules to produce composite modules; support for converting between maps and textures, allowing maps to be produced in seconds in any paint program; and a highly optimized library of algorithms for generating maps.  

The Mesh Generation system converts 2D grids (in particular, Maps) into 3D meshes. The heart of this process is the Marching Squares algorithm. 

Finally, the Cave Generation system ties everything together to produce 3D terrain from scratch, relying heavily on modules to make it easy to swap functionality in and out and to author custom modules without touching a line of the original source code.

## 1. Overview of contents

CaveGeneratorUI is your interface to the generator in the editor. 

The Sample Modules folder contains samples of the customizable components of the generator, which can be plugged into the appropriate slots in the CaveGeneratorUI. Furthermore, you can write your own modules to replace the ones provided to customize the structure of the cave using your own algorithms. See the section on Modules for information on writing your own.

HeightMapVisualizer and MapGenVisualizer are MonoBehaviours that visualize the respective type of module, useful if you want immediate visual feedback while tinkering with the properties of a given module. 

Sample Modules contains sample instances of modules ready to be plugged into CaveGeneratorUI. Additional instances can be created through the Assets->Create menu or by duplicating the existing modules.

Scripts contains the source code. The file structure within Scripts matches the namespace structure of the project. All scripts are contained within namespaces to avoid conflicts with other code in your project. You do not need to touch the source code to use the project, but if you do wish to dig into it or to extract only a part of this project for your own use, then see the readme in Scripts for an overview. As of this writing, there's about a hundred code files split over eleven namespaces. 

## 2. Quickstart

### 2.1 In editor

[CaveGeneratorUI's inspector](http://i.imgur.com/mmYcpcA.png)

Create a new empty game object, and attach the CaveGeneratorUI script. Insert sample modules (provided) and materials (not provided), and if using the Rock Outline style of generator, also supply rock prefabs (not provided). High quality materials and rock assets can be found for free on the Unity store. Most modules also have parameters that can be tuned to control the results.

Run the scene, and you will see three buttons appear in the inspector. Generating Cave will create a new cave as a child of the generator, overwriting the current child. Save Single Map will generate a map using the current configuration of the map module and save it as an asset. Creating a prefab will convert the current child into a prefab and save it into your directory along with the meshes. This allows you to exit play mode, drag the cave into your scene, and work with it in the editor. It is composed entirely of core unity objects, meaning it retains no dependency on my code. This ensures compatibility with other tools, as well as future patches which may in principle break my code.  

There are two types of generators currently available: Three Tiered and Rock Outline. 

Three tiered builds three meshes for the floor, ceiling and walls and is available as isometric (first image above) or enclosed (third image above). 

Rock outline builds just a floor, and then instantiates rock prefabs along the outlines of the floor (second image above). Multiple rock prefabs can be assigned in the inspector, and the generator will randomly pick from them. A weight can be assigned to each prefab to make certain prefabs instantiated more frequently than others. Note that in order for the prefab to be oriented correctly, it must be rotated so that the long side runs along the z-axis. The prefab will be rotated so that the z-axis runs along the outline itself. 

### 2.2 In code

Note: the API for the cave generator has gone through a complete redesign several times already, and this may happen again in the future. As such, backward compatibility should not be expected in a given version.

You can access the major features of this project entirely through code, including the generator itself. At a minimum you'll need to import AKSaigyouji.CaveGeneration, and access the static class CaveGenerator. All of my namespaces start with "AKSaigyouji." - this conforms to the best practice of using a distinct name at the start of namespaces for third party code to minimize the possibility of namespace conflicts. 

## 3. Workflow

Broadly speaking, there are three ways to work with this project. 

### 3.1 Entirely through the editor

Configure the CaveGeneratorUI through the inspector and plug in the appropriate modules. Generate caves until you find one that suits your purposes, tweaking the modules to get the right mix of properties. Convert it to a prefab, then work with that prefab directly in the editor. Note that it's very important to use the button on the inspector for CaveGeneratorUI to convert to prefab: if you simply drag the cave from the hierarchy into the assets, the prefab will not be serialized correctly (it will work until the end of the session, but your meshes will disappear when you reload the project).

### 3.2 Design in the editor, then rebuild at runtime

A downside to the first approach is that you have to save large meshes as assets. If you're generating large caves, or just a large number of them, this can consume a lot of memory and increase the build size of the game to an unacceptable size.

This approach gives you the best of both worlds: save a cave as a prefab, design content for that prefab, then save all the content but destroy the prefab. Then, at run-time, call the Generate method on CaveGeneratorUI with the same modules loaded into it that were used to generate the cave in the first place. This will produce the exact same cave, as long as you uncheck "Randomize Seeds" on CaveGeneratorUI. Alternatively, you can pass the modules as arguments to the CaveGenerator class directly.

If taking this approach, be sure to save the modules you used when generating the prefab. You'll want to duplicate the modules you used and store those duplicates somewhere safe. If you mutate the module (e.g. by generating a random cave with it, causing the seed to be rerolled) you won't be able to rebuild the cave unless you've saved all of its properties somewhere. 

An alternative to saving the module is to save a copy of the map, and then use a static map holder to reproduce that same map in the cave generator. The map will be saved as a PNG, which does an excellent job of compressing the map data so that it takes significantly less than 1 bit per tile. 

### 3.3 Design and build algorithmically at run-time

As certain recent games show us, procedurally generating content at run-time is difficult to do right. At the moment, support for this type of generation is limited, though I am working on some tools. For this approach, configure modules to build the kind of caves you want, write code to build the appropriate Configuration object, then pass it to the CaveGenerator class, which will return the cave. Alternatively, wire everything up in the editor, and then simply have a script call the generate method on the CaveGeneratorUI script. The challenging part is writing code to build content for the resulting cave at run-time without knowing its structure ahead of time. 

A more feasible approach (compared to complete randomization) is a hybrid approach along the lines of, for example, Diablo 2. Generate a number of fixed sections, place markers throughout the sections to indicate where content can be randomly generated, then assemble these pieces randomly to produce randomized yet highly structured content. The entrance carver map generator module was designed to help stitch together caves: you can slot a module into an entrance carver module, and it will carve an opening along the boundary, connecting it to the rest of the map. This is an approach I'm investigating more closely - the difficult infrastructure is already in place, I just need to wire it up and give it some polish.

## 4. Visual editor

Access the visual editor by going to Window, and selecting "AKS - Map Gen Editor". This is an incomplete feature, but is nonetheless usable in its current state. 

The visual editor is a custom editor window that allows you to stitch together map generator modules out of existing modules. The editor shows module outlines (for dynamic map generators, this means a boundary outline, since the exact outline will depend on the seed), allowing for multiple modules to be stitched together. I wish to experiment with the idea of allowing users to add markers to specific spots in various static maps, and giving each marker a string. These strings could either be used as keys in a dictionary to access the various markers at run-time to add content, or possibly as codes to automatically generate content. This would allow for the rapid generation of randomized maps in the style of Diablo 2, such as the den of evil, catacombs, sewers, durance of hate, etc, without having to come up with sophisticated algorithms that can understand the structure of a random cave. 

## 5. Creating maps in paint programs

It's possible to draw a map in any paint program and import it into this project. The resolution of the image will determine the map size: a 45 by 50 picture will be a 45 by 50 map. Black tiles will be interpreted as wall tiles, everything else will be interpreted as floor tiles. Save the image using a lossless format (I recommend PNG), and make sure "_map" is in the name. e.g. "CaveTest_map.png". This will allow the custom asset processor to intercept the import process and configure the texture so you don't have to mess with any of the settings. 

Drag the file into your project, create an instance of the static map holder module (use the Assets/Create menu) and place the imported texture into the slot in the map holder. The module can now be used in the generator to render the drawn map as a 3D cave. 

![PNG in GIMP and Cave in Unity](http://i.imgur.com/cJswOo1.png)

This has many potential uses: it's an extremely efficient way to prototype certain types of levels, it can be used to build primitives to combine in the visual editor or in a custom module, or you can use it directly to generate a cave with specific structure without relying on modelling tools like Blender.

## 6. Creating new module types

The module system is designed to allow you not just to customize the modules I have provided, but to write your own either from scratch or on top of the ones provided to tailor the project to your own needs. Information on writing your own modules can be found here:

[Modules](Modules.md)

## 7. Acknowledgements

Several core algorithms in this project (namely cellular automata, marching squares and the noise functions used for height maps) were learned from Sebastian Lagues videos on Procedural Cave Generation and Procedural Landmass Generation. Those videos and others can be found on his youtube channel [here](https://www.youtube.com/user/Cercopithecan). He has put together some remarkable visualizations of these and other algorithms in his tutorials. 
