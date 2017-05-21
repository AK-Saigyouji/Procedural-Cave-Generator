# Procedural Cave Generator

## 0. Introduction

This is a highly customizable, scalable system for generating randomized 3D cave terrain in Unity. 

![3D cave with height map](http://i.imgur.com/sBi6T2U.jpg)

![Enclosed cave](http://i.imgur.com/GS2n1Nu.jpg)

Note: The textures themselves are from an excellent free asset pack called [Natural Tiling Textures](https://www.assetstore.unity3d.com/en/#!/content/35173) by Terramorph Workshop. 

## 1. Overview of contents

CaveGeneratorUI is your interface to the generator in the editor.

The Modules folder contains the customizable components of the generator, in the form of Scriptable Objects. These can be independently customized and then plugged into the appropriate slots in the CaveGeneratorUI. Furthermore, you can write your own modules to replace the ones provided to customize the structure of the cave using your own algorithms. See the readme in the Modules folder for details on how to do that.

HeightMapVisualizer is an editor-only monobehaviour that draws a height map and automatically updates it in response to changes in the height map's properties. This facilitates the exploration of the relationship between a height map's properties and the end result, without having to bounce between the cave generator and the height map module.

## 2. Quickstart

### 2.1 In editor

Create a new empty game object, and attach the CaveGeneratorUI script. Set the properties in the inspector. You can find sample modules for the Map Generator, Floor Height Map and Ceiling Height Map slots in the Modules folder.

Run the scene, and you will see two buttons appear in the inspector: Generate New Cave and Create Prefab. Generating a new map will create a new cave, overwriting any previously generated cave. Creating a prefab will convert the current cave into a prefab and save it into your directory in a folder called "GeneratedCave" along with the meshes. This allows you to exit play mode, drag the cave into your scene, and work with it in the editor. Once a cave has been converted to a prefab, it retains no dependency on the CaveGenerator, nor on any script from this project: it's composed entirely of core Unity objects.

The modules required by the generator are Scriptable Objects and can themselves be configured. You can also create additional ones by going through the menu: Assets -> Create -> Cave Generation and selecting the appropriate options, or by duplicating an existing module. This allows you to define specific configurations of the various modules and save them as independent assets. 

### 2.2 In code

Note: the API for the cave generator has gone through a complete redesign several times already, and this may happen again in the future. As such, backward compatibility should not be expected in a given version.

You can also create caves entirely through code by adding the CaveGeneration namespace. The simple way is to configure a CaveGeneratorUI in the editor, then call its Generate method through code. 

To customize and build caves entirely through code, you can use the CaveGenerator (not CaveGeneratorUI) static class. It has a method Generate which takes a CaveConfiguration object. This object's properties match those seen in the inspector for CaveGeneratorUI and can be configured through its exposed properties. 

## 3. Workflow

There are three ways to work with the generator.

#### 3.1 Entirely through the editor

Configure the CaveGeneratorUI through the inspector and plug in the appropriate modules. Generate caves until you find one that suits your purposes, tweaking the modules to get the right mix of properties. Convert it to a prefab, then work with that prefab directly in the editor. Note that it's very important to use the button on the inspector for CaveGeneratorUI to convert to prefabs: if you simply drag the cave from the hierarchy into the assets, the prefab will not be serialized correctly. 

#### 3.2 Design in the editor, then rebuild at runtime

A downside to the first approach is that you have to save large meshes as assets. If you're generating large caves, or just a large number of them, this can consume a lot of memory, which can dramatically increase the build size of the game. 

This approach gives you the best of both worlds: save a cave as a prefab, design content for that prefab, then save all the content but destroy the prefab. Then, at run-time, call the Generate method on CaveGeneratorUI with the same modules loaded into it that were used to generate the cave in the first place. This will produce the exact same cave, as long as you uncheck "Randomize Seeds" on CaveGeneratorUI. Alternatively, you can pass the modules as arguments to the CaveGenerator class directly.

If taking this approach, be sure to save the modules you used when generating the prefab. You'll want to duplicate the modules you used and store those duplicates somewhere safe. If you mutate the module (e.g. by generating a random cave with it, causing the seed to be rerolled) you won't be able to rebuild the cave unless you've saved all of its properties somewhere. 

#### 3.3 Design and build algorithmically at run-time.

This is by far the most difficult approach, but allows for unlimited content as your game will generate a new, original cave every time. Configure modules to build the kind of caves you want, build a CaveConfiguration object, then pass them it to the CaveGenerator class, which will return a Cave object. You can then use this object to design algorithms to place content based on the resulting structure of the cav.e 

Note that the default map generator can be difficult to use in this third approach, as there are few guaranteed constraints on the output: as such, you will likely need to define your own map generator, using a more structured approach so that you have more control over the resulting cave's structure to make content easier to place. 

A very useful tool for generating run-time caves is the compound module, which allows you to stitch together multiple smaller caves, giving you complete control over the global structure while being highly randomized locally. See the readme in Modules for more information about compound modules.

## 4. Acknowledgements

A number of core algorithms in this project (namely cellular automata, marching squares and the noise functions used for height maps) were learned from Sebastian Lagues videos on Procedural Cave Generation and Procedural Landmass Generation. Those videos and others can be found on his youtube channel [here](https://www.youtube.com/user/Cercopithecan). He has put together some remarkable visualizations of these and other algorithms in his tutorials. 
