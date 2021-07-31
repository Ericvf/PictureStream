using PictureStream.App.Framework;
using PictureStream.App.ViewModels;
using PictureStream.App.Views;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace PictureStream.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        internal static ResourceLoader ResourceLoader = new ResourceLoader();

        private static ManageVM manageVM;
        public static ManageVM ManageVM
        {
            get
            {
                if (manageVM == null)
                    manageVM = new ManageVM();

                return manageVM;
            }
        }

        internal static StorageFolder LocalStorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        TaskPanePopup aboutPopup;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += App_Resuming;
        }

        void App_Resuming(object sender, object e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += onSettingsCommandsRequested;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += onSettingsCommandsRequested;
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(Manager), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= onSettingsCommandsRequested;
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        private void onSettingsCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Clear();

            //SettingsCommand about = new SettingsCommand("about", App.ResourceLoader.GetString(@"SettingsCommandAboutTitle"), (uiCommand) =>
            //{
            //    if (aboutPopup == null)
            //        aboutPopup = new TaskPanePopup(new Button() { Width = 300, Height=200 } );
            //    aboutPopup.Show();
            //});

            SettingsCommand serverapp = new SettingsCommand("serverapp", App.ResourceLoader.GetString(@"SettingsCommandServerApp"), async (uiCommand) =>
            {
                var uri = new Uri("http://www.appbyfex.com/Home/PictureStream/");
                await Launcher.LaunchUriAsync(uri);
            });

            SettingsCommand privacy = new SettingsCommand("privacy", App.ResourceLoader.GetString(@"SettingsCommandAboutPrivacyTitle"), async (uiCommand) =>
            {
                var dialog = new MessageDialog(App.ResourceLoader.GetString(@"PrivacyPolicyText"),
                    App.ResourceLoader.GetString(@"SettingsCommandAboutPrivacyTitle"));

                await dialog.ShowAsync();
            });

            args.Request.ApplicationCommands.Add(serverapp);
            args.Request.ApplicationCommands.Add(privacy);
        }

    }
}
