using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{
    /// <summary>
    /// Html to pdf service.
    /// </summary>
    public interface INativeToPngService
    {
        /// <summary>
        /// Html to PNG interface
        /// </summary>
        /// <param name="html">Html text (source)</param>
        /// <param name="fileName">Name of file (without suffix) for local file storage.</param>
        /// <param name="width">Width (in pixels) of PNG.  Default is PageSize.Default * 72 * 4;</param>
        /// <returns></returns>
		Task<ToFileResult> ToPngAsync(string html, string fileName, int width);

        /// <summary>
        /// WebView to PNG interface
        /// </summary>
        /// <param name="webView">Xamarin.Forms.WebView (source)</param>
        /// <param name="fileName">Name of file (without suffix) for local file storage.</param>
        /// <param name="width">Width (in pixels) of PNG.  Default is PageSize.Default * 72 * 4;</param>
        /// <returns></returns>
        Task<ToFileResult> ToPngAsync(WebView webView, string fileName, int width);
    }


}
