using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Optimax;

namespace WireFlow
{
    public class EditableListBox: ListBox
    {
        public bool IsEditing { get; set; }

        private FrameworkElement selectedContainer;
        private int selectedIndex;

        private Action<string, string> editSuccess;
        private TextEditingAdorner adorner;

        private const int FUDGE_PIXELS = 2;


        public void BeginEdit(Action<string, string> onSuccess)
        {
            if (IsEditing)
                return;
            if (SelectedIndex < 0)
                return;
            var item = Items[SelectedIndex];
            if (item == null)
                return;
            selectedContainer = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as FrameworkElement;
            if (selectedContainer == null)
                return;
            selectedIndex = SelectedIndex;

            var aLayer = AdornerLayer.GetAdornerLayer(selectedContainer);
            adorner = new TextEditingAdorner(selectedContainer, this,
                new Rect(Padding.Left, 0, this.ActualWidth - Padding.Right - Padding.Left - FUDGE_PIXELS, selectedContainer.DesiredSize.Height),
                (string)item);

            adorner.CancelEditEvent += (a) => CancelEdit();
            adorner.CommitEditEvent += (a, oldName, newName) => CommitEdit(oldName, newName);
            aLayer.Add(adorner);
            //    adorner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            //           (MethodInvoker)delegate ()
            //           {
            //               elem.Focus();
            //               Keyboard.Focus(elem);
            //           });
            //}, element);
            adorner.Focus();

            editSuccess = onSuccess;

            IsEditing = true;
        }


        public void CancelEdit()
        {
            if (!IsEditing)
                return;

            var aLayer = AdornerLayer.GetAdornerLayer(selectedContainer);
            //var adorns = aLayer?.GetAdorners(selectedContainer);
            //var adorner = adorns?.FirstOrDefault();
            if (adorner != null)
                aLayer?.Remove(adorner);

            IsEditing = false;
            selectedContainer = null;


            SelectedIndex = selectedIndex;
            this.Focus();

        }



        public void CommitEdit(string oldValue, string newValue)
        {
            if (!IsEditing)
                return;
            editSuccess?.Invoke(oldValue, newValue);
            CancelEdit();
        }


        protected override void OnLostFocus(RoutedEventArgs e)
        {
            CancelEdit();
            base.OnLostFocus(e);

        }


        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            CancelEdit();
            base.OnSelectionChanged(e);
        }
    }
}
