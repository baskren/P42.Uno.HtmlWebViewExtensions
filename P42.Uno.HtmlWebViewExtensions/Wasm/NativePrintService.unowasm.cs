using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uno.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#if __WASM__
using WebView = P42.Uno.HtmlWebViewExtensions.WebViewX;
#else
using WebView = Windows.UI.Xaml.Controls.WebView;
#endif

namespace P42.Uno.HtmlWebViewExtensions
{
    class NativePrintService : INativePrintService
    {
        public bool IsAvailable()
        {
            var result = WebAssemblyRuntime.InvokeJS("typeof window.print == 'function';");
            return result == "true";
        }

        public async Task PrintAsync(WebView webView, string jobName)
        {
            //var id = webView.GetHtmlAttribute("id");
            //var result = WebAssemblyRuntime.InvokeJS($"UnoPrint_PrintElement('{id}');");
            var result = await webView.InvokeScriptAsync("window.print", null);
            //await Task.CompletedTask;
        }

        public Task PrintAsync(string html, string jobName)
        {
            throw new NotImplementedException();
        }
    }
}
