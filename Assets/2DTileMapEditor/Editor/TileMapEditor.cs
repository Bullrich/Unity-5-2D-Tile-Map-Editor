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

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();
            map.MapSize=EditorGUILayout.Vector2Field("Map Size:", map.MapSize);
            map.texture2D = (Texture2D)EditorGUILayout.ObjectField("Texture2D:", map.texture2D, typeof(Texture2D), false);

            if (map.texture2D == null)
            {
                EditorGUILayout.HelpBox("You have not selected a texture 2D yet.", MessageType.Warning);
            } else
            {
                EditorGUILayout.LabelField("Tile Size:", map.tileSize.x + "x" + map.tileSize.y);
                EditorGUILayout.LabelField("Grid Size in Units:", map.gridSize.x + "x" + map.gridSize.y);
                EditorGUILayout.LabelField("Pixels To Units:", map.pixelsToUnits.ToString());
            }

            EditorGUILayout.EndVertical();
        }

        private void OnEnable()
        {
            map = target as TileMap;
            Tools.current = Tool.View;

            if (map.texture2D != null)
            {
                var path = AssetDatabase.GetAssetPath(map.texture2D);
                map.spriteReferences = AssetDatabase.LoadAllAssetsAtPath(path);

                var sprite = (Sprite)map.spriteReferences[1];
                var width = sprite.textureRect.width;
                var height = sprite.textureRect.height;

                map.tileSize = new Vector2(width, height);
                // Calculate pixel to units
                map.pixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);
                map.gridSize = new Vector2((width / map.pixelsToUnits) * map.MapSize.x, (height / map.pixelsToUnits) * map.MapSize.y);

            }

        }


    }
}