#if __IOS__ || __MACOS__
using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using WebKit;

namespace P42.Uno.HtmlWebViewExtensions
{
    /// <summary>
    /// HTML to PDF service.
    /// </summary>
    public class NativeToPngService : INativeToPngService
    {
        const string LocalStorageFolderName = "P42.Uno.HtmlWevViewExtensions.ToPngService";

        public static string FolderPath()
        {
            P42.Utils.DirectoryExtensions.AssureExists(P42.Utils.Environment.TemporaryStoragePath);
            var root = Path.Combine(P42.Utils.Environment.TemporaryStoragePath, LocalStorageFolderName);
            P42.Utils.DirectoryExtensions.AssureExists(root);
            return root;
        }

        static NativeToPngService()
        {
            var path = FolderPath();
            Directory.Delete(path, true);
        }

        public bool IsAvailable => UIPrintInteractionController.PrintingAvailable && NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0));

        public async Task<ToFileResult> ToPngAsync(string html, string fileName, int width)
        {
            if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            {
                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                const string jScript = @"var meta = document.createElement('meta'); meta.setAttribute('name', 'viewport'); meta.setAttribute('content', 'width=device-width'); document.getElementsByTagName('head')[0].appendChild(meta);";
                var wkUScript = new WKUserScript((NSString)jScript, WKUserScriptInjectionTime.AtDocumentEnd, true);
                using (var wkUController = new WKUserContentController())
                {
                    wkUController.AddUserScript(wkUScript);
                    var configuration = new WKWebViewConfiguration
                    {
                        UserContentController = wkUController
                    };
                    using (var webView = new WKWebView(new CGRect(0, 0, width, width), configuration)
                    {
                        UserInteractionEnabled = false,
                        BackgroundColor = UIColor.White
                    })
                    {
                        webView.NavigationDelegate = new WKNavigationCompleteCallback(fileName, new PageSize { Width = width }, null, taskCompletionSource, NavigationCompleteAsync);
                        webView.LoadHtmlString(html, null);
                        return await taskCompletionSource.Task;
                    }
                }
            }
            return await Task.FromResult(new ToFileResult(true, "PNG output not available prior to iOS 11"));
        }

        public async Task<ToFileResult> ToPngAsync(Windows.UI.Xaml.Controls.WebView unoWebView, string fileName, int width)
        {
            if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            {
                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                if (unoWebView.GetNativeWebView() is Windows.UI.Xaml.Controls.NativeWebView wkWebView)
                {
                    wkWebView.BackgroundColor = UIColor.White;
                    wkWebView.UserInteractionEnabled = false;
                    wkWebView.NavigationDelegate = new WKNavigationCompleteCallback(fileName, new PageSize { Width = wkWebView.Bounds.Width, Height = wkWebView.Bounds.Height }, null, taskCompletionSource, NavigationCompleteAsync);
                    return await taskCompletionSource.Task;
                }
                return await Task.FromResult(new ToFileResult(true, "Could not get NativeWebView for Uno WebView"));
            }
            return await Task.FromResult(new ToFileResult(true, "PNG output not available prior to iOS 11"));
        }


        static async Task NavigationCompleteAsync(WKWebView webView, string filename, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            try
            {
                var widthString = await webView.EvaluateJavaScriptAsync("document.documentElement.offsetWidth");
                var width = double.Parse(widthString.ToString());

                var heightString = await webView.EvaluateJavaScriptAsync("document.documentElement.offsetHeight");
                var height = double.Parse(heightString.ToString());

                if (width < 1 || height < 1)
                {
                    taskCompletionSource.SetResult(new ToFileResult(true, "WebView has zero width or height"));
                    return;
                }

                webView.ClipsToBounds = false;
                webView.ScrollView.ClipsToBounds = false;

                var bounds = webView.Bounds;
                webView.Bounds = new CGRect(0, 0, (nfloat)width, (nfloat)height);

                var scale = pageSize.Width / width;


                var snapshotConfig = new WKSnapshotConfiguration
                {
                    SnapshotWidth = pageSize.Width / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density
                };

                var image = await webView.TakeSnapshotAsync(snapshotConfig);

                if (image.AsPNG() is NSData data)
                {
                    var path = Path.Combine(NativeToPngService.FolderPath(), filename + ".png");
                    File.WriteAllBytes(path, data.ToArray());
                    taskCompletionSource.SetResult(new ToFileResult(false, path));
                    return;
                }
                webView.Bounds = bounds;
                taskCompletionSource.SetResult(new ToFileResult(true, "No data returned."));
            }
            catch (Exception e)
            {

                taskCompletionSource.SetResult(new ToFileResult(true, "Exception: " + e.Message + (e.InnerException != null
                    ? "Inner exception: " + e.InnerException.Message
                    : null)));
            }
            finally
            {
                webView.Dispose();
            }

        }
    }


    class WKNavigationCompleteCallback : WKNavigationDelegate
    {
        public bool Completed { get; private set; }

        int loadCount;
        readonly string _filename;
        readonly PageSize _pageSize;
        readonly PageMargin _margin;
        readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
        readonly Func<WKWebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> _action;

        public WKNavigationCompleteCallback(string fileName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource, Func<WKWebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> action)
        {
            _filename = fileName;
            _pageSize = pageSize;
            _margin = margin;
            _taskCompletionSource = taskCompletionSource;
            _action = action;
        }

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            loadCount++;
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            loadCount--;
            P42.Utils.Timer.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                if (loadCount <= 0)
                {
                    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _action?.Invoke(webView, _filename, _pageSize, _margin, _taskCompletionSource);
                    });
                    return false;
                }
                return true;
            });

        }
    }
}
#endif