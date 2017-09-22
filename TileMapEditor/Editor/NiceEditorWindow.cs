using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

// By @Bullrich
namespace TileMapEditor
{
	public class NiceEditorWindow : EditorWindow {
		[MenuItem("Window/Map Editor/Delete Maps")]
		public static void OpenTilePickerWindow()
		{
			var window = EditorWindow.GetWindow(typeof(NiceEditorWindow));
			var title = new GUIContent();
			title.text = "Delete Maps";
			window.titleContent = title;
		}

		private void OnGUI()
		{
			string mapFolder = "Assets/Maps";
			if (Directory.Exists(mapFolder))
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Current maps:");
				EditorGUILayout.LabelField((Directory.GetFiles(mapFolder).Length / 2).ToString());
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Delete Maps"))
				{
					// Delete the maps
					if (EditorUtility.DisplayDialog("Delete Maps",
						"Warning, this action will delete all the maps, are you sure you want to continue?", "Yeah", "Cancel"))
					{
						FileUtil.DeleteFileOrDirectory(mapFolder);
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox("There are currently no maps", MessageType.Warning);
			}
		}
	}
}