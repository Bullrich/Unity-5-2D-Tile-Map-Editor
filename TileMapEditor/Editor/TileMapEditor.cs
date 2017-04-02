using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//By @JavierBullrich

namespace TileMapEditor {
    [CustomEditor(typeof(TileMap))]
    public class TileMapEditor : Editor {

        public TileMap map;
        BrushController brController;

        BrushController getBrController() {
            if (brController == null)
                brController =CreateInstance<BrushController>();
            if (!brController.BrushExists())
                brController.CreateBrush(map);
            return brController;
        }

        //TileBrush brush;
        Vector3 mouseHitPos;
        Map loadMap;

        bool mouseOnMap {
            get {
                return mouseHitPos.x > 0 && mouseHitPos.x < map.gridSize.x
                  && mouseHitPos.y < 0 && mouseHitPos.y > -map.gridSize.y;
            }
        }

        public override void OnInspectorGUI() {
            if (map.tiles == null)
                map.tiles = new List<Tile>();
            EditorGUILayout.BeginVertical();

            var oldSize = map.mapSize;
            map.mapSize = EditorGUILayout.Vector2Field("Map Size:", map.mapSize);

            // Transform the vector floats into ints
            map.mapSize = new Vector2(Mathf.Round(map.mapSize.x), Mathf.Round(map.mapSize.y));

            if (map.mapSize != oldSize)
                UpdateCalculations();


            var oldTexture = map.texture2D;
            map.texture2D = (Texture2D)EditorGUILayout.ObjectField("Texture2D:", map.texture2D, typeof(Texture2D), false);

            if (oldTexture != map.texture2D) {
                UpdateCalculations();
                map.tileID = 1;
                getBrController().CreateBrush(map);
            }

            if (map.texture2D == null) {
                EditorGUILayout.HelpBox("You have not selected a texture 2D yet.\nSelect a texture or load a map", MessageType.Warning);

                EditorGUILayout.LabelField("Load map");
                loadMap = (Map)EditorGUILayout.ObjectField("Load map", loadMap, typeof(Map), false);

                if (loadMap != null) {
                    Debug.Log("A map is loaded");
                    map.texture2D = loadMap.texture;
                    map.tilePadding = loadMap.tilePadding;
                    map.gridSize = loadMap.mapSize;
                    UpdateCalculations();

                    ClearMap();
                    map.tileID = 1;

                    LoadMap(loadMap);
                    loadMap = null;
                }
            } else {
                // + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - 
                GUILayout.BeginVertical(EditorStyles.helpBox);

                MapInformation(map);

                // + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - 
                EditorGUILayout.EndVertical();

                map.mapName = EditorGUILayout.TextField("Map name: ", map.mapName);
                if (GUILayout.Button("Create Prefab"))
                    if (!IsNullOrWhiteSpace(map.mapName))
                    {
                        if (EditorUtility.DisplayDialog("Save map", "Do you want to save this map as " + map.mapName + ".prefab?\n If a prefab with the same name is found it will be overwritten.", "Yes", "No"))
                        {
                            map.tileContainer.name = map.mapName;
                            CreatePrefab(map.tileContainer);
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Name missing", "You have to set a name to the map to save it", "Ok");
                        Debug.Log("Called");
                    }
                EditorGUILayout.HelpBox("SHIFT TO ADD TILES \nALT TO DELETE THEM", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        void MapInformation(TileMap currentMap)
        {
            EditorGUILayout.LabelField("Tile Size:", currentMap.tileSize.x + "x" + currentMap.tileSize.y);
            currentMap.tilePadding = EditorGUILayout.Vector2Field("Tile Padding", currentMap.tilePadding);
            EditorGUILayout.LabelField("Grid Size in Units:", currentMap.gridSize.x + "x" + currentMap.gridSize.y);
            EditorGUILayout.LabelField("Pixels To Units:", currentMap.pixelsToUnits.ToString());
            if (!getBrController().BrushExists())
                getBrController().CreateBrush(currentMap);

            getBrController().RandomTile(currentMap.randomTile);

            getBrController().UpdateBrush(currentMap.currentTileBrush);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField((LayerMask.LayerToName(currentMap.collisionLayer) != "" ? LayerMask.LayerToName(map.collisionLayer) : "Invalid layer"));
            currentMap.collisionLayer = EditorGUILayout.IntSlider(currentMap.collisionLayer, 0, 31);
            EditorGUILayout.EndHorizontal();

            currentMap.randomTile = EditorGUILayout.Toggle("Random Tile", currentMap.randomTile);
            currentMap.spriteLayer = EditorGUILayout.IntField("Sprite Layer", currentMap.spriteLayer);

            EditorGUILayout.LabelField("COUNT: " + currentMap.tiles.Count);



            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Colliders"))
                if (currentMap.tileContainer != null)
                    AddColliders(currentMap.tileContainer);
            if (GUILayout.Button("Clear Tiles"))
                if (EditorUtility.DisplayDialog("Clear map's tiles?", "Are you sure?", "Clear", "Do not clear"))
                    ClearMap();
            EditorGUILayout.EndHorizontal();
        }

        private void LoadMap(Map loadMap) {
            map.texture2D = loadMap.texture;
            map.tilePadding = loadMap.tilePadding;
            map.gridSize = loadMap.mapSize;

            ClearMap();
            foreach (Tile til in loadMap.tiles) {
                var tileSize = map.tileSize.x / map.pixelsToUnits;
                SpawnTile(til);
            }
        }

        private void CreatePrefab(GameObject prefabOb) {
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

            EditorTools.CreatePrefab(map.tileContainer);
        }

        public static bool IsNullOrWhiteSpace(string value) {
            if (value != null)
                for (int i = 0; i < value.Length; i++)
                    if (!char.IsWhiteSpace(value[i]))
                        return false;

            return true;
        }

        private void OnEnable() {
            map = target as TileMap;
            Tools.current = Tool.View;

            if (map.tileContainer == null) {
                var go = new GameObject("Tiles");
                go.transform.SetParent(map.transform);
                go.transform.position = Vector2.zero;

                map.tileContainer = go;
            }

            if (map.texture2D != null) {
                UpdateCalculations();
                getBrController().NewBrush(map);
            }

        }

        private void OnDisable() {
            if(map.texture2D != null)
            getBrController().DestroyBrush(map.transform);
        }

        private void OnSceneGUI() {
            if (map.texture2D != null && getBrController().BrushExists()) {
                UpdateHitPosition();
                getBrController().MoveBrush(map, mouseHitPos, mouseOnMap);

                if (mouseOnMap) {
                    Event current = Event.current;
                    if (current.shift)
                        DrawTile();
                    else if (current.alt)
                        RemoveTile();
                }
            }
        }

        void AddColliders(GameObject parentObj) {
            foreach (Transform t in parentObj.transform)
                if (t.GetComponent<PolygonCollider2D>() == null)
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


        private void UpdateCalculations() {
            var path = AssetDatabase.GetAssetPath(map.texture2D);
            map.spriteReferences = AssetDatabase.LoadAllAssetsAtPath(path);

            Debug.Log(map.spriteReferences.Length);

            var sprite = (Sprite)map.spriteReferences[1];
            var width = sprite.textureRect.width;
            var height = sprite.textureRect.height;

            map.tileSize = new Vector2(width, height);

            // Calculate pixel to units
            map.pixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);
            map.gridSize = new Vector2((width / map.pixelsToUnits) * map.mapSize.x, (height / map.pixelsToUnits) * map.mapSize.y);
        }

        private void UpdateHitPosition() {

            var p = new Plane(map.transform.TransformDirection(Vector3.forward), Vector3.zero);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var hit = Vector3.zero;
            var dist = 0f;

            if (p.Raycast(ray, out dist))
                hit = ray.origin + ray.direction.normalized * dist;

            mouseHitPos = map.transform.InverseTransformPoint(hit);
        }

        void DrawTile() {
            GameObject tile;
            Vector2 brPos = getBrController().BrushPosition();
            int index = -1;
            if (!MapContainTile(brPos))
                tile = createTile(getBrController().GetBrushDrawPosition(), brPos);
            else {
                index = GetTileIndex(brPos);
                tile = map.tiles[index].tile;
            }
            Sprite tileSprite;
            if (map.randomTile)
                tileSprite = map.spriteReferences[UnityEngine.Random.Range(1, map.spriteReferences.Length)] as Sprite;

            else
                tileSprite = getBrController().getRenderer2D();
            tile.GetComponent<SpriteRenderer>().sprite = tileSprite;

            if (index > -1)
                map.tiles[GetTileIndex(brPos)].tile = tile;
            else
                map.tiles.Add(new Tile(brPos, tile));
        }

        void SpawnTile(Tile tile) {
            Vector2 tilePos = tile.position;
            GameObject tileGO = createTile(getBrController().transformGridPosToWorldPos(tile.position, map), tile.position);

            tileGO.GetComponent<SpriteRenderer>().sprite = tile.tile.GetComponent<SpriteRenderer>().sprite;

            map.tiles.Add(new Tile(tile.position, tileGO));
        }

        GameObject createTile(Vector3 tilePos, Vector2 tileIndex) {
            GameObject tile = new GameObject("tile_" + tileIndex.x + "_" + tileIndex.y);
            tile.AddComponent<SpriteRenderer>();
            tile.GetComponent<SpriteRenderer>().sortingOrder = map.spriteLayer;
            tile.transform.SetParent(map.tileContainer.transform);
            tile.transform.position = tilePos;
            return tile;
        }

        void RemoveTile() {
            Vector2 brPos = getBrController().BrushPosition();
            if (MapContainTile(brPos)) {
                DestroyImmediate(map.tiles[GetTileIndex(brPos)].tile);
                map.tiles.RemoveAt(GetTileIndex(brPos));
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