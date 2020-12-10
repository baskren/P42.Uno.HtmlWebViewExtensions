using Android.Graphics;
using Android.Views;
using System;
using System.Threading.Tasks;

namespace P42.Uno.HtmlWebViewExtensions.Droid
{
    class WebViewCallBack : Android.Webkit.WebViewClient
    {
        bool _complete;
        readonly string _fileName;
        readonly PageSize _pageSize;
        readonly PageMargin _margin;
        readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
        readonly Func<Android.Webkit.WebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> _onPageFinished;

        public WebViewCallBack(TaskCompletionSource<ToFileResult> taskCompletionSource, string fileName, PageSize pageSize, PageMargin margin, Func<Android.Webkit.WebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> onPageFinished)
        {
            _fileName = fileName;
            _pageSize = pageSize;
            _margin = margin;
            _taskCompletionSource = taskCompletionSource;
            _onPageFinished = onPageFinished;
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            System.Diagnostics.Debug.WriteLine(nameof(WebViewCallBack) + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnPageStarted(view, url, favicon);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "Needed to invoke async code on main thread.")]
        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine(nameof(WebViewCallBack) + P42.Utils.ReflectionExtensions.CallerString() + ": SUCCESS!");
            if (!_complete)
            {
                _complete = true;

                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                {
                    _onPageFinished?.Invoke(view, _fileName, _pageSize, _margin, _taskCompletionSource);
                });
            }
        }

        public override void OnReceivedError(Android.Webkit.WebView view, Android.Webkit.IWebResourceRequest request, Android.Webkit.WebResourceError error)
        {
            base.OnReceivedError(view, request, error);
            _taskCompletionSource.SetResult(new ToFileResult(true, error.Description));
        }

        public override void OnReceivedHttpError(Android.Webkit.WebView view, Android.Webkit.IWebResourceRequest request, Android.Webkit.WebResourceResponse errorResponse)
        {
            base.OnReceivedHttpError(view, request, errorResponse);
            _taskCompletionSource.SetResult(new ToFileResult(true, errorResponse.ReasonPhrase));
        }

        public override bool OnRenderProcessGone(Android.Webkit.WebView view, Android.Webkit.RenderProcessGoneDetail detail)
        {
            System.Diagnostics.Debug.WriteLine(nameof(WebViewCallBack) + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            return base.OnRenderProcessGone(view, detail);
        }

        public override void OnLoadResource(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine(nameof(WebViewCallBack) + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnLoadResource(view, url);
            P42.Utils.Timer.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                if (!_complete)
                    OnPageFinished(view, url);
                return false;
            });
        }

        public override void OnPageCommitVisible(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine(nameof(WebViewCallBack) + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnPageCommitVisible(view, url);
        }

        public override void OnUnhandledKeyEvent(Android.Webkit.WebView view, KeyEvent e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(WebViewCallBack) + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnUnhandledKeyEvent(view, e);
        }

        public override void OnUnhandledInputEvent(Android.Webkit.WebView view, InputEvent e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(WebViewCallBack) + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnUnhandledInputEvent(view, e);
        }
    }
}
