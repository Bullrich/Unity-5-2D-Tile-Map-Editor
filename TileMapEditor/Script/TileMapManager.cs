using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// by @Bullrich

namespace TileMapEditor
{
	public class TileMapManager : MonoBehaviour {

        public List<TileMap> maps;
        public TileMap selectedMap;

        public string mapName = "DefaultMap";

        public GameObject CreateMap(TileMapManager managerObj) {
            GameObject parentObj = managerObj.gameObject;

            foreach (TileMap map in managerObj.maps) {
                GameObject prefabOb = map.tileContainer;
                SnapToGrid snap;
                if (prefabOb.GetComponent<SnapToGrid>() == null)
                    snap = prefabOb.AddComponent<SnapToGrid>();
                else
                    snap = prefabOb.GetComponent<SnapToGrid>();
                Map mapValues;
                if (prefabOb.GetComponent<Map>() == null)
                    prefabOb.AddComponent<Map>();
                mapValues = prefabOb.GetComponent<Map>();
                mapValues.tiles = map.tiles.ToArray();
                mapValues.texture = map.texture2D;
                mapValues.tilePadding = map.tilePadding;
                mapValues.mapSize = map.gridSize;

                snap.cell_size = (float)(100f / map.pixelsToUnits);
                snap.enabled = false;

                prefabOb.transform.SetParent(parentObj.transform);
            }
            return parentObj;
        }

        public static bool IsNullOrWhiteSpace(string value) {
            if (value != null)
                for (int i = 0; i < value.Length; i++)
                    if (!char.IsWhiteSpace(value[i]))
                        return false;

            return true;
        }

        public int GetTileIndex(TileMap currentMap, Vector2 tilePos) {
            for (int i = 0; i < currentMap.tiles.Count; i++) {
                if (currentMap.tiles[i].position == tilePos)
                    return i;
            }
            Debug.LogError("The index of the tile " + tilePos + " was not found!");
            return 0;
        }

        public void DrawTile(TileMap currentMap, Vector2 brPos, Vector2 brushDrawPosition, Sprite drawSprite) {
            GameObject tile;
            int index = -1;
            if (!MapContainTile(currentMap, brPos))
                tile = createTile(currentMap, brushDrawPosition, brPos);
            else {
                index = GetTileIndex(currentMap, brPos);
                tile = currentMap.tiles[index].tile;
            }
            Sprite tileSprite;
            if (currentMap.randomTile)
                tileSprite = currentMap.spriteReferences[UnityEngine.Random.Range(1, currentMap.spriteReferences.Length)] as Sprite;

            else
                tileSprite = drawSprite;
            tile.GetComponent<SpriteRenderer>().sprite = tileSprite;

            if (index > -1)
                currentMap.tiles[GetTileIndex(currentMap, brPos)].tile = tile;
            else
                currentMap.tiles.Add(new Tile(brPos, tile));
        }

        public void SpawnTile(TileMap currentMap, Tile tile, Vector3 tileSpawnPos) {
            Vector2 tilePos = tile.position;
            GameObject tileGO = createTile(currentMap, tileSpawnPos, tile.position);

            tileGO.GetComponent<SpriteRenderer>().sprite = tile.tile.GetComponent<SpriteRenderer>().sprite;

            currentMap.tiles.Add(new Tile(tile.position, tileGO));
        }

        public GameObject createTile(TileMap currentMap, Vector3 tilePos, Vector2 tileIndex) {
            GameObject tile = new GameObject("tile_" + tileIndex.x + "_" + tileIndex.y);
            tile.AddComponent<SpriteRenderer>();
            tile.GetComponent<SpriteRenderer>().sortingOrder = currentMap.spriteLayer;
            tile.transform.SetParent(currentMap.tileContainer.transform);
            tile.transform.position = tilePos;
            return tile;
        }

        public void RemoveTile(TileMap currentMap, Vector2 brPos) {
            if (MapContainTile(currentMap, brPos)) {
                DestroyImmediate(currentMap.tiles[GetTileIndex(currentMap, brPos)].tile);
                currentMap.tiles.RemoveAt(GetTileIndex(currentMap, brPos));
            }
        }

        public void ClearMap(TileMap currentMap) {
            for (int i = 0; i < currentMap.tileContainer.transform.childCount; i++) {
                Transform t = currentMap.tileContainer.transform.GetChild(i);
                DestroyImmediate(t.gameObject);
                i--;
                currentMap.tiles.Clear();
            }
        }

        public bool MapContainTile(TileMap currentMap, Vector2 tilePos) {
            foreach (Tile tile in currentMap.tiles)
                if (tile.position == tilePos)
                    return true;
            return false;
        }
    }
}
