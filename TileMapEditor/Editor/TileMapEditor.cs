using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//By @JavierBullrich

namespace TileMapEditor
{
    [CustomEditor(typeof(TileMap))]
	public class TileMapEditor : Editor {

        public TileMap map;

        TileBrush brush;
        Vector3 mouseHitPos;

        bool mouseOnMap
        {
            get
            {
                return mouseHitPos.x > 0 && mouseHitPos.x < map.gridSize.x
                  && mouseHitPos.y < 0 && mouseHitPos.y > -map.gridSize.y;
            }
        }

        public override void OnInspectorGUI()
        {
            if (map.tiles == null)
                map.tiles = new List<Tile>();
            EditorGUILayout.BeginVertical();

            var oldSize = map.mapSize;
            map.mapSize=EditorGUILayout.Vector2Field("Map Size:", map.mapSize);

            // Transform the vector floats into ints
            map.mapSize = new Vector2(Mathf.Round(map.mapSize.x), Mathf.Round(map.mapSize.y));

            if (map.mapSize != oldSize)
                UpdateCalculations();


            var oldTexture = map.texture2D;
            map.texture2D = (Texture2D)EditorGUILayout.ObjectField("Texture2D:", map.texture2D, typeof(Texture2D), false);

            if (oldTexture != map.texture2D) {
                UpdateCalculations();
                map.tileID = 1;
                CreateBrush();
            }

            if (map.texture2D == null)
            {
                EditorGUILayout.HelpBox("You have not selected a texture 2D yet.", MessageType.Warning);
            } else
            {
                EditorGUILayout.LabelField("Tile Size:", map.tileSize.x + "x" + map.tileSize.y);
                map.tilePadding = EditorGUILayout.Vector2Field("Tile Padding", map.tilePadding);
                EditorGUILayout.LabelField("Grid Size in Units:", map.gridSize.x + "x" + map.gridSize.y);
                EditorGUILayout.LabelField("Pixels To Units:", map.pixelsToUnits.ToString());
                UpdateBrush(map.currentTileBrush);

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal("Horizontal");
                EditorGUILayout.LabelField((LayerMask.LayerToName(map.collisionLayer) != "" ? LayerMask.LayerToName(map.collisionLayer) : "Invalid layer"));
                map.collisionLayer=EditorGUILayout.IntSlider(map.collisionLayer, 0, 31);
                EditorGUILayout.EndHorizontal();

                map.mapName = EditorGUILayout.TextField("Map name: ", map.mapName);
                EditorGUILayout.LabelField("COUNT: "+ map.tiles.Count);

                if (GUILayout.Button("Clear Tiles"))
                    if (EditorUtility.DisplayDialog("Clear map's tiles?", "Are you sure?", "Clear", "Do not clear"))
                        ClearMap();

                EditorGUILayout.BeginHorizontal("Horizontal");
                if(GUILayout.Button("Create Colliders"))
                    if(map.tileContainer != null)
                        AddColliders(map.tileContainer);
                if (GUILayout.Button("Create Prefab"))
                    if (!IsNullOrWhiteSpace(map.mapName)) {
                        if (EditorUtility.DisplayDialog("Save map", "Do you want to save this map as " + map.mapName + ".prefab?\n If a prefab with the same name is found it will be overwritten.", "Yes", "No")) {
                            map.tileContainer.name = map.mapName;
                            CreatePrefab(map.tileContainer);
                        }
                    } else {
                        EditorUtility.DisplayDialog("Name missing", "You have to set a name to the map to save it", "Ok");
                        Debug.Log("Called");
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("SHIFT TO ADD TILES \nALT TO DELETE THEM", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void CreatePrefab(GameObject prefabOb) {
            SnapToGrid snap;
            if (prefabOb.GetComponent<SnapToGrid>() == null)
                snap = map.tileContainer.AddComponent<SnapToGrid>();
            else
                snap = map.tileContainer.GetComponent<SnapToGrid>();
            snap.cell_size = (float)(100f / map.pixelsToUnits);

            EditorTools.CreatePrefab(map.tileContainer);
        }

        public static bool IsNullOrWhiteSpace(string value) {
            if (value != null)
                for (int i = 0; i < value.Length; i++)
                    if (!char.IsWhiteSpace(value[i]))
                        return false;

            return true;
        }

        private void OnEnable()
        {
            map = target as TileMap;
            Tools.current = Tool.View;

            if (map.tileContainer == null) {
                var go = new GameObject("Tiles");
                go.transform.SetParent(map.transform);
                go.transform.position = Vector2.zero;

                map.tileContainer = go;
            }

            if (map.texture2D != null)
            {
                UpdateCalculations();
                NewBrush();
            }

        }

        private void OnDisable()
        {
            DestroyBrush();
        }

        private void OnSceneGUI()
        {
            if (brush != null)
            {
                UpdateHitPosition();
                MoveBrush();

                if (map.texture2D != null && mouseOnMap) {
                    Event current = Event.current;
                    if (current.shift)
                        Draw();
                    else if (current.alt)
                        RemoveTile();
                }
            }
        }

        void AddColliders(GameObject parentObj) {
            foreach (Transform t in parentObj.transform)
                if(t.GetComponent<PolygonCollider2D>()==null)
                t.gameObject.AddComponent<PolygonCollider2D>();
            FuseColliders(parentObj.transform);
        }

        void FuseColliders(Transform polygonParent) {
            List<List<Vector2>> containers = new List<List<Vector2>>();

            foreach (Transform t in polygonParent) {
                if (t.GetComponent<PolygonCollider2D>() != null) {
                    List<Vector2> points = new List<Vector2>();
                    foreach (Vector2 vect in t.GetComponent<PolygonCollider2D>().points) {
                        Vector3 vec = ((Vector3)vect + t.position) - polygonParent.transform.position;
                        points.Add(vec);
                    }
                    DestroyImmediate(t.GetComponent<PolygonCollider2D>());
                    containers.Add(points);
                }
            }

            PolygonConverter polyConverter = new PolygonConverter();


            polyConverter.CreateLevelCollider(
                polyConverter.UniteCollisionPolygons(
                    containers, polygonParent, map.collisionLayer));
        }


        private void UpdateCalculations()
        {
            var path = AssetDatabase.GetAssetPath(map.texture2D);
            map.spriteReferences = AssetDatabase.LoadAllAssetsAtPath(path);

            var sprite = (Sprite)map.spriteReferences[1];
            var width = sprite.textureRect.width;
            var height = sprite.textureRect.height;

            map.tileSize = new Vector2(width, height);
    
            // Calculate pixel to units
            map.pixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);
            map.gridSize = new Vector2((width / map.pixelsToUnits) * map.mapSize.x, (height / map.pixelsToUnits) * map.mapSize.y);
        }

        void CreateBrush()
        {
            var sprite = map.currentTileBrush;

            if (sprite != null)
            {
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

        void NewBrush()
        {
            if (brush == null)
            {
                if (GameObject.Find("Brush") != null)
                    brush = map.transform.FindChild("Brush").GetComponent<TileBrush>();
                else
                    CreateBrush();
            }
        }

        void DestroyBrush()
        {
            if (brush != null)
                DestroyImmediate(brush.gameObject);
        }

        public void UpdateBrush(Sprite sprite)
        {
            if (brush != null)
                brush.UpdateBrush(sprite);
        }

        private void UpdateHitPosition()
        {

            var p = new Plane(map.transform.TransformDirection(Vector3.forward), Vector3.zero);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var hit = Vector3.zero;
            var dist = 0f;

            if (p.Raycast(ray, out dist))
                hit = ray.origin + ray.direction.normalized * dist;

            mouseHitPos = map.transform.InverseTransformPoint(hit);

            //Debug.Log(mouseHitPos.ToString());
        }

        void MoveBrush()
        {
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

            //Debug.Log("x " + row + " y " + column);
        }

        void Draw() {
            var id = brush.tileID.ToString();

            var posX = brush.transform.position.x;
            var posY = brush.transform.position.y;

            GameObject tile;// = GameObject.Find(map.name + "/"+ map.tileContainer.name + "/tile_" + id);

            int index = -1;

            if (!MapContainTile(brush.brushPosition)) {
                tile = new GameObject("tile_" + brush.brushPosition.x + "_" + brush.brushPosition.y);
                tile.AddComponent<SpriteRenderer>();
                tile.transform.SetParent(map.tileContainer.transform);
                tile.transform.position = new Vector3(posX, posY, 0);
            } else {
                index = GetTileIndex(brush.brushPosition);
                tile = map.tiles[index].tile;
            }

            tile.GetComponent<SpriteRenderer>().sprite = brush.renderer2D.sprite;
            if (index > -1)
                map.tiles[GetTileIndex(brush.brushPosition)].tile = tile;
            else
                map.tiles.Add(new Tile(brush.brushPosition, tile));
        }

        void RemoveTile() {
            var id = brush.tileID.ToString();

            //GameObject tile = GameObject.Find(map.name + "/" + map.tileContainer.name + "/tile_" + id);

            /*if (map.tiles.ContainsKey(brush.brushPosition)) {
                DestroyImmediate(map.tiles[brush.brushPosition]);
                map.tiles.Remove(brush.brushPosition);
            }*/

            if (MapContainTile(brush.brushPosition)) {
                DestroyImmediate(map.tiles[GetTileIndex(brush.brushPosition)].tile);
                map.tiles.RemoveAt(GetTileIndex(brush.brushPosition));
            }
        }

        void ClearMap() {
            for (int i = 0; i < map.tileContainer.transform.childCount; i++) {
                Transform t = map.tileContainer.transform.GetChild(i);
                DestroyImmediate(t.gameObject);
                i--;
                map.tiles.Clear();
            }
        }

        public bool MapContainTile(Vector2 tilePos) {
            foreach (Tile tile in map.tiles)
                if (tile.position == tilePos)
                    return true;
            return false;
        }

        public int GetTileIndex(Vector2 tilePos) {
            for (int i = 0; i < map.tiles.Count; i++) {
                if (map.tiles[i].position == tilePos)
                    return i;
            }
            Debug.LogError("The index of the tile " + tilePos + " was not found!");
            return 0;
        }
    }
}