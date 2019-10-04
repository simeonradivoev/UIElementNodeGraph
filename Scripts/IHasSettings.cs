using UnityEngine.UIElements;

namespace NodeEditor
{
	/// <summary>
    /// Interface for nodes that have additional settings.
    /// </summary>
	public interface IHasSettings
	{
		/// <summary>
        /// Get a visual element that will be shown when the additional settings icon is pressed.
        /// </summary>
        /// <returns></returns>
		VisualElement CreateSettingsElement();
	}
}