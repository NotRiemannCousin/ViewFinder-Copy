# Viewfinder Copy

An exercise in Unity involves recreating a game mechanic from a game called 'Viewfinder'. This mechanic revolves around copying objects based on the camera perspective and then placing them back into the world.

<p align="center" width="100%"> 

## Trailer:
[![Trailer](https://img.youtube.com/vi/k_lIQ2EZRH8/0.jpg)](https://www.youtube.com/watch?v=k_lIQ2EZRH8)


## My Video about this:
[![My Video](https://img.youtube.com/vi/UVyv49LU4dY/0.jpg)](https://www.youtube.com/watch?v=UVyv49LU4dY)

</p>

## Important
- If the mesh is not planar, then it must be convex.
- Add the class "Slicerable" to the objects that can be copiable.
- This code just support objects with 1 material (To be fixed).
- This code runs at the main thread (to be fixed, using MeshData+Job).
- There are a default material to the new faces, that can be changed.
- The new vertices has same UV cordinates (default, same as Vector.Zero).
- The triangulation method is Fan Triangulation (to be changed).

## Relevant Scripts

### ItemController, PlayerItemsManager, etc
These scripts regulate the exchange of items. The `Photo` and `CameraScript` are the unique Items in this project. Adapted from the scripts in the 3D FPS sample from Unity. 

### CameraScript
This script just tell to `_Photo` to take a "picture" by using the `SayCheese()` method when `OnUse()`. No big deal...

### Photo
`SayCheese()` get the frustrum planes, get the `Slicerable` objects that are in the frustum and get the images for the background and the `Photo` object.
The `CopyObjects()` copy the objects (the `lightmapIndex` and `lightmapScaleOffset` are not Serialized so they can't be copied normally), cut the meshes and set the PhotoOutput object (a object with the same orientation of camera) as it parent and after all objects it sets the PhotoOutput as inactive. This method maybe will use Jobs in the future.
The `OnUse()` set the PhotoOutput as active and set the orientation to the same of camera again.

### Slicerable
It's just a script to separate the objects that can be copied and cutted from that cannot. It's also holds some data like the material of the cutted faces.

### MeshUtils
A static class that has some methods for mesh manipulation.
The meshes just are cutted in `MeshCut()`. The steps of this code are showed in my video.