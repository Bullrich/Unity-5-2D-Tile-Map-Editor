using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//By @JavierBullrich

namespace TileMapEditor
{
	public class TileBrush : MonoBehaviour {

        public Vector2 brushSize = Vector2.zero;
        public int tileID = 0;
        public SpriteRenderer renderer2D;
        public Vector2 brushPosition;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, brushSize);
        }

        public void UpdateBrush(Sprite sprite)
        {
            renderer2D.sprite = sprite;
        }
    }
}