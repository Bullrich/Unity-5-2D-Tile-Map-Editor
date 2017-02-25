using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//By @JavierBullrich

namespace TileMapEditor {
    public class TileMap : MonoBehaviour {

        public Texture2D texture2D;
        public Vector2
             mapSize = new Vector2(20, 10),
            tileSize = new Vector2(),
            tilePadding = new Vector2(),
            gridSize = new Vector2();

        public Object[] spriteReferences;

        public int
            pixelsToUnits = 100,
            tileID = 0,
            spriteLayer = 0,
            collisionLayer;

        public GameObject tileContainer;
        public string mapName = "Tilemap";

        public bool randomTile;

        public List<Tile> tiles;

        public Sprite currentTileBrush {
            get { return spriteReferences[tileID] as Sprite; }
        }

        private void OnDrawGizmosSelected() {
            var pos = transform.position;

            if (texture2D != null) {
                Gizmos.color = Color.gray;
                var row = 0;
                var maxColumns = mapSize.x;
                var total = mapSize.x * mapSize.y;
                var tile = new Vector3(tileSize.x / pixelsToUnits, tileSize.y / pixelsToUnits);
                var offset = new Vector2(tile.x / 2, tile.y / 2);

                for (int i = 0; i < total; i++) {
                    var column = i % maxColumns;

                    var newX = (column * tile.x) + offset.x + pos.x;
                    var newY = -(row * tile.y) - offset.y + pos.y;
                    Gizmos.DrawWireCube(new Vector2(newX, newY), tile);

                    if (column == maxColumns - 1)
                        row++;
                }


                Gizmos.color = Color.white;
                var centerX = pos.x + (gridSize.x / 2);
                var centerY = pos.y - (gridSize.y / 2);

                Gizmos.DrawWireCube(new Vector2(centerX, centerY), gridSize);
            }
        }

    }

    [System.Serializable]
    public class Tile {
        public Vector2 position;
        public GameObject tile;

        public Tile(Vector2 pos, GameObject til) {
            position = pos;
            tile = til;
        }

        public void Update(Vector2 pos, GameObject til) {
            position = pos;
            tile = til;
        }
    }
}