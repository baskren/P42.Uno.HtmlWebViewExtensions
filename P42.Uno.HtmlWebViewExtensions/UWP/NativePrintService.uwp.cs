﻿#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace P42.Uno.HtmlWebViewExtensions
{
	/// <summary>
	/// Web view extensions service.
	/// </summary>
	public class NativePrintService : INativePrintService
	{
		/// <summary>
		/// Cans the print.
		/// </summary>
		/// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
		public bool IsAvailable()
		{
			return PrintManager.IsSupported();
		}

		TaskCompletionSource<bool> _printingTCS;
		public async Task PrintAsync(WebView webView, string jobName)
		{
			if (_printingTCS != null)
				return;

			_printingTCS = new TaskCompletionSource<bool>();
			MainThread.BeginInvokeOnMainThread(async () =>
			{

				if (string.IsNullOrWhiteSpace(jobName))
					jobName = Windows.ApplicationModel.Package.Current.DisplayName;
				WebViewPrintHelper printHelper = null;
				if (webView.Source is Uri Uri && !string.IsNullOrWhiteSpace(Uri.AbsolutePath))
                {
					printHelper = new WebViewPrintHelper(Uri, jobName);
                }
				else if (webView is Windows.UI.Xaml.Controls.WebView nativeWebView)
				{
					printHelper = new WebViewPrintHelper(webView, jobName);
				}
				if (printHelper != null)
				{
					await Task.Delay(50);
					printHelper.RegisterForPrinting();
					await printHelper.InitAsync();
				}
				_printingTCS?.TrySetResult(true);
			});
			await _printingTCS.Task;

			_printingTCS = null;
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

        static void OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (sender is WebView webView && webView.Tag is TaskCompletionSource<bool> tcs)
            {
				tcs.SetResult(false);
				//await P42.Uno.Controls.Toast.CreateAsync("Print Service Error", "WebView failed to navigate to provided string.  Please try again.\n\nWebErrorStatus: " + e.WebErrorStatus);
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