using System.Windows;
using System.Windows.Controls;
using MaxServices.Core.Controls;
using MaxServices.Core.Models;
using MaxServices.Core.Themes;

namespace WireFlow.Views
{
    /// <summary>
    /// Chooses where a text lies in its frame: the 9-point position chooser (MaxServices.Core's PagePositionControl) in a
    /// "Text position" group, as PDFTools' Add Margins and Resize Pages dialogs show it, with OK and Cancel under it.
    /// </summary>
    public sealed class TextPositionDialog : Window
    {
        private readonly PagePositionControl chooser;


        public TextPositionDialog(string title, PageContentPosition initial)
        {
            Title = title;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
            SetResourceReference(BackgroundProperty, "NormalColorBrush");
            SetResourceReference(ForegroundProperty, "LightLightColorBrush");
            SourceInitialized += (s, e) => AppTheme.ApplyTitleBar(this);

            chooser = new PagePositionControl
            {
                SelectedPosition = initial,
                ShowLabel = true,
                LabelPlacement = Dock.Right,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4)
            };
            // Wide enough for the longest label, so that the window keeps its size as the label changes
            var group = new GroupBox { Header = "Text position", Content = chooser, MinWidth = 240, Margin = new Thickness(0, 0, 0, 16) };

            var ok = new Button { Content = "OK", Width = 88, Height = 30, IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (s, e) => DialogResult = true;
            var cancel = new Button { Content = "Cancel", Width = 88, Height = 30, IsCancel = true };
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Children = { ok, cancel } };

            Content = new StackPanel { Margin = new Thickness(16), Children = { group, buttons } };
            Loaded += (s, e) => chooser.Focus();
        }


        public PageContentPosition SelectedPosition => chooser.SelectedPosition;


        /// <summary>Shows the dialog over its owner; the position chosen, or null if it was cancelled.</summary>
        public static PageContentPosition? Show(Window owner, string title, PageContentPosition initial)
        {
            var dialog = new TextPositionDialog(title, initial) { Owner = owner };
            return dialog.ShowDialog() == true ? dialog.SelectedPosition : (PageContentPosition?)null;
        }
    }
}
