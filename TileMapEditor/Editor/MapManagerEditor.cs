using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// by @Bullrich

namespace TileMapEditor {
    [CustomEditor(typeof(TileMapManager))]
    public class MapManagerEditor : Editor {

        public TileMapManager manager;
        BrushController brController;

        BrushController getBrController() {
            if (brController == null)
                brController = CreateInstance<BrushController>();
            if (!brController.BrushExists())
                brController.CreateBrush(selectedMap());
            return brController;
        }

        Vector3 mouseHitPos;
        Map loadMap;

        bool mouseOnMap {
            get {
                return mouseHitPos.x > 0 && mouseHitPos.x < selectedMap().gridSize.x
                  && mouseHitPos.y < 0 && mouseHitPos.y > -selectedMap().gridSize.y;
            }
        }

        TileMap selectedMap() {
            return manager.selectedMap;
        }

        public override void OnInspectorGUI() {

            TileMap map = selectedMap();
            if (map.tiles == null)
                map.tiles = new List<Tile>();

            EditorGUILayout.BeginVertical();
            // + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - 
            GUILayout.BeginVertical(EditorStyles.helpBox);

            var oldTexture = map.texture2D;
            map.texture2D = (Texture2D)EditorGUILayout.ObjectField("Texture2D:", map.texture2D, typeof(Texture2D), false);

            if (oldTexture != map.texture2D) {
                UpdateCalculations(map);
                map.tileID = 1;
                getBrController().CreateBrush(map);
            }

            if (map.texture2D == null) {

                EditorGUILayout.HelpBox("You have not selected a texture 2D yet.\nSelect a texture or load a map", MessageType.Warning);

                EditorGUILayout.LabelField("Load map");
                loadMap = (Map)EditorGUILayout.ObjectField("Load map", loadMap, typeof(Map), false);

                if (loadMap != null) {
                    LoadPremadeMap(map);
                }
            } else {

                var oldSize = map.mapSize;
                map.mapSize = EditorGUILayout.Vector2Field("Map Size:", map.mapSize);

                // Transform the vector floats into ints
                map.mapSize = new Vector2(Mathf.Round(map.mapSize.x), Mathf.Round(map.mapSize.y));

                if (map.mapSize != oldSize)
                    UpdateCalculations(map);

                MapInformation(map);

                EditorGUILayout.HelpBox("SHIFT TO ADD TILES \nALT TO DELETE THEM", MessageType.Info);
            }
            // + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - 
            GUILayout.EndVertical();

            foreach (TileMap _map in manager.maps) {
                if (map != _map)
                    GUILayout.Label(_map.layerName);
            }

            map.mapName = EditorGUILayout.TextField("Map name: ", manager.mapName);
            if (manager.maps.Count > 0)
                if (GUILayout.Button("Create Prefab"))
                    if (!TileMapManager.IsNullOrWhiteSpace(manager.mapName)) {
                        if (EditorUtility.DisplayDialog("Save map", 
                            "Do you want to save this map as " + manager.mapName + ".prefab?\n If a prefab with the same name is found it will be overwritten.", "Yes", "No")) {
                            map.tileContainer.name = map.layerName;
                            CreatePrefab(manager.maps);
                        }
                    } else {
                        EditorUtility.DisplayDialog("Name missing", "You have to set a name to the map to save it", "Ok");
                        Debug.Log("Called");
                    }

            EditorGUILayout.EndVertical();
        }

        void LoadPremadeMap(TileMap currentMap) {
            Debug.Log("A map is loaded");
            currentMap.texture2D = loadMap.texture;
            currentMap.tilePadding = loadMap.tilePadding;
            currentMap.gridSize = loadMap.mapSize;
            UpdateCalculations(currentMap);

            manager.ClearMap(currentMap);
            currentMap.tileID = 1;

            LoadMap(currentMap, loadMap);
            loadMap = null;
        }

        void MapInformation(TileMap currentMap) {
            currentMap.layerName = EditorGUILayout.TextField("Layer name: ", currentMap.layerName);

            EditorGUILayout.LabelField("Tile Size:", currentMap.tileSize.x + "x" + currentMap.tileSize.y);
            currentMap.tilePadding = EditorGUILayout.Vector2Field("Tile Padding", currentMap.tilePadding);
            EditorGUILayout.LabelField("Grid Size in Units:", currentMap.gridSize.x + "x" + currentMap.gridSize.y);
            EditorGUILayout.LabelField("Pixels To Units:", currentMap.pixelsToUnits.ToString());
            if (!getBrController().BrushExists())
                getBrController().CreateBrush(currentMap);

            getBrController().RandomTile(currentMap.randomTile);

            getBrController().UpdateBrush(currentMap.currentTileBrush);

            EditorGUILayout.Space();

            

            currentMap.randomTile = EditorGUILayout.Toggle("Random Tile", currentMap.randomTile);
            currentMap.spriteLayer = EditorGUILayout.IntField("Sprite Layer", currentMap.spriteLayer);
            //currentMap.spriteSortingLayer.id = EditorGUILayout.IntField("Sprite Layer " + currentMap.spriteSortingLayer.name, currentMap.spriteSortingLayer.id);
            //currentMap.spriteSortingLayer = SortingLayer.layers

            EditorGUILayout.LabelField("COUNT: " + currentMap.tiles.Count);



            EditorGUILayout.BeginHorizontal();
            currentMap.hasColliders = EditorGUILayout.Toggle("Has colliders", currentMap.hasColliders);
            if (GUILayout.Button("Clear Tiles"))
                if (EditorUtility.DisplayDialog("Clear map's tiles?", "Are you sure?", "Clear", "Do not clear"))
                    manager.ClearMap(currentMap);
            EditorGUILayout.EndHorizontal();

            if (currentMap.hasColliders) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Collider label: ");
                currentMap.collisionLayer = EditorGUILayout.LayerField(currentMap.collisionLayer);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void LoadMap(TileMap mapSelected, Map loadMap) {
            mapSelected.texture2D = loadMap.texture;
            mapSelected.tilePadding = loadMap.tilePadding;
            mapSelected.gridSize = loadMap.mapSize;

            manager.ClearMap(mapSelected);
            foreach (Tile til in loadMap.tiles) {
                var tileSize = mapSelected.tileSize.x / mapSelected.pixelsToUnits;
                manager.SpawnTile(mapSelected, til, getBrController().transformGridPosToWorldPos(til.position, mapSelected));
            }
        }

        private void CreatePrefab(List<TileMap> maps) {

            foreach (TileMap map in maps) {
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

                EditorTools.CreatePrefab(map.tileContainer);
            }
        }

        private void CreateMap(TileMapManager managerObj) {
            GameObject prefabObj = manager.CreateMap(managerObj);

            EditorTools.CreatePrefab(prefabObj);
        }

        private void OnEnable() {
            manager = target as TileMapManager;
            Tools.current = Tool.View;


            if (selectedMap() == null) {
                GameObject newMap = new GameObject("map");
                newMap.transform.SetParent(manager.transform);
                newMap.transform.position = Vector2.zero;
                TileMap newCreatedMap = newMap.AddComponent<TileMap>();
                manager.maps = new List<TileMap>();
                manager.maps.Add(newCreatedMap);
                manager.selectedMap = manager.maps[0];
            }

            if (selectedMap().tileContainer == null) {
                var go = new GameObject("Tiles");
                go.transform.SetParent(selectedMap().transform);
                go.transform.position = Vector2.zero;

                selectedMap().tileContainer = go;
            }

            if (selectedMap().texture2D != null) {
                UpdateCalculations(selectedMap());
                getBrController().NewBrush(selectedMap());
            }

        }

        private void OnDisable() {
            if (selectedMap().texture2D != null)
                getBrController().DestroyBrush(selectedMap().transform);
        }

        private void OnSceneGUI() {
            if (selectedMap().texture2D != null && getBrController().BrushExists()) {
                UpdateHitPosition(selectedMap());
                getBrController().MoveBrush(selectedMap(), mouseHitPos, mouseOnMap);

                if (mouseOnMap) {
                    Event current = Event.current;
                    if (current.shift)
                        manager.DrawTile(selectedMap(), getBrController().BrushPosition(), getBrController().GetBrushDrawPosition(), getBrController().getRenderer2D());
                    else if (current.alt)
                        manager.RemoveTile(selectedMap(), getBrController().BrushPosition());
                }
            }
        }

        void AddColliders(GameObject parentObj, LayerMask collisionLayer) {
            foreach (Transform t in parentObj.transform)
                if (t.GetComponent<PolygonCollider2D>() == null)
                    t.gameObject.AddComponent<PolygonCollider2D>(); 
            FuseColliders(parentObj.transform, collisionLayer);
        }

        void FuseColliders(Transform polygonParent, LayerMask collisionLayer) {
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
                    containers, polygonParent, collisionLayer));
        }


        private void UpdateCalculations(TileMap currentMap) {
            var path = AssetDatabase.GetAssetPath(currentMap.texture2D);
            currentMap.spriteReferences = AssetDatabase.LoadAllAssetsAtPath(path);

            Debug.Log(currentMap.spriteReferences.Length);

            var sprite = (Sprite)currentMap.spriteReferences[1];
            var width = sprite.textureRect.width;
            var height = sprite.textureRect.height;

            currentMap.tileSize = new Vector2(width, height);

            // Calculate pixel to units
            currentMap.pixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);
            currentMap.gridSize = new Vector2((width / currentMap.pixelsToUnits) * currentMap.mapSize.x, (height / currentMap.pixelsToUnits) * currentMap.mapSize.y);
        }

        private void UpdateHitPosition(TileMap currentMap) {

            var p = new Plane(currentMap.transform.TransformDirection(Vector3.forward), Vector3.zero);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var hit = Vector3.zero;
            var dist = 0f;

            if (p.Raycast(ray, out dist))
                hit = ray.origin + ray.direction.normalized * dist;

            mouseHitPos = currentMap.transform.InverseTransformPoint(hit);
        }

        

        
    }
}