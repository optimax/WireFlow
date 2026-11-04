using System;

namespace OptimaxTools.Dock.Themes
{
	/// <inheritdoc/>
	public class GenericTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri("/OptimaxTools.Dock;component/Themes/generic.xaml", UriKind.Relative);
		}
	}
}