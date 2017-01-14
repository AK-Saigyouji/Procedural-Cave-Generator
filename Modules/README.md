# Modules

This folder contains modules: customizable Scriptable Objects that define core parts of the cave generator. At the moment, there are two types of components: map generators and height maps.

## Usage

Either use a sample module provided, or create one using the menu. Samples can be found in the relevant folders in this directory. To create one using the menu, select Assets -> Create -> Cave Generation and select the relevant module. Modules offer a number of customizable properties unique to their type. 

You may wish to create several versions of the same type of module, and name them to reflect their distinct customization. As a simple example, you could have multiple map generators of the same type named Dense, Standard, and Sparse, configured with relatively high, balanced and low wall densities respectively. They can then be loaded dynamically at run-time to generate caves on demand, rather than generating all the caves before-hand. Saving a cave as a prefab requires saving their meshes, and meshes are very large objects (i.e. take a lot of memory). 

## Defining your own modules

In addition to using and customizing the provided modules, you can write your own based on custom logic and plug them into the cave generator. This section covers how to do that. At the moment two classes of modules can be written: map generators and height maps. 

### Define a new height map module

In this example, we'll write a new height map module to illustrate how it's done. We'll need to create a new C# script and do three things: extend the HeightMapModule class, implement the GetHeightMap() method, and add a CreateAssetMenu attribute.

As a simple example to illustrate the process, we'll create a sinusoidal height map.

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
            return HeightMapFactory.Build(GetHeight, 0, 1);
        }

        float GetHeight(float x, float z)
        {
            return 0.5f * (Mathf.Sin(x) + Mathf.Sin(z));
        }
    }
}
```

We can now return to the editor and select Assets -> Create -> Cave Generation -> Height Maps -> Sinusoidal and this will create a new height map for us, which can be plugged into the Cave Generator.

This is pretty limited, however, as we can't configure any of this height map's properties through the inspector. To illustrate how to do that, let's add three features to our height map: min height, which will set the base height value. Amplitude, which will stretch our height map in y-direction. And frequency, which will compress our height map in the xz plane. The way we do this is no different from exposing properties on a MonoBehaviour. Here is a simple implementation:

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
        public float minHeight = 0f;
        public float amplitude = 1f;
        public float frequency = 1f;

        public override IHeightMap GetHeightMap()
        {
            return HeightMapFactory.Build(GetHeight, minHeight, minHeight + amplitude);
        }

        float GetHeight(float x, float z)
        {
            x *= frequency;
            z *= frequency;
            return minHeight + amplitude * 0.5f * (Mathf.Sin(x) + Mathf.Sin(z));
        }
    }
}
```

Now each height map of this type that we create through the Assets menu can be independently configured with its own minheight, amplitude and frequency. 

### Define a new map generator module

Defining a map generator module is very similar to the above process, but instead of implementing HeightMapModule you need to implement MapGenModule, and instead of overriding GetHeightMap, you need to override the Generate method, which returns a Map object.

The Map class is a 2D array of 0s and 1s corresponding to floors and walls. A wealth of helper methods have been provided to facilitate the creation of Maps. In the near future I intend to offer a fully fleshed out example of a new map generator module to illustrate how to work with this class.