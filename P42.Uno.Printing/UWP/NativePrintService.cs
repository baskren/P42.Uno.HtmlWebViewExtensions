using System;
using System.Collections.Generic;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace P42.Uno.Printing
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
		public bool CanPrint()
		{
			return PrintManager.IsSupported();
		}

		public Task PrintAsync(WebView webView, string jobName)
		{
			P42.Utils.Uno.Device.BeginInvokeOnMainThread(async () =>
			{
				if (string.IsNullOrWhiteSpace(jobName))
					jobName = P42.Utils.Uno.AppInfo.Name;
				WebViewPrintHelper printHelper = null;
				var properties = new Dictionary<string, string>
				{
					{ "class", "Forms9Patch.UWP.PrintService" },
					{ "method", nameof(PrintAsync) },
				};
				try
				{
					/*
					if (webView.Source is HtmlWebViewSource htmlSource && !string.IsNullOrWhiteSpace(htmlSource.Html))
					{
						properties["line"] = "47";
						printHelper = new WebViewPrintHelper(htmlSource.Html, htmlSource.BaseUrl, jobName);
					}
					else if (webView.Source is UrlWebViewSource urlSource && !string.IsNullOrWhiteSpace(urlSource.Url))
					{
						properties["line"] = "53";
						printHelper = new WebViewPrintHelper(urlSource.Url, jobName);
					}
					else if (webView is Windows.UI.Xaml.Controls.WebView nativeWebView)
					{
					*/
					properties["line"] = "57";
					printHelper = new WebViewPrintHelper(webView, jobName);
					//}
				}
				catch (Exception e)
				{
					await P42.Uno.Controls.Toast.CreateAsync("Print Service Error", "Could not initiate print WebViewPrintHelper.  Please try again.\n\nException: " + e.Message + "\n\nInnerException: " + e.InnerException);
						// Analytics.TrackException?.Invoke(e, properties);
				}

				if (printHelper != null)
				{
					try
					{
						properties["line"] = "71";
						printHelper.RegisterForPrinting();
						properties["line"] = "73";
						await printHelper.Init();
						properties["line"] = "75";
						var showprint = await PrintManager.ShowPrintUIAsync();
					}
					catch (Exception e)
					{
						await P42.Uno.Controls.Toast.CreateAsync("Print Service Error", "Could not Show Print UI Async.  Please try again.\n\nException: " + e.Message + "\n\nInnerException: " + e.InnerException);
						//Analytics.TrackException?.Invoke(e, properties);
					}
				}
			});
			return Task.CompletedTask;
		}

		public Task PrintAsync(string html, string jobName)
		{
			var webView = new WebView
			{
				Source = new HtmlWebViewSource
				{
					Html = html
				}
			};
			return PrintAsync(webView, jobName);
		}
	}
}
#endif