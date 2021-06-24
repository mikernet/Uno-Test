using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnoTest.Client.Dialogs;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace UnoTest.Client.Roots
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainRoot : Page
    {
        public MainRoot()
        {
            InitializeComponent();
            SubFrame.Navigated += OnSubFrameNavigated;
            SubFrame.Navigate(typeof(TestRoot));
        }

        public bool GoBack()
        {
            if (!SubFrame.CanGoBack)
                return false;

            SubFrame.GoBack();
            return true;
        }

        private void OnSubFrameNavigated(object sender, NavigationEventArgs e)
        {
            var lastStackItem = SubFrame.BackStack.LastOrDefault();

            if (lastStackItem != null && lastStackItem.SourcePageType == e.SourcePageType && Equals(lastStackItem.Parameter, e.Parameter))
                SubFrame.BackStack.Remove(lastStackItem);
        }

        private async void OnProfileClick(object sender, RoutedEventArgs e)
        {
            MenuToggleButton.IsChecked = false;

            _ = await new MessageDialog("Menu button clicked.").ShowAsync();
        }

        private void OnMenuCollapseAreaPressed(object sender, RoutedEventArgs e)
        {
            MenuToggleButton.IsChecked = false;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            SubFrame.GoBack();
        }
    }
}
