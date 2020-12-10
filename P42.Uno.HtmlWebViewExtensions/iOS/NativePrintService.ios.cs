﻿#if __IOS__
using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WebKit;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{

    /// <summary>
    /// Web view extensions service.
    /// </summary>
    public class NativePrintService : UIPrintInteractionControllerDelegate, INativePrintService
    {

        /// <summary>
        /// Print the specified viewToPrint and jobName.
        /// </summary>
        /// <param name="unoWebView">View to print.</param>
        /// <param name="jobName">Job name.</param>
        public async Task PrintAsync(WebView unoWebView, string jobName)
        {
            //var effectApplied = viewToPrint.Effects.Any(e => e is Forms9Patch.WebViewPrintEffect);
            //var actualSource = viewToPrint.ActualSource() as WebViewSource;
            var printInfo = UIPrintInfo.PrintInfo;
            printInfo.JobName = jobName;
            printInfo.Duplex = UIPrintInfoDuplex.None;
            printInfo.OutputType = UIPrintInfoOutputType.General;

            var printController = UIPrintInteractionController.SharedPrintController;
            printController.ShowsPageRange = true;
            printController.ShowsPaperSelectionForLoadedPapers = true;
            printController.PrintInfo = printInfo;
            printController.Delegate = this;

            if (unoWebView.GetNativeWebView() is Windows.UI.Xaml.Controls.NativeWebView wkWebView)
            {
                var html = await wkWebView.EvaluateJavaScriptAsync("document.documentElement.outerHTML") as NSString;
                printController.PrintFormatter = new UIMarkupTextPrintFormatter(html);
                printController.Present(true, (printInteractionController, completed, error) =>
                {
                    System.Diagnostics.Debug.WriteLine(GetType() + "." + P42.Utils.ReflectionExtensions.CallerMemberName() + ": PRESENTED completed[" + completed + "] error[" + error + "]");
                });
            }

        }

        /// <summary>
        /// Cans the print.
        /// </summary>
        /// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
        public bool CanPrint()
        {
            return UIPrintInteractionController.PrintingAvailable;
        }

        public async Task PrintAsync(string html, string jobName)
        {
            var webView = new WebView();
            webView.NavigationCompleted += OnNavigationComplete;
            webView.NavigationFailed += OnNavigationFailed;

            var tcs = new TaskCompletionSource<bool>();
            webView.Tag = tcs;
            webView.NavigateToString(html);
            if (await tcs.Task)
                await PrintAsync(webView, jobName);
        }

        static async void OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (sender is WebView webView && webView.Tag is TaskCompletionSource<bool> tcs)
            {
                tcs.SetResult(false);
                await P42.Uno.Controls.Toast.CreateAsync("Print Service Error", "WebView failed to navigate to provided string.  Please try again.\n\nWebErrorStatus: " + e.WebErrorStatus);
                return;
            }
            throw new Exception("Cannot locate WebView or TaskCompletionSource for WebView.OnNavigationFailed");
        }

        static void OnNavigationComplete(WebView webView, WebViewNavigationCompletedEventArgs args)
        {
            if (webView.Tag is TaskCompletionSource<bool> tcs)
            {
                tcs.SetResult(true);
                return;
            }
            throw new Exception("Cannot locate TaskCompletionSource for WebView.NavigationToString");
        }
    }
}
#endif