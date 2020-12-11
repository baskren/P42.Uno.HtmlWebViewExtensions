using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{
    public static class PrintService
    {
        static INativePrintService _nativePrintService;
        static INativePrintService NativePrintService =>
#if __IOS__ || __ANDROID__ || NETFX_CORE
            _nativePrintService = _nativePrintService ?? new NativePrintService();
#else
            null;
#endif

        /// <summary>
        /// Print the specified webview and jobName.
        /// </summary>
        /// <param name="webview">Webview.</param>
        /// <param name="jobName">Job name.</param>
        public static async Task PrintAsync(this WebView webview, string jobName)
        {
            await (NativePrintService?.PrintAsync(webview, jobName) ?? Task.Delay(5));
        }

        /// <summary>
        /// Print HTML string
        /// </summary>
        /// <param name="html"></param>
        /// <param name="jobName"></param>
        public static async Task PrintAsync(this string html, string jobName)
        {
            await (NativePrintService?.PrintAsync(html, jobName) ?? Task.Delay(5));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Forms9Patch.WebViewExtensions"/> can print.
        /// </summary>
        /// <value><c>true</c> if can print; otherwise, <c>false</c>.</value>
        public static bool IsAvailable => NativePrintService?.IsAvailable() ?? false;
    }
}
