using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uno.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlWebViewExtensions
{
    class NativePrintService
    {
        public bool IsAvailable()
        {
            var result = WebAssemblyRuntime.InvokeJS("typeof window.print == 'function';");
            return result == "true";
        }

        public async Task PrintAsync(WebView webView, string jobName)
        {
            var id = webView.GetHtmlAttribute("id");
            var result = WebAssemblyRuntime.InvokeJS($"UnoPrint_PrintElement('{id}');");
            await Task.CompletedTask;
        }


    }
}
