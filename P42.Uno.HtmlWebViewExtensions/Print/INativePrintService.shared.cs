using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{
    /// <summary>
    /// Print service.
    /// </summary>
    public interface INativePrintService
    {
        /// <summary>
        /// Print the specified webView and jobName.
        /// </summary>
        /// <param name="webView">Web view.</param>
        /// <param name="jobName">Job name.</param>
        Task PrintAsync(WebView webView, string jobName);

        /// <summary>
        /// Print the specified HTML with jobName
        /// </summary>
        /// <param name="html"></param>
        /// <param name="jobName"></param>
        Task PrintAsync(string html, string jobName);

        /// <summary>
        /// Cans the print.
        /// </summary>
        /// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
        bool CanPrint();

    }
}
