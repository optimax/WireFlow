using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MaxServices.Core.Models;
using OptimaxTools.Wpf;
using WireFlow.Views;

namespace WireFlow
{
    /// <summary>
    /// The editor of the TextAlignment of a text or a box: the row shows where the text lies in its frame, named as the
    /// 9-point position chooser of PDFTools names it ("Top-Left", "Center", ...), and its "..." button (or Ctrl+Enter, or a
    /// double click on the value) opens TextPositionDialog, with that chooser. The position is the text's alignments across,
    /// its TextAlignment (and a box's TextHorizontalAlignment), and down, its TextVerticalAlignment; the row follows each of
    /// them as it changes. Justify shows as left.
    /// </summary>
    public class TextPositionPropertyEditor : PropertyEditor
    {
        private static readonly PageContentPosition[,] Positions =
        {
            { PageContentPosition.TopLeft, PageContentPosition.TopCenter, PageContentPosition.TopRight },
            { PageContentPosition.CenterLeft, PageContentPosition.Center, PageContentPosition.CenterRight },
            { PageContentPosition.BottomLeft, PageContentPosition.BottomCenter, PageContentPosition.BottomRight },
        };

        // The properties whose values make the position
        private static readonly string[] AlignmentProperties = { "TextAlignment", "TextVerticalAlignment", "TextHorizontalAlignment" };


        /// <summary>Makes it the editor of the TextAlignment of texts and boxes in the property grid (their nodes' properties).</summary>
        public static void Register()
        {
            PropertyEditors.Register(typeof(FlowTextAlignment), typeof(NodeProperties), "TextAlignment", new TextPositionPropertyEditor());
        }


        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var box = new TextBox { IsReadOnly = true, Text = Describe(item.Component) };
            EventHandler changed = (sender, e) => box.Text = Describe(item.Component);
            PropertyChangedEventHandler refreshed = (sender, e) =>
            {
                if (e.PropertyName == nameof(PropertyItem.Value))
                    changed(sender, e);
            };
            // The alignments are watched while the row shows, as PropertyItem watches its own property
            List<PropertyDescriptor> watched = null;
            box.Loaded += (sender, e) =>
            {
                if (watched != null)
                    return;
                var properties = TypeDescriptor.GetProperties(item.Component);
                watched = AlignmentProperties.Select(name => properties[name]).Where(p => p != null).ToList();
                foreach (var property in watched)
                    property.AddValueChanged(item.Component, changed);
                item.PropertyChanged += refreshed;
                changed(sender, e);
            };
            box.Unloaded += (sender, e) =>
            {
                if (watched == null)
                    return;
                foreach (var property in watched)
                    property.RemoveValueChanged(item.Component, changed);
                item.PropertyChanged -= refreshed;
                watched = null;
            };
            if (!item.IsReadOnly)
                box.MouseDoubleClick += (sender, e) =>
                {
                    ShowDialog(item, Window.GetWindow(box));
                    e.Handled = true;
                };
            return box;
        }


        public override bool HasDialog(PropertyItem item) => true;


        /// <summary>Opens TextPositionDialog at the text's position, and moves the text to the position chosen there.</summary>
        public override void ShowDialog(PropertyItem item, Window owner)
        {
            var initial = PositionOf(item.Component);
            var chosen = TextPositionDialog.Show(owner, item.DisplayName, initial);
            if (chosen != null && chosen.Value != initial)
                Apply(item, chosen.Value);
        }


        /// <summary>The position of a text or a box's text in its frame, from its alignments across and down.</summary>
        public static PageContentPosition PositionOf(object component)
        {
            var properties = TypeDescriptor.GetProperties(component);
            var across = properties["TextAlignment"]?.GetValue(component) is FlowTextAlignment a ? a : FlowTextAlignment.Left;
            var down = properties["TextVerticalAlignment"]?.GetValue(component) is FlowVerticalAlignment v ? v : FlowVerticalAlignment.Top;
            int column = across switch { FlowTextAlignment.Center => 1, FlowTextAlignment.Right => 2, _ => 0 };
            int row = down switch { FlowVerticalAlignment.Center => 1, FlowVerticalAlignment.Bottom => 2, _ => 0 };
            return Positions[row, column];
        }


        /// <summary>The position of a text in its frame as the row shows it: the chooser's name for it, and justified text so.</summary>
        public static string Describe(object component)
        {
            string name = PositionOf(component) switch
            {
                PageContentPosition.TopLeft => "Top-Left",
                PageContentPosition.TopCenter => "Top",
                PageContentPosition.TopRight => "Top-Right",
                PageContentPosition.CenterLeft => "Left",
                PageContentPosition.CenterRight => "Right",
                PageContentPosition.BottomLeft => "Bottom-Left",
                PageContentPosition.BottomCenter => "Bottom",
                PageContentPosition.BottomRight => "Bottom-Right",
                _ => "Center"
            };
            bool justified = TypeDescriptor.GetProperties(component)["TextAlignment"]?.GetValue(component) is FlowTextAlignment.Justify;
            return justified ? name + " (justified)" : name;
        }


        /// <summary>Sets the alignments across and down that put the text at a position of its frame.</summary>
        public static void Apply(PropertyItem item, PageContentPosition position)
        {
            for (int row = 0; row < 3; row++)
                for (int column = 0; column < 3; column++)
                {
                    if (Positions[row, column] != position)
                        continue;
                    var down = row switch { 1 => FlowVerticalAlignment.Center, 2 => FlowVerticalAlignment.Bottom, _ => FlowVerticalAlignment.Top };
                    var block = column switch { 1 => FlowHorizontalAlignment.Center, 2 => FlowHorizontalAlignment.Right, _ => FlowHorizontalAlignment.Left };
                    var across = column switch { 1 => FlowTextAlignment.Center, 2 => FlowTextAlignment.Right, _ => FlowTextAlignment.Left };
                    item.SetComponentProperty("TextVerticalAlignment", down);
                    item.SetComponentProperty("TextHorizontalAlignment", block);
                    item.SetValue(across);
                    return;
                }
        }
    }
}
