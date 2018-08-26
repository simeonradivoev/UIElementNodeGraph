using System;
using System.IO;
using System.Linq;
using System.Text;
using NodeEditor.Editor.Scripts.Views;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Editor.Scripts
{
	public class NodeGraphEditWindow : EditorWindow
	{
		[SerializeField]
		string m_Selected;

		[SerializeField]
		GraphObjectBase m_GraphObject;

		[NonSerialized]
		bool m_HasError;

		GraphEditorView m_GraphEditorView;

		GraphEditorView graphEditorView
		{
			get { return m_GraphEditorView; }
			set
			{
				if (m_GraphEditorView != null)
				{
					m_GraphEditorView.RemoveFromHierarchy();
					m_GraphEditorView.Dispose();
				}

				m_GraphEditorView = value;
				if (m_GraphEditorView != null)
				{
					m_GraphEditorView.saveRequested += UpdateAsset;
					m_GraphEditorView.showInProjectRequested += PingAsset;
					this.GetRootVisualContainer().Add(graphEditorView);
				}
			}
		}

		GraphObjectBase graphObject
		{
			get { return m_GraphObject; }
			set
			{
				if (m_GraphObject is TmpGraphObject)
					DestroyImmediate(m_GraphObject);
				m_GraphObject = value;
			}
		}

		public string selectedGuid
		{
			get { return m_Selected; }
			private set { m_Selected = value; }
		}

		public string assetName
		{
			get { return titleContent.text; }
			set
			{
				titleContent.text = value;
				graphEditorView.assetName = value;
			}
		}

		void OnDisable()
		{
			graphEditorView = null;
		}

		void OnDestroy()
		{
			if (graphObject != null)
			{
				string nameOfFile = AssetDatabase.GUIDToAssetPath(selectedGuid);
				if (graphObject.isDirty && EditorUtility.DisplayDialog("Shader Graph Has Been Modified", "Do you want to save the changes you made in the shader graph?\n" + nameOfFile + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
					UpdateAsset();
				Undo.ClearUndo(graphObject);
				if(graphObject is TmpGraphObject) DestroyImmediate(graphObject);
			}

			graphEditorView = null;
		}

		public void PingAsset()
		{
			if (selectedGuid != null)
			{
				var path = AssetDatabase.GUIDToAssetPath(selectedGuid);
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
				EditorGUIUtility.PingObject(asset);
			}
		}

		public void UpdateAsset()
		{
			if (selectedGuid != null && graphObject != null)
			{
				var path = AssetDatabase.GUIDToAssetPath(selectedGuid);
				if (string.IsNullOrEmpty(path) || graphObject == null)
					return;

				if (graphObject is TmpGraphObject && m_GraphObject.graph.GetType() == typeof(NodeGraph))
					UpdateNodeGraphOnDisk(path);
				else
				{
					EditorUtility.SetDirty(graphObject);
					AssetDatabase.SaveAssets();
				}

				graphObject.SetDirty(false);
				var windows = Resources.FindObjectsOfTypeAll<NodeGraphEditWindow>();
				foreach (var materialGraphEditWindow in windows)
				{
					materialGraphEditWindow.Rebuild();
				}
			}
		}

		public void Initialize(string assetGuid)
		{
			try
			{
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(assetGuid));
				if (asset == null)
					return;

				if (!EditorUtility.IsPersistent(asset))
					return;

				if (selectedGuid == assetGuid)
					return;

				Type graphType;
				if (asset is GraphObjectBase)
				{
					if(asset is TmpGraphObject) Debug.LogError("TmpGraphObject is only used in editor");
					graphObject = (GraphObjectBase)asset;
					graphType = typeof(NodeGraph);
				}
				else
				{
					var path = AssetDatabase.GetAssetPath(asset);
					var extension = Path.GetExtension(path);

					switch (extension)
					{
						case ".NodeGraph":
							graphType = typeof(NodeGraph);
							break;
						default:
							return;
					}

					var textGraph = File.ReadAllText(path, Encoding.UTF8);
					graphObject = CreateInstance<TmpGraphObject>();
					graphObject.hideFlags = HideFlags.HideAndDontSave;
					graphObject.graph = JsonUtility.FromJson(textGraph, graphType) as IGraph;
				}

				selectedGuid = assetGuid;
				if (graphObject.graph == null) graphObject.graph = Activator.CreateInstance(graphType) as IGraph;
				graphObject.graph.OnEnable();
				graphObject.graph.ValidateGraph();
				graphEditorView = new GraphEditorView(this, m_GraphObject.graph as AbstractNodeGraph)
				{
					persistenceKey = selectedGuid,
					assetName = asset.name.Split('/').Last()
				};
				graphEditorView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

				titleContent = new GUIContent(asset.name.Split('/').Last());

				Repaint();
			}
			catch (Exception)
			{
				m_HasError = true;
				m_GraphEditorView = null;
				graphObject = null;
				throw;
			}
		}

		void Update()
		{
			if (m_HasError)
				return;

			try
			{
				if (graphObject == null && selectedGuid != null)
				{
					var guid = selectedGuid;
					selectedGuid = null;
					Initialize(guid);
				}

				if (graphObject == null)
				{
					Close();
					return;
				}

				var materialGraph = graphObject.graph as AbstractNodeGraph;
				if (materialGraph == null)
					return;

				if (graphEditorView == null)
				{
					var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(selectedGuid));
					graphEditorView = new GraphEditorView(this, materialGraph)
					{
						persistenceKey = selectedGuid,
						assetName = asset.name.Split('/').Last()
					};
				}

				graphEditorView.HandleGraphChanges();
				graphObject.graph.ClearChanges();
			}
			catch (Exception e)
			{
				m_HasError = true;
				m_GraphEditorView = null;
				graphObject = null;
				Debug.LogException(e);
				throw;
			}
		}

		void UpdateNodeGraphOnDisk(string path)
		{
			var graph = graphObject.graph as INodeGraph;
			if (graph == null)
				return;

			UpdateNodeGraphOnDisk(path, graph);
		}

		static void UpdateNodeGraphOnDisk(string path, INodeGraph graph)
		{
			File.WriteAllText(path, EditorJsonUtility.ToJson(graph, true));
			AssetDatabase.ImportAsset(path);
		}

		private void Rebuild()
		{
			if (graphObject != null && graphObject.graph != null)
			{
				
			}
		}

		void OnGeometryChanged(GeometryChangedEvent evt)
		{
			graphEditorView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			graphEditorView.graphView.FrameAll();
		}
	}
}
