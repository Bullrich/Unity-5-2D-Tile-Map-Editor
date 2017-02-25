# 2D Tile Map

### Version: 0.8.0

This asset is a simple Tile Map Editor.

![Tile map gif](https://zippy.gfycat.com/ReflectingHandyAlleycat.gif)

It's features are:
- Setting a grid of desired size.
- Padding the tiles with the wished padding size.
- Adding a Polygon Collider 2D to the whole Tile Map (experimental)
- Saving the map as prefab
- Custom window for selecting the sprites.

---
##### How to use

First import this project into your Unity project. After that select **GameObject > TileMap** to import a TileMap editor and **Window > Tile Picker** to open the tile picker window.

#### Tile Map

In the tile map editor select a spritesheet already divided in its setting (select the sprite and in sprite mode select **Multiple**, then in _Sprite Editor_ split the sprite into the desired pieces and click apply).

When you add a sprite new options will appear.

Field | Action
--- | ---
Tile size | It show the size of individual tiles.
Tile Padding | Allows you to add a custom padding.
Grid Size in Units | It shows the size of the whole grid in Unity units.
Pixels to Unitys | It shows the [pixel to units](https://docs.unity3d.com/ScriptReference/Sprite-pixelsPerUnit.html) value of the selected sprite.
Layer | It let you select the layer mask to create the collider.
Random Tile | If checked, when you paint a tile it will paint a random one.
Sprite Layer | Decides on what sorting layer of the default layer the SpriteRenderer will be set.
Map name | Name of the map at the time of saving.
Clear Tiles | Delete all the tiles in the scene.
Create Collider | Creates a big PolygonCollider2D taking all the sprites in the map (experimental).
Create Prefab | Creates a prefab in TileMapEditor/Maps

Else, if you wish to continue work on a saved map, you can drag a map with the script Map.cs to the LoadMap section, the system will reload that map so you can continue your work where you left.

#### Tile Picker

The tile picker window will show the selected sprite. If it doesn't show anything click on it, focusing again should reload the window.

Simply click on a sprite to select it at the time of drawing. You can use the zoom factor to decide the size of the sprite in the window.

#### Controls

The controls for drawing are really simple.
- Press the _shift key_ to draw where you have the brush.
- Press the _alt key_ to delete where you have the brush.

#### Copy right and stuff

This grid was made after following a [tutorial from Lynda's site](https://www.lynda.com/Unity-tutorials/Unity-5-2D-Building-Tile-Map-Editor/384876-2.html) and then upgraded to save prefabs and add a Polygon Collision 2D using the [free open source Clipper Library](http://www.angusj.com/delphi/clipper.php).

Feel free to use, edit or upgraded it.

---

## Version History

Version | Changelog
--- | ---
0.8.0 | Realeased most of the tool working.
0.8.1 | Fixed the tool adding a SnapToGridd component to the collider.
0.9 | Added the ability to reload saved maps.
0.9.1 | Added option to create random tiles and set their sorting layer.