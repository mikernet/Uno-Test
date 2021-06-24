using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace UnoTest.Client.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BusinessPage : Page
    {
        public long BusinessId { get; private set; }

        public DTO.Business.Business Business { get; private set; }

        public bool IsManager => Business?.CurrentUserRole >= DTO.BusinessRole.Manager;

        public bool IsAccountManager => Business?.CurrentUserRole == DTO.BusinessRole.AccountManager;

        private MainRoot MainRoot => (MainRoot)App.RootFrame.Content;

        public BusinessPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BusinessId = (long)e.Parameter;
            ReloadData();
        }

        private async void ReloadData()
        {
            App.RootFrame.IsEnabled = false;

            while (true) {
                try {
                    Business = await App.ServiceClient.GetBusinessAsync(BusinessId);

                    if (!IsAccountManager)
                        MainPivot.Items.Remove(PositionsTab);

                    Bindings.Update();
                    App.RootFrame.IsEnabled = true;
                    return;
                }
                catch (UnauthorizedAccessException) {
                    App.Logout();
                    return;
                }
                catch (Exception ex) {
                    var result = await new ContentDialog {
                        Content = ex.Message,
                        PrimaryButtonText = "Retry",
                        CloseButtonText = "Go Back",
                    }.WithAppStyle().ShowAsync();

                    if (result == ContentDialogResult.None) {
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
            string prompt = "Select a square logo image to fill the space provided. Logos that are not square will be scaled down to fit the area.";
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
                    message = await App.ServiceClient.SetImageAsync(BusinessId, ImageType.BusinessProfile, stream);
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

        private async void OnEmployeeProfileClick(object sender, RoutedEventArgs e)
        {
            var employee = (DTO.Business.Employee)((FrameworkElement)sender).Tag;

            if (employee.UserId == MainRoot.User.Id) {
                Frame.Navigate(typeof(ProfilePage));
                return;
            }

            if (Business.CurrentUserRole == DTO.BusinessRole.Employee) {
                _ = await new MessageDialog("You do not have access to employee profiles.").ShowAsync();
                return;
            }

            bool canManageEmployee = Business.CurrentUserRole == DTO.BusinessRole.AccountManager || Business.CurrentUserRole > employee.Role;

            if (!canManageEmployee) {
                _ = await new MessageDialog("You do not have access to this employee profile.", "ERROR").ShowAsync();
                return;
            }

            if (employee.IsPendingActivation) {
                var result = await new ContentDialog {
                    Title = "SELECT ACTION",
                    Content = "What would you like to do?",
                    PrimaryButtonText = "Retract Request",
                    CloseButtonText = "Cancel",
                }.WithAppStyle().ShowAsync();

                if (result == ContentDialogResult.Primary)
                    DeleteEmployment(employee);
            }
            else {
                var dialog = new EmployeeActionDialog {
                    Title = $"{employee.Name}",
                };

                await dialog.ShowAsync();

                if (dialog.EmployeeAction == EmployeeAction.ViewProfile)
                    Frame.Navigate(typeof(ProfilePage), employee.UserId);
                else if (dialog.EmployeeAction == EmployeeAction.WriteReport)
                    SubmitReport(employee);
                else if (dialog.EmployeeAction == EmployeeAction.Edit)
                    await new EditEmployeeDialog(Business, employee).ShowAsync();
                else
                    return;
            }
        }

        private async void OnAddEmployeeClick(object sender, RoutedEventArgs e)
        {
            if (Business.Positions.Count == 0) {
                _ = await new MessageDialog("An employment position must be added first.").ShowAsync();
                return;
            }

            _ = await new AddEmployeeDialog(Business).ShowAsync();
            ReloadData();
        }

        private async void OnPositionClick(object sender, RoutedEventArgs e)
        {
            var position = (DTO.Business.Position)((FrameworkElement)sender).Tag;

            var result = await new ContentDialog {
                Title = $"{position.Name}",
                Content = "What would you like to do?",
                PrimaryButtonText = "Edit",
                SecondaryButtonText = "Delete",
                CloseButtonText = "Cancel",
            }.WithAppStyle().ShowAsync();

            if (result == ContentDialogResult.None)
                return;

            if (result == ContentDialogResult.Primary) {
                App.RootFrame.IsEnabled = false;
                PositionDialog dialog;

                try {
                    dialog = await PositionDialog.CreateAsync(Business.Id, position);
                }
                catch (Exception ex) {
                    _ = await new MessageDialog(ex.Message, "ERROR").ShowAsync();
                    App.RootFrame.IsEnabled = true;
                    return;
                }

                App.RootFrame.IsEnabled = true;
                _ = await dialog.ShowAsync();
            }
            else if (result == ContentDialogResult.Secondary) {
                App.RootFrame.IsEnabled = false;
                string message;

                try {
                    message = await App.ServiceClient.DeletePosition(Business.Id, position.Id);
                }
                catch (Exception ex) {
                    _ = await new MessageDialog(ex.Message, "ERROR").ShowAsync();
                    App.RootFrame.IsEnabled = true;
                    return;
                }

                App.RootFrame.IsEnabled = true;
                _ = await new MessageDialog(message).ShowAsync();
            }

            ReloadData();
        }

        private async void OnAddPositionClick(object sender, RoutedEventArgs e)
        {
            App.RootFrame.IsEnabled = false;

            PositionDialog dialog;

            try {
                dialog = await PositionDialog.CreateAsync(Business.Id);
            }
            catch (Exception ex) {
                App.RootFrame.IsEnabled = true;
                await new MessageDialog(ex.Message).ShowAsync();
                return;
            }

            App.RootFrame.IsEnabled = true;
            _ = await dialog.ShowAsync();
            ReloadData();
        }

        private async void DeleteEmployment(DTO.Business.Employee employee)
        {
            var result = await new ContentDialog {
                Title = "DELETE EMPLOYEE?",
                Content = $"Are you sure you want to delete employee '{employee.Name}'?",
                PrimaryButtonText = "DELETE",
                CloseButtonText = "Cancel",
            }.WithAppStyle().ShowAsync();

            if (result == ContentDialogResult.None)
                return;

            App.RootFrame.IsEnabled = false;
            string message;

            try {
                message = await App.ServiceClient.DeleteEmploymentAsync(Business.Id, employee.UserId);
            }
            catch (Exception ex) {
                _ = await new MessageDialog(ex.Message, "ERROR").ShowAsync();
                App.RootFrame.IsEnabled = true;
                return;
            }

            App.RootFrame.IsEnabled = true;
            _ = await new MessageDialog(message).ShowAsync();
            ReloadData();
        }

        private async void SubmitReport(DTO.Business.Employee employee)
        {
            var report = new DTO.Report.SubmitReport {
                BusinessId = Business.Id,
                UserId = employee.UserId,
                OverallRating = 5,
                Categories = Business.Positions
                    .First(p => p.Id == employee.PositionId)
                    .ReviewCategories
                    .Split(',')
                    .Select(c => new DTO.Report.CategoryRating {
                        Category = c.Trim(),
                        Rating = 5,
                    }).ToList(),
            };

            _ = await new ReviewDialog(report).ShowAsync();
        }

        private IEnumerable<DTO.Business.Employee> FilterEmployees(IEnumerable<DTO.Business.Employee> employees, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return employees;

            return Filter();

            IEnumerable<DTO.Business.Employee> Filter()
            {
                string[] searchSegments = filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return employees
                    .Where(e => e.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Any(n => searchSegments
                            .Any(s => n.StartsWith(s, StringComparison.InvariantCultureIgnoreCase))));
            }
        }

        private class EmployeeActions
        {
            public string Name { get; set; }

            public Action Execute { get; set; }
        }
    }
}
