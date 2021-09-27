using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
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
using UnoTest.Client.Dialogs;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UnoTest.Client.Roots
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginRoot : Page
    {
        public LoginRoot()
        {
            InitializeComponent();
            SignUp.Data = new UserInfo();
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            if (!await Login.ValidateAsync())
                return;

            Frame.Navigate(typeof(MainRoot));
        }

        private async void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            if (Login.Email.Length == 0) {
                _ = await new MessageDialog("Please enter your email address.").ShowAsync();
                return;
            }
        }

        private async void OnInvisibleButtonClick(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("Invisible button clicked!").ShowAsync();
        }
        private async void OnSignUpClick(object sender, RoutedEventArgs e)
        {
            if (!await SignUp.ValidateAsync())
                return;

            await new MessageDialog("Success").ShowAsync();
        }
        private async void OnTestDialogClick(object sender, RoutedEventArgs e)
        {
            var testDialog = new TestDialog(new TestDialogInfo());
            _ = await testDialog.ShowAsync();
        }
    }
}
