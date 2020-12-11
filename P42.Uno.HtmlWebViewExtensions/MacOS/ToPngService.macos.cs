#if __MACOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{
    class NativeToPngService : INativeToPngService
    {
        public bool IsAvailable => false;

        public async Task<ToFileResult> ToPngAsync(string html, string fileName, int width)
        {
            return await Task.FromResult(new ToFileResult(true, "PNG output not implemented in MacOS version of P42.Uno.HtmlWebViewExtensions"));
        }

        public async Task<ToFileResult> ToPngAsync(WebView webView, string fileName, int width)
        {
            return await Task.FromResult(new ToFileResult(true, "PNG output not implemented in MacOS version of P42.Uno.HtmlWebViewExtensions"));
        }
    }
}
#endif
