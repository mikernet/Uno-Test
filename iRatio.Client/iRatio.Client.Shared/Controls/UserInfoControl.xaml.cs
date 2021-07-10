using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

using UnoTest.Client.Data;

namespace UnoTest.Client.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserInfoControl : UserControl
    {
        private UserInfo _data;

        public UserInfo Data {
            get => _data;
            set {
                _data = value;
                DataContext = value;
            }
        }

        public UserInfoControl()
        {
            InitializeComponent();
        }

        public async Task<bool> ValidateAsync()
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(Data, new ValidationContext(Data), validationResults, true);

            if (validationResults.Count > 0) {
                await new MessageDialog(string.Join("\n", validationResults.Select(v => v.ErrorMessage)) + "\nFN: " + FirstName.Text).ShowAsync();
                return false;
            }

            return true;
        }
    }
}
