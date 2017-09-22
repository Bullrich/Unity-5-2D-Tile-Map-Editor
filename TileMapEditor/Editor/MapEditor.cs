using UnityEditor;
using UnityEngine;

namespace TileMapEditor
{
    [CustomEditor(typeof(Map))]
    public class MapEditor : Editor
    {
        private Map _map;

        public override void OnInspectorGUI()
        {
            // should show tile texture, and a variable for tile padding and map size. Should count all the tiles
            if (_map == null)
                _map = (Map) target;

            EditorGUILayout.BeginVertical();
            RenderHorizontalLabel("Total tiles:", _map.tiles.Length.ToString());
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Map's texture");
            GUI.enabled = false;
            _map.texture = (Texture2D) EditorGUILayout.ObjectField(_map.texture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            RenderHorizontalLabel("Tile padding:", string.Format("{0} | {1}", _map.tilePadding.x, _map.tilePadding.y));
            RenderHorizontalLabel("Map's size", string.Format("x: {0} | y: {1}", _map.mapSize.x, _map.mapSize.y));

            EditorGUILayout.EndVertical();
        }

        private void RenderHorizontalLabel(string description, string result)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(description);
            EditorGUILayout.LabelField(result);
            EditorGUILayout.EndHorizontal();
        }
    }
}