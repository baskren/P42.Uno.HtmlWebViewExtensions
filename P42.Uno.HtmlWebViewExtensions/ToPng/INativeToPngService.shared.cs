using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{
    interface INativeToPngService
    {
        bool IsAvailable { get; }

		Task<ToFileResult> ToPngAsync(string html, string fileName, int width);

        Task<ToFileResult> ToPngAsync(WebView webView, string fileName, int width);
    }


}
