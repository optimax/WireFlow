using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using OptimaxTools.Wpf;

namespace WireFlow
{
    /// <summary>
    /// The editor of a page's or layout's Layout (its node's Master) in Properties: a box to type a layout's name or ID in
    /// (kept as typed), with a drop-down list of the project's layouts by name to choose one from. The list holds layouts
    /// only: none is the box emptied. A value is set when chosen, or, typed, with Enter or as the box loses the focus; Esc
    /// takes the typing back.
    /// </summary>
    public sealed class LayoutPropertyEditor : PropertyEditor
    {
        /// <summary>The names of the project's layouts, which the window gives.</summary>
        public static Func<IList<string>> LayoutNames { get; set; }


        public static void Register()
        {
            PropertyEditors.Register(typeof(string), typeof(NodeProperties), "Layout", new LayoutPropertyEditor());
        }


        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var names = (LayoutNames?.Invoke() ?? new List<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            var combo = new ComboBox { IsEditable = true, IsTextSearchEnabled = false, ItemsSource = names, IsEnabled = !item.IsReadOnly };
            combo.SetBinding(ComboBox.TextProperty, new Binding(nameof(PropertyItem.Text))
            {
                Source = item,
                Mode = item.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
            if (!item.IsReadOnly)
            {
                // A layout chosen from the list is set as the list closes
                combo.DropDownClosed += (sender, e) => Commit(combo);
                combo.PreviewKeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        Commit(combo);
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Escape && !combo.IsDropDownOpen)
                    {
                        combo.GetBindingExpression(ComboBox.TextProperty)?.UpdateTarget();
                        e.Handled = true;
                    }
                };
                combo.LostKeyboardFocus += (sender, e) =>
                {
                    if (!combo.IsKeyboardFocusWithin)
                        Commit(combo);
                };
            }
            return combo;
        }


        // Sets the property from the text, then shows its value: the layout set, or the one before if it was refused
        private static void Commit(ComboBox combo)
        {
            var binding = combo.GetBindingExpression(ComboBox.TextProperty);
            if (binding == null)
                return;
            if (binding.IsDirty)
                binding.UpdateSource();
            binding.UpdateTarget();
        }
    }
}
