using UnityEngine.Experimental.UIElements;

namespace NodeEditor
{
	public interface IHasSettings
	{
		VisualElement CreateSettingsElement();
	}
}