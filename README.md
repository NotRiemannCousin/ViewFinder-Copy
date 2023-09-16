# Viewfinder Copy

An exercise in Unity involves recreating a game mechanic from a game called 'Viewfinder'. This mechanic revolves around copying objects based on the camera perspective and then placing them back into the world.

<iframe width="560" height="315" src="https://www.youtube.com/embed/k_lIQ2EZRH8" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen title="Viewfinder Trailer"></iframe>

## Important
- If the mesh is not planar, then it must be convex.
- Add the class "Slicerable" to the objects that can be copiable.
- This code just support objects with 1 material (To be fixed).
- This code runs at the main thread (to be fixed, using MeshData+Job).
- There are a default material to the new faces, that can be changed.
- The new vertices has same UV cordinates (default, same as Vector.Zero).
- The triangulation method is Fan Triangulation (to be changed).
