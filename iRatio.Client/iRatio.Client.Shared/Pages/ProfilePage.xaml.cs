using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnoTest.Client.Dialogs;
using UnoTest.Client.Images;
using UnoTest.Client.Roots;
using UnoTest.Client.Service;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UnoTest.Client.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        public long? UserId { get; private set; }

        public DTO.User.User User { get; private set; }

        public bool IsCurrentUser => UserId == null;

        private MainRoot MainRoot => (MainRoot)App.RootFrame.Content;

        private List<ProfileReportFilter> Filters { get; set; }

        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UserId = (long?)e.Parameter;
            ReloadData();
        }

        private async void ReloadData()
        {
            App.RootFrame.IsEnabled = false;

            while (true) {
                try {
                    User = await App.ServiceClient.GetUserAsync(UserId);

                    if (IsCurrentUser)
                        MainRoot.User = User;

                    Filters = new();
                    Filters.Add(new() { Name = "Show All", Filter = r => true });

                    foreach (string business in User.Reports.Select(r => r.BusinessName).Distinct())
                        Filters.Add(new() { Name = business, Filter = r => r.BusinessName == business });

                    foreach (string position in User.Reports.Select(r => r.Position).Distinct())
                        Filters.Add(new() { Name = position, Filter = r => r.Position == position });

                    Bindings.Update();
                    FilterBox.SelectedIndex = 0;
                    App.RootFrame.IsEnabled = true;
                    return;
                }
                catch (UnauthorizedAccessException) when (UserId == null) {
                    App.Logout();
                    return;
                }
                catch (Exception ex) {
                    string closeButtonText = IsCurrentUser ? "Log Out" : "Go Back";

                    var result = await new ContentDialog {
                        Content = ex.Message,
                        PrimaryButtonText = "Retry",
                        CloseButtonText = closeButtonText,
                    }.ShowAsync();

                    if (result == ContentDialogResult.None) {
                        if (IsCurrentUser) {
                            App.Logout();
                            return;
                        }

                        Frame.GoBack();
                        App.RootFrame.IsEnabled = true;
                        return;
                    }
                }
            }
        }

        private async void OnProfileImageClick(object sender, RoutedEventArgs e)
        {
            App.RootFrame.IsEnabled = false;
            string prompt = "Select an image with your face centered in the picture.";
#if __WASM__
            prompt += " Max image size is 5MB.";
#endif
            _ = await new MessageDialog(prompt).ShowAsync();

            ImagePicker picker = new();
            var stream = await picker.GetImageStreamAsync();

            if (stream != null) {
                var uploadingDialog = new UploadingDialog();
                _ = uploadingDialog.ShowAsync();

                string message;

                try {
                    message = await App.ServiceClient.SetImageAsync(UserId, ImageType.UserProfile, stream);
                }
                catch (Exception ex) {
                    uploadingDialog.Hide();
                    _ = await new MessageDialog(ex.Message, "ERROR").ShowAsync();
                    App.RootFrame.IsEnabled = true;
                    return;
                }

                uploadingDialog.Hide();
                App.RootFrame.IsEnabled = true;
                _ = await new MessageDialog(message).ShowAsync();
                ReloadData();
                return;
            }

            App.RootFrame.IsEnabled = true;
        }

        private async void OnEditProfileClick(object sender, RoutedEventArgs e)
        {
            App.RootFrame.IsEnabled = false;

            var data = new DTO.User.UpdateUser {
                FirstName = User.FirstName,
                LastName = User.LastName,
                Description = User.Description,
                DateOfBirth = User.DateOfBirth,
                Phone = User.Phone,
                Address = User.Address,
                CountryCode = User.CountryCode,
                RegionShortName = User.RegionShortName,
                City = User.City,
                PostalCode = User.PostalCode,
                Email = User.Email,
            };

            UpdateUserDialog dialog;

            try {
                dialog = await UpdateUserDialog.CreateAsync(data);
            }
            catch (Exception ex) {
                await new MessageDialog(ex.Message).ShowAsync();
                App.RootFrame.IsEnabled = true;
                return;
            }

            App.RootFrame.IsEnabled = true;
            _ = await dialog.ShowAsync();
            ReloadData();
        }

        private async void OnEmployerClick(object sender, RoutedEventArgs e)
        {
            var employment = (DTO.User.Employment)((FrameworkElement)sender).Tag;

            if (employment.IsPendingActivation) {
                var result = await new ContentDialog {
                    Content = $"Employment with {employment.BusinessName} is pending your confirmation.",
                    PrimaryButtonText = "Accept",
                    SecondaryButtonText = "Reject",
                    CloseButtonText = "Cancel",
                }.ShowAsync();

                if (result == ContentDialogResult.None)
                    return;

                bool accept = result == ContentDialogResult.Primary;
                string message;

                try {
                    message = await App.ServiceClient.RespondToEmploymentAsync(User.Id, employment.BusinessId, accept);
                }
                catch (Exception ex) {
                    _ = await new MessageDialog(ex.Message).ShowAsync();
                    return;
                }

                _ = await new MessageDialog(message).ShowAsync();
            }

            Frame.Navigate(typeof(BusinessPage), employment.BusinessId);
        }

        private IEnumerable<DTO.User.Report> FilterReports(IEnumerable<DTO.User.Report> reports, object filter)
        {
            if (filter is ProfileReportFilter f)
                return reports.Where(r => f.Filter.Invoke(r));

            return reports;
        }

        private string NothingHereIfEmpty(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "Nothing here yet..." : text;
        }

        private string GetBirthdayText(DateTime dateOfBirth)
        {
            return $"{dateOfBirth:MMMM d, yyyy} (age {GetAge(dateOfBirth)})";
        }

        private static int GetAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;

            int a = (today.Year * 100 + today.Month) * 100 + today.Day;
            int b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;

            return (a - b) / 10000;
        }
    }
}
