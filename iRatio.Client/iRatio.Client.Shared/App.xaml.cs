using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web;
using UnoTest.Client.Roots;
using Microsoft.Extensions.Logging;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xaml = Windows.UI.Xaml;

namespace UnoTest.Client
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private bool _initialized = false;

        public static Frame RootFrame { get; private set; }

        public App()
        {
            ConfigureFilters(Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);

            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            #if __IOS__ || __ANDROID__
            Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ForegroundColor = Windows.UI.Colors.Black;
            #endif

            if (!_initialized) {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, args) =>
                {
                    args.Handled = RootFrame.Content is MainRoot mainRoot && mainRoot.GoBack();
                };

                RootFrame = new Frame();

                RootFrame.Navigated += (s, e) => {
                    RootFrame.BackStack.Clear();
                    RootFrame.ForwardStack.Clear();
                };

                RootFrame.NavigationFailed += (s, e) => throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    // TODO: Load state from previously suspended application
                }

                #if __IOS__
                Uno.UI.FeatureConfiguration.DatePicker.UseLegacyStyle = true;
                #endif

                _initialized = true;
            }

            #if DEBUG

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }

            #endif

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Xaml.Window.Current.Content is not Frame) {
                Xaml.Window.Current.Content = RootFrame;
            }

            if (!e.PrelaunchActivated) {
                if (RootFrame.Content == null) {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    RootFrame.Navigate(typeof(LoginRoot), e.Arguments);
                }

                // Ensure the current window is active
                Xaml.Window.Current.Activate();
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private static void ConfigureFilters(ILoggerFactory factory)
        {
            factory.WithFilter(new FilterLoggerSettings
            {
                { "Uno", LogLevel.Warning },
                { "Windows", LogLevel.Warning },

                // Debug JS interop
                // { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

                // Generic Xaml events
                // { "Windows.UI.Xaml", LogLevel.Debug },
                // { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
                // { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
                // { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

                // Layouter specific messages
                // { "Windows.UI.Xaml.Controls", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
                // { "Windows.Storage", LogLevel.Debug },

                // Binding related messages
                // { "Windows.UI.Xaml.Data", LogLevel.Debug },

                // DependencyObject memory references tracking
                // { "ReferenceHolder", LogLevel.Debug },

                // ListView-related messages
                // { "Windows.UI.Xaml.Controls.ListViewBase", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.ListView", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.GridView", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.VirtualizingPanelLayout", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.NativeListViewBase", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.ListViewBaseSource", LogLevel.Debug }, //iOS
                // { "Windows.UI.Xaml.Controls.ListViewBaseInternalContainer", LogLevel.Debug }, //iOS
                // { "Windows.UI.Xaml.Controls.NativeListViewBaseAdapter", LogLevel.Debug }, //Android
                // { "Windows.UI.Xaml.Controls.BufferViewCache", LogLevel.Debug }, //Android
                // { "Windows.UI.Xaml.Controls.VirtualizingPanelGenerator", LogLevel.Debug }, //WASM
            });

            #if DEBUG

            factory.AddConsole(LogLevel.Warning);

            #else

            //factory.AddConsole(LogLevel.Warning);

            #endif
        }
    }
}
