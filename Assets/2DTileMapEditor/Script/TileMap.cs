using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//By @JavierBullrich

namespace TileMapEditor
{
	public class TileMap : MonoBehaviour {

        public Vector2 MapSize = new Vector2(20, 10);
        public Texture2D texture2D;
        public Vector2 tileSize = new Vector2();
        public Object[] spriteReferences;
        public Vector2 gridSize = new Vector2();
        public int pixelsToUnits = 100;



        private void OnDrawGizmosSelected()
        {
            var pos = transform.position;

            if (texture2D != null)
            {
                Gizmos.color = Color.white;
                var centerX = pos.x + (gridSize.x / 2);
                var centerY = pos.y - (gridSize.y / 2);

                Gizmos.DrawWireCube(new Vector2(centerX, centerY), gridSize);
            }
        }

    }
}