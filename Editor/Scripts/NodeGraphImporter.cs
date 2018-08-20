using UnityEditor.Experimental.AssetImporters;

namespace NodeEditor.Scripts
{
	[ScriptedImporter(1, ShaderGraphExtension)]
	public class NodeGraphImporter : ScriptedImporter
	{
		public const string ShaderGraphExtension = "shadergraph";

		public override void OnImportAsset(AssetImportContext ctx)
		{

		}
	}
}