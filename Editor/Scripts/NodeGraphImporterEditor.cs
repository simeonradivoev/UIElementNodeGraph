using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace NodeEditor.Scripts
{
	[CustomEditor(typeof(NodeGraphImporter))]
	public class NodeGraphImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open Node Editor"))
			{
				AssetImporter importer = target as AssetImporter;
				Debug.Assert(importer != null, "importer != null");
				ShowGraphEditWindow(importer.assetPath);
			}
		}

		internal static bool ShowGraphEditWindow(string path)
		{
			var guid = AssetDatabase.AssetPathToGUID(path);
			var extension = Path.GetExtension(path);
			if (extension != ".NodeGraph")
				return false;

			var foundWindow = false;
			foreach (var w in Resources.FindObjectsOfTypeAll<NodeGraphEditWindow>())
			{
				if (w.selectedGuid == guid)
				{
					foundWindow = true;
					w.Focus();
				}
			}

			if (!foundWindow)
			{
				var window = CreateInstance<NodeGraphEditWindow>();
				window.Show();
				window.Initialize(guid);
			}

			return true;
		}

		[OnOpenAsset(0)]
		public static bool OnOpenAsset(int instanceID, int line)
		{
			var path = AssetDatabase.GetAssetPath(instanceID);
			return ShowGraphEditWindow(path);
		}
	}
}