using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.IO;


namespace Blank
{
    public class WebView2Service
    {
        private static WebView2Service _instance;
        private CoreWebView2Environment _environment;
        private TaskCompletionSource<CoreWebView2Environment> _initializationTask;

        public static WebView2Service Instance => _instance ??= new WebView2Service();

        private WebView2Service()
        {
            _initializationTask = new TaskCompletionSource<CoreWebView2Environment>();
            InitializeEnvironmentAsync();
        }

        private async void InitializeEnvironmentAsync()
        {
            try
            {
                var environmentOptions = new CoreWebView2EnvironmentOptions();
                environmentOptions.AdditionalBrowserArguments = "--disable-features=msSmartScreenProtection";

                _environment = await CoreWebView2Environment.CreateWithOptionsAsync("", "", environmentOptions);
                _initializationTask.SetResult(_environment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebView2 Environment Initialization Error: {ex}");
                _initializationTask.SetException(ex);
            }
        }

        public async Task InitializeWebViewAsync(WebView2 webView)
        {
            try
            {
                var env = await _initializationTask.Task;
                await webView.EnsureCoreWebView2Async(env);

                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string htmlFolder = Path.Combine(appDirectory, "Assets/HtmlFiles");

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "local.app",
                    htmlFolder,
                    CoreWebView2HostResourceAccessKind.Allow);

                webView.CoreWebView2.Navigate("http://local.app/index.html");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebView2 Initialization Error: {ex}");
                throw;
            }
        }
    }
}

