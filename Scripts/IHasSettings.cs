using UnityEngine.UIElements;

namespace NodeEditor
{
	public interface IHasSettings
	{
		VisualElement CreateSettingsElement();
	}
}