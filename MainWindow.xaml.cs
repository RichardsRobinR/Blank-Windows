using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.WebUI;

namespace Blank
{
    public sealed partial class MainWindow : Window
    {

        private string lastReceivedMessage;


        public MainWindow()
        {
            this.InitializeComponent();




            ExtendsContentIntoTitleBar = true;  // enable custom titlebar
            SetTitleBar(AppTitleBar);      // set user ui element as titlebar

            

            InitializeAsync();
        }


        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            Debug.WriteLine($"Received message: {message}");
            lastReceivedMessage = message;


        }

        private async void Save_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

            FileSavePicker savePicker = new FileSavePicker();

            // See the sample code below for how to make the window accessible from the App class.
            //var window = App.MainWindow;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);



            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                await FileIO.WriteTextAsync(file, lastReceivedMessage);

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                //if (status == FileUpdateStatus.Complete)
                //{
                //    SaveFileOutputTextBlock.Text = "File " + file.Name + " was saved.";
                //}
                //else if (status == FileUpdateStatus.CompleteAndRenamed)
                //{
                //    SaveFileOutputTextBlock.Text = "File " + file.Name + " was renamed and saved.";
                //}
                //else
                //{
                //    SaveFileOutputTextBlock.Text = "File " + file.Name + " couldn't be saved.";
                //}


            }

        }


       

        async void InitializeAsync()
        {
            try
            {
                //// Initialize WebView2 with explicit user data folder
                //var env = await CoreWebView2Environment.CreateAsync(

                //    options: new CoreWebView2EnvironmentOptions
                //    {
                //        AdditionalBrowserArguments = "--disable-web-security"
                //    });


                CoreWebView2EnvironmentOptions environmentOptions = new CoreWebView2EnvironmentOptions();
                environmentOptions.AdditionalBrowserArguments = "--disable-features=msSmartScreenProtection";

                var env = await CoreWebView2Environment.CreateWithOptionsAsync("", "", environmentOptions);




                await MyWebView.EnsureCoreWebView2Async(env);
                MyWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                MyWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                //MyWebView.DefaultBackgroundColor = Colors.Transparent;

                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string htmlFolder = Path.Combine(appDirectory, "Assets/HtmlFiles");
                Debug.WriteLine(htmlFolder);

               

                // Map virtual domain to your HTML folder
                MyWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "local.app", // Virtual hostname
                    htmlFolder,  // Physical folder path
                    CoreWebView2HostResourceAccessKind.Allow);

                // Navigate using virtual host URL
                MyWebView.CoreWebView2.Navigate("http://local.app/index.html");

                MyWebView.Opacity = 0;
                MyWebView.Visibility = Visibility.Visible;

                var fadeAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(500))
                };

                var storyboard = new Storyboard();
                Storyboard.SetTarget(fadeAnimation, MyWebView);
                Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                storyboard.Children.Add(fadeAnimation);
                storyboard.Begin();

                // Setup event handlers
                MyWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                MyWebView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
                
                //MyWebView.CoreWebView2.Ini
                //MyWebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebView2 Initialization Error: {ex}");
            }
        }

        private void CoreWebView2_ContextMenuRequested(CoreWebView2 sender, CoreWebView2ContextMenuRequestedEventArgs args)
        {
            args.Handled = true;
        }

       
    }
}