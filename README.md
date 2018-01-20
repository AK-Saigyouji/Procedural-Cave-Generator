# Procedural Cave Generator

## 0. Introduction

This is a highly customizable system for generating randomized 3D cave terrain in Unity. The project is complete, and in maintenance mode - I will continue to fix issues, but am unlikely to implement new features, and will take care to preserve the interface so that later versions can be swapped without breaking a project. 

![3D cave with height map](http://i.imgur.com/sBi6T2U.jpg)

![Rock outline cave](http://i.imgur.com/U93AITz.jpg)

![Enclosed cave](http://i.imgur.com/GS2n1Nu.jpg)

Note: The textures themselves are from an excellent free asset pack called [Natural Tiling Textures](https://www.assetstore.unity3d.com/en/#!/content/35173) by Terramorph Workshop. 

## 1. Overview of contents

CaveGeneratorUI is your interface to the generator in the editor. 

The Sample Modules folder contains samples of the customizable components of the generator, which can be plugged into the appropriate slots in the CaveGeneratorUI. Furthermore, you can write your own modules to replace the ones provided to customize the structure of the cave using your own algorithms. See the section on Modules for information on writing your own.

Scripts contains the source code. The file structure within Scripts matches the namespace structure of the project. All scripts are contained within namespaces to avoid conflicts with other code in your project. You do not need to touch the source code to use the project, but if you do wish to dig into it or to extract only a part of this project for your own use, then see the readme in Scripts for a breakdown of the codebase.

## 2. Quickstart

![CaveGeneratorUI's inspector](http://i.imgur.com/mmYcpcA.png)

Create a new empty game object and attach the CaveGeneratorUI script. Insert sample modules (provided) and materials (not provided), and if using the Rock Outline style of generator, also supply rock prefabs (not provided). High quality materials and rock assets can be found for free on the Unity store.

Run the scene, and you will see three buttons appear in the inspector. Generate Cave will create a new cave as a child of the generator, overwriting the current child. Save Single Map will generate a map using the current configuration of the map module and save it as an asset. Convert to Prefab will convert the current child into a prefab and save it into your directory along with the meshes. This allows you to exit play mode, drag the cave into your scene, and work with it in the editor. It is composed entirely of core unity objects, meaning it retains no dependency on my code. This ensures compatibility with other tools, as well as future Unity updates which could potentially break my code.

There are two types of generators currently available: Three Tiered and Rock Outline. 

Three tiered builds three meshes for the floor, ceiling and walls and is available as isometric (first image above) or enclosed (third image above). 

Rock outline builds just a floor, and then instantiates prefabs along the outlines of the floor (second image above). Multiple rock prefabs can be assigned in the inspector, and the generator will randomly pick from them. A weight can be assigned to each prefab to make certain prefabs instantiated more frequently than others. Note that in order for the prefab to be oriented correctly, it must be rotated so that the long side runs along the z-axis.

## 3. Workflow

Broadly speaking, there are three ways to work with this project. 

### 3.1 Entirely through the editor

Configure the CaveGeneratorUI through the inspector and plug in the appropriate modules. Generate caves until you find one that suits your purposes, tweaking the modules to get the right mix of properties. Convert it to a prefab, then work with that prefab directly in the editor. Note that it's very important to use the button on the inspector for CaveGeneratorUI to convert to prefab: if you simply drag the cave from the hierarchy into the assets, the prefab will not be serialized correctly (it will work until the end of the session, but your meshes will disappear when you reload the project).

### 3.2 Design in the editor, then rebuild at runtime

A downside to the first approach is that you have to save large meshes as assets. If you're generating large caves, or just a large number of them, this could increase the build size of the project to an unacceptable degree.

This approach gives you the best of both worlds: save a cave as a prefab, design content for that prefab, then save all the content but destroy the prefab. Then, at run-time, call the Generate method on CaveGeneratorUI with the same modules loaded into it that were used to generate the cave in the first place. This will produce the exact same cave, as long as you uncheck "Randomize Seeds" on CaveGeneratorUI. Alternatively, you can pass the modules as arguments to the CaveGenerator class directly through scripting.

If taking this approach, be sure to save the modules you used when generating the prefab. You'll want to duplicate the modules you used and store those duplicates somewhere safe. If you mutate the module (e.g. by generating a random cave with it, causing the seed to be rerolled) you won't be able to rebuild the cave unless you've saved all of its properties somewhere. 

An alternative to saving the module is to save a copy of the map, and then use a static map holder (Create -> AKSaigyouji -> Map Generators -> Static Map Holder) to reproduce that same map in the cave generator. The map will be saved as a PNG, which compresses the map very efficiently.

### 3.3 Design and build algorithmically at run-time

As certain recent games show us, procedurally generating content at run-time is difficult to do right. For this approach, configure modules to build the kind of caves you want, write code to build the appropriate Configuration object, then pass it to the CaveGeneratorFactory class, which will return an appropriate cave generator capable of building the corresponding cave. Alternatively, wire everything up in the editor, and then simply have a script call the generate method on the CaveGeneratorUI script. Note that these classes are in the "AKSaigyouji.CaveGeneration" namespace, so you'll need to add the appropriate using statement. The challenging part is writing code to build content for the resulting cave at run-time without knowing its structure ahead of time. 

A more feasible approach (compared to complete randomization) is a hybrid approach along the lines of, for example, Diablo 2. Generate a number of fixed chunks, place markers throughout the sections to indicate where content can be randomly generated, then assemble these pieces randomly to produce randomized yet highly structured content. The entrance carver map generator module was designed to help stitch together caves: you can slot a module into an entrance carver module, and it will carve an opening along the boundary, connecting it to the rest of the map.

I am building a framework for this approach, which can be found in my Atlas-Chart repo. This cave generator can be used in conjunction with that framework to supply the environment. 

## 4. Visual editor

The visual editor is an experimental feature (specifically, a custom editor window: access by going to Window -> AKS - Map Gen Editor) to visually create a map generator module out of other map generator modules. It's a neat tool, but suffers from the problem of lacking any obvious use cases, making it more of a toy than a tool at the moment. Nonetheless I've left it in the live build in case anyone wants to play around with it. Most of the commands can be accessed through the context window by right clicking on the grid.

## 5. Creating maps in paint programs

It's possible to draw a map in any paint program and import it into this project. The resolution of the image will determine the map size: a 45 by 50 picture will be a 45 by 50 map. Black tiles will be interpreted as wall tiles, everything else will be interpreted as floor tiles. Save the image using a lossless format (I recommend PNG), and ensure "_map" is in the name. e.g. "CaveTest_map.png". This will allow the custom asset processor to intercept the import process and configure the texture automatically.

Drag the file into your project, create an instance of the static map holder module (use the Assets/Create menu) and place the imported texture into the slot in the map holder. The module can now be used in the generator to render the drawn map as a 3D cave. 

![PNG in GIMP and Cave in Unity](http://i.imgur.com/cJswOo1.png)

This has many potential uses: it's an extremely efficient way to prototype certain types of levels, it can be used to build primitives to combine in the visual editor or in a custom module, or you can use it directly to generate a cave with specific structure without relying on modelling tools like Blender. It's especially useful if working with the Atlas-Chart framework. 

## 6. Creating new module types

The module system is designed to allow you not just to customize the modules I have provided, but to write your own either from scratch or on top of the ones provided to tailor the project to your own needs. Information on writing your own modules can be found here:

[Modules](Modules.md)

## 7. Acknowledgements

Several core algorithms in this project (namely cellular automata, marching squares and the noise functions used for height maps) were learned from Sebastian Lagues videos on Procedural Cave Generation and Procedural Landmass Generation. Those videos and others can be found on his youtube channel [here](https://www.youtube.com/user/Cercopithecan). He has put together some remarkable visualizations of these and other algorithms in his tutorials, making them a good starting point if you're interesting in getting into procedural generation.
