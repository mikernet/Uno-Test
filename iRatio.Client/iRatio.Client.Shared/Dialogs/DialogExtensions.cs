using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UnoTest.Client.Dialogs
{
    public static class DialogExtensions
    {
        public static ContentDialog WithAppStyle(this ContentDialog dialog)
        {
            dialog.Style = (Style)Application.Current.Resources["DialogStyle"];
            return dialog;
        }
    }
}
