# Cotangent

3D printing application. MIT License.

This is Cotangent 1.0.5, the same version as is published on https://www.cotangent.io


# Set Up Build

## Install Unity

Current repository requires **Unity 2018.1.9f1**. You can download old versions of Unity here: https://unity3d.com/get-unity/download/archive

## Get Submodules

run 

    git submodule update --init --recursive
    
to check out all the submodules


## Open Project In Unity

There will be many errors

## Unzip gsUnityVR

Unzip the file Assets\frame3Sharp\gsUnityVR.zip. This will create a fake unity asmdef module that is required due to Unity dependency weirdness.

## Install Vectrosity

Import **Vectrosity5-Unity5.6.unitypackage** from the Vectrosity 5.5 package zip. 

(Yes, you will need to buy a Vectrosity license from https://starscenesoftware.com/vectrosity.html. Sorry, no way around this currently. You are welcome to try to remove this dependency and submit a PR)

## fix Shaders

There is a bug in Unity 2018.1 that will cause two of the shaders in f3Sharp to fail to compile, resulting in f3Sharp dll not building, and a bunch of "missing f3 namespace" errors. The culprit errors that cause this cascade will look like this:

    Shader error in 'f3/f3VtxColorFlatShaded': invalid subscript 'uv2' at /Program Files/Unity/Editor/Data/CGIncludes/UnityStandardMeta.cginc(21) (on d3d11)
    
To fix this, find each shader, right click on it, and select **Reimport** from the menu, as described here: https://www.reddit.com/r/Unity3D/comments/8kjzdf/standard_shader_invalid_subscript_uv2_error/. You should have to do this for the    **f3StandardVtxColor** and **f3VtxColorFlatShaded** shaders in Assets\frame3Sharp\Resources\StandardMaterials.

## Close and Re-Open Unity

this will produce the f3 dll and get rid of all the "missing f3 namespace" errors

## Open Cotangent Scene

in Assets\cotangentApp\CotangentScene

## Hit Play

It should work...


