using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using UnoTest.Client.Data;

namespace UnoTest.Client.Dialogs
{
    public sealed partial class TestDialog : ContentDialog
    {
        public TestDialogInfo Data { get; private set; }

        public TestDialog(TestDialogInfo data)
        {
            Data = data;
            InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            IsEnabled = false;
            //string message;

            //try {
            //    message = await App.ServiceClient.SubmitReportAsync(Report);
            //}
            //catch (Exception ex) {
            //    IsEnabled = true;
            //    _ = await new MessageDialog(ex.Message, "ERROR").ShowAsync();
            //    return;
            //}

            //_ = await new MessageDialog(message, "THANK YOU").ShowAsync();

            _ = await new MessageDialog("Success", "THANK YOU").ShowAsync();
            Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
