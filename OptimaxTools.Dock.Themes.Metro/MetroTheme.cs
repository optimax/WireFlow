using System;

namespace OptimaxTools.Dock.Themes;

/// <inheritdoc/>
public class MetroTheme : Theme
{
	/// <inheritdoc/>
	public override Uri GetResourceUri()
	{
		return new Uri(
			"/OptimaxTools.Dock.Themes.Metro;component/Theme.xaml",
			UriKind.Relative);
	}
}