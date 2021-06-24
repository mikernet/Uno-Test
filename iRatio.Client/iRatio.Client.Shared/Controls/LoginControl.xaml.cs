using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UnoTest.Client.Controls
{
    public sealed partial class LoginControl : UserControl
    {
        public string Email => EmailBox.Text.Trim();

        public string Password => PasswordBox.Password;

        public LoginControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (settings.Values.TryGetValue("email", out object emailValue) && emailValue is string email) {
                EmailBox.Text = email;
                PasswordBox.Focus(FocusState.Programmatic);
            }
            else {
                EmailBox.Focus(FocusState.Programmatic);
            }
        }

        public async Task<bool> ValidateAsync()
        {
            if (!Email.Contains('@') || !Email.Contains('.') || Email.Contains(' ') || Email.Length < 7) {
                await new MessageDialog("Invalid email address.").ShowAsync();
            }
            else if (Password.Length == 0) {
                await new MessageDialog("Please enter a password.").ShowAsync();
            }
            else {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                settings.Values["email"] = Email;
                return true;
            }

            return false;
        }
    }
}
