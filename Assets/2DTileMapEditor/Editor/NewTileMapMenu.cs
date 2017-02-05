using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//By @JavierBullrich

namespace TileMapEditor
{
	public class NewTileMapMenu {

        [MenuItem("GameObject/TileMap")]
        public static void CreateTileMap()
        {
            GameObject go = new GameObject("TileMap");
            go.AddComponent<TileMap>();
        }
	}
}