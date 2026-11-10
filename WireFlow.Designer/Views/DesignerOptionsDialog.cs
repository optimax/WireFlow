using MaxServices.Core.Themes;
using OptimaxTools.Options;

namespace WireFlow.Views
{
    /// <summary>
    /// Tools > Options: the Options dialog of MarkdownTools' and PDFTools' programs (OptimaxTools.Options), in the Designer's
    /// theme, with its groups of options on the left and the chosen group's on the right. It changes nothing itself: OK sets
    /// its results, which the main window applies and saves.
    /// </summary>
    internal sealed class DesignerOptionsDialog : OptionsDialog
    {
        /// <summary>Whether the project open when the Designer closed opens again when it starts, as OK left it.</summary>
        public bool ReopenPreviousProject { get; private set; }

        /// <summary>Whether the Select tool is chosen again after each object drawn, as OK left it.</summary>
        public bool SelectAfterDrawing { get; private set; }


        public DesignerOptionsDialog(ConfigSettings settings)
        {
            ReopenPreviousProject = settings.ReopenPreviousProject;
            SelectAfterDrawing = settings.SelectAfterDrawing;
            // In the theme's colors, as the color dialog
            SetResourceReference(BackgroundProperty, "NormalColorBrush");
            SetResourceReference(ForegroundProperty, "LightLightColorBrush");
            SourceInitialized += (s, e) => AppTheme.ApplyTitleBar(this);

            var files = Page("Files");
            Check(files, "ReopenPreviousProject", "Reopen the previous project at startup", settings.ReopenPreviousProject).ToolTip =
                "The project open when the Designer was last closed opens again when it starts.";
            var drawing = Page("Drawing");
            Check(drawing, "SelectAfterDrawing", "Return to the Select tool after drawing an object", settings.SelectAfterDrawing).ToolTip =
                "Once an object is drawn, the tool is Select again, as Esc makes it; otherwise the tool stays for drawing more.";
            Finish();
        }


        protected override void Apply()
        {
            ReopenPreviousProject = IsChecked("ReopenPreviousProject");
            SelectAfterDrawing = IsChecked("SelectAfterDrawing");
        }
    }
}
