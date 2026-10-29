using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GroupExtensions
{
    public static class CanvasEx
    {
        #region

        public static readonly DependencyProperty IsGroupProperty = DependencyProperty.RegisterAttached(
                "IsGroup", typeof(Boolean), typeof(CanvasEx),
                new FrameworkPropertyMetadata(null)
            );


        public static void SetIsGroup(UIElement element, Boolean value)
        {
            element.SetValue(IsGroupProperty, value);
        }


        public static Boolean GetIsGroup(UIElement element)
        {
            return (Boolean)element.GetValue(IsGroupProperty);
        }


        #endregion
    }




}
