#if __MACOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{
    class NativeToPdfService : INativeToPdfService
    {
        public bool IsAvailable => false;

        public async Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin)
        {
            return await Task.FromResult(new ToFileResult(true, "PDF output not implemented in MacOS version of P42.Uno.HtmlWebViewExtensions"));
        }

        public async Task<ToFileResult> ToPdfAsync(WebView webView, string fileName, PageSize pageSize, PageMargin margin)
        {
            return await Task.FromResult(new ToFileResult(true, "PNG output not implemented in MacOS version of P42.Uno.HtmlWebViewExtensions"));
        }
    }
}
#endif
