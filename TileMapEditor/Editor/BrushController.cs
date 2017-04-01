using UnityEngine;
using UnityEditor;
//By @JavierBullrich

namespace TileMapEditor {
	public class BrushController : Editor {
        TileBrush brush;

        public void CreateBrush(TileMap map) {
            var sprite = map.currentTileBrush;

            if (sprite != null) {
                GameObject go = new GameObject("Brush");
                go.transform.SetParent(map.transform);

                brush = go.AddComponent<TileBrush>();
                brush.renderer2D = go.AddComponent<SpriteRenderer>();
                brush.renderer2D.sortingOrder = 1000;
                var pixelsToUnits = map.pixelsToUnits;
                brush.brushSize = new Vector2(sprite.textureRect.width / pixelsToUnits, sprite.textureRect.height / pixelsToUnits);
                brush.UpdateBrush(sprite);
            }
        }
        public void NewBrush(TileMap map) {
            if (brush == null) {
                if (GameObject.Find("Brush") != null)
                    brush = map.transform.FindChild("Brush").GetComponent<TileBrush>();
                else
                    CreateBrush(map);
            }
        }

        public void RandomTile(bool random) {
            if (random)
                brush.renderer2D.color = new Color(1f, 1f, 1f, .5f);
            else
                brush.renderer2D.color = new Color(1f, 1f, 1f, 1f);
        }

        public bool BrushExists() {
            return (brush != null);
        }

        public void DestroyBrush(Transform tileMapTran) {
            foreach (Transform t in tileMapTran)
                if (t.GetComponent<TileBrush>() != null)
                    DestroyImmediate(t.gameObject);
        }

        public void UpdateBrush(Sprite sprite) {
            if (brush != null)
                brush.UpdateBrush(sprite);
        }

        public void MoveBrush(TileMap map ,Vector2 mouseHitPos, bool mouseOnMap) {
            var tileSize = map.tileSize.x / map.pixelsToUnits;

            var x = Mathf.Floor(mouseHitPos.x / tileSize) * tileSize;
            var y = Mathf.Floor(mouseHitPos.y / tileSize) * tileSize;

            var row = x / tileSize;
            var column = Mathf.Abs(y / tileSize) - 1;

            if (!mouseOnMap)
                return;

            int id = Mathf.RoundToInt((column * map.mapSize.x) + row);

            brush.tileID = id;

            x += map.transform.position.x + tileSize / 2;
            y += map.transform.position.y + tileSize / 2;

            brush.transform.position = new Vector3(x, y, map.transform.position.z);
            brush.brushPosition = new Vector2(row, column);
        }

        public Vector3 transformGridPosToWorldPos(Vector2 gridPos, TileMap map) {
            var tileSize = map.tileSize.x / map.pixelsToUnits;

            var x = gridPos.x * tileSize;
            var y = -((gridPos.y + 1) * tileSize);

            x += map.transform.position.x + tileSize / 2;
            y += map.transform.position.y + tileSize / 2;

            return new Vector3(x, y, map.transform.position.z);
        }

        public Vector2 GetBrushDrawPosition() {
            var id = brush.tileID.ToString();

            var posX = brush.transform.position.x;
            var posY = brush.transform.position.y;

            return new Vector2(posX, posY);
        }

        public Vector2 BrushPosition() {
            return brush.brushPosition;
        }
        public Sprite getRenderer2D() {
            return brush.renderer2D.sprite;
        }
    }
}