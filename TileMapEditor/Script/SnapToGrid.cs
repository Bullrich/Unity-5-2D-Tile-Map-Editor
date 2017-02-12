using UnityEngine;
using System.Collections;

namespace TileMapEditor {
    [ExecuteInEditMode]
    public class SnapToGrid : MonoBehaviour {

        public float cell_size = 1f; // = larghezza/altezza delle celle
        private float x, y, z;
        [Tooltip("Defines if this function should be applyed or not")]
        public bool lockedOnGrid = true;

        void Start() {
            x = 0f;
            y = 0f;
            z = 0f;

        }

        void Update() {
            if (lockedOnGrid && !Application.isPlaying) {
                x = Mathf.Round(transform.position.x / cell_size) * cell_size;
                y = Mathf.Round(transform.position.y / cell_size) * cell_size;
                z = transform.position.z;
                transform.position = new Vector3(x, y, z);
            }
        }
    }
}