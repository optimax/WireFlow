using System;
using System.Windows;
using System.Windows.Input;

namespace WireFlow
{
    public static class FrameworkElementExtensions
    {
        public static void InvokeWithWaitCursor(this FrameworkElement element, Action action)
        {
            var oldCursor = element.Cursor;
            element.Cursor = Cursors.Wait;
            try
            {
                action();
            }
            finally
            {
                element.Cursor = oldCursor;
            }
        }

    }
}
