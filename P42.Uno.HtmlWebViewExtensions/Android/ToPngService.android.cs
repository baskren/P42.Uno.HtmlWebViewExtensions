using System.IO;
using Android.Graphics;
using Android.Views;
using System.Threading.Tasks;
using System.Reflection;
using System;
using Android.Runtime;
using Android.OS;
using Android.Content;
using P42.Uno.HtmlWebViewExtensions.Droid;

namespace P42.Uno.HtmlWebViewExtensions
{

    public class NativeToPngService : Java.Lang.Object, INativeToPngService
    {

        public bool IsAvailable => true;

        public async Task<ToFileResult> ToPngAsync(string html, string fileName, int width)
        {
            //if (!await XamarinEssentialsExtensions.ConfirmOrRequest<Xamarin.Essentials.Permissions.StorageWrite>())
            //    return new ToFileResult(true, "Write External Stoarge permission must be granted for PNG images to be available.");
            using (var webView = new Android.Webkit.WebView(Android.App.Application.Context))
            {
                webView.Settings.JavaScriptEnabled = true;
#pragma warning disable CS0618 // Type or member is obsolete
                webView.DrawingCacheEnabled = true;
#pragma warning restore CS0618 // Type or member is obsolete
                webView.SetLayerType(LayerType.Software, null);

                webView.Layout(0, 0, width, width);
                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                using (var callback = new WebViewCallBack(taskCompletionSource, fileName, new PageSize { Width = width }, null, OnPageFinished))
                {
                    webView.SetWebViewClient(callback);
                    webView.LoadData(html, "text/html; charset=utf-8", "UTF-8");
                    return await taskCompletionSource.Task;
                }
            }
        }

        public async Task<ToFileResult> ToPngAsync(Windows.UI.Xaml.Controls.WebView unoWebView, string fileName, int width)
        {
            //if (!await XamarinEssentialsExtensions.ConfirmOrRequest<Xamarin.Essentials.Permissions.StorageWrite>())
            //    return new ToFileResult(true, "Write External Stoarge permission must be granted for PNG images to be available.");
            if (unoWebView.GetNativeWebView() is Android.Webkit.WebView droidWebView)
            {
                droidWebView.SetLayerType(LayerType.Software, null);
                droidWebView.Settings.JavaScriptEnabled = true;
#pragma warning disable CS0618 // Type or member is obsolete
                droidWebView.DrawingCacheEnabled = true;
                droidWebView.BuildDrawingCache();
#pragma warning restore CS0618 // Type or member is obsolete
                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                using (var callback = new WebViewCallBack(taskCompletionSource, fileName, new PageSize { Width = width }, null, OnPageFinished))
                {
                    droidWebView.SetWebViewClient(callback);
                    return await taskCompletionSource.Task;
                }
            }
            return await Task.FromResult(new ToFileResult(true, "Could not get NativeWebView for Uno WebView"));
        }


        static async Task OnPageFinished(Android.Webkit.WebView droidWebView, string fileName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            var specWidth = MeasureSpecFactory.MakeMeasureSpec((int)(pageSize.Width), MeasureSpecMode.Exactly);
            var specHeight = MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
            droidWebView.Measure(specWidth, specHeight);
            var height = droidWebView.ContentHeight;
            droidWebView.Layout(0, 0, droidWebView.MeasuredWidth, height);

            if (height < 1)
            {
                var heightString = await droidWebView.EvaluateJavaScriptAsync("document.documentElement.offsetHeight");
                height = (int)System.Math.Ceiling(double.Parse(heightString.ToString()));
            }

            var width = droidWebView.MeasuredWidth;

            if (width < 1)
            {
                var widthString = await droidWebView.EvaluateJavaScriptAsync("document.documentElement.offsetWidth");
                width = (int)System.Math.Ceiling(double.Parse(widthString.ToString()));
            }

            if (height < 1 || width < 1)
            {
                taskCompletionSource.SetResult(new ToFileResult(true, "WebView width or height is zero."));
                return;
            }


            await Task.Delay(50);
            using (var _dir = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads))
            {
                if (!_dir.Exists())
                    _dir.Mkdir();

                var file = Java.IO.File.CreateTempFile(fileName + ".", ".png", _dir);
                var path = file.AbsolutePath;

                using (var stream = new FileStream(file.Path, FileMode.Create, System.IO.FileAccess.Write))
                {
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Honeycomb)
                    {
                        await Task.Delay(1000);

                        //using (var bitmap = Bitmap.CreateBitmap(System.Math.Max(view.MeasuredWidth, view.ContentWidth()), view.MeasuredHeight, Bitmap.Config.Argb8888))
                        using (var bitmap = Bitmap.CreateBitmap(droidWebView.MeasuredWidth, height, Bitmap.Config.Argb8888))
                        {
                            using (var canvas = new Canvas(bitmap))
                            {
                                if (droidWebView.Background != null)
                                    droidWebView.Background.Draw(canvas);
                                else
                                    canvas.DrawColor(Android.Graphics.Color.White);

                                droidWebView.SetClipChildren(false);
                                droidWebView.SetClipToPadding(false);
                                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                                    droidWebView.ClipToOutline = false;

                                await Task.Delay(50);
                                droidWebView.Draw(canvas);
                                await Task.Delay(50);
                                bitmap.Compress(Bitmap.CompressFormat.Png, 80, stream);
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(1000);
#pragma warning disable CS0618 // Type or member is obsolete
                        using (var bitmap = Bitmap.CreateBitmap(droidWebView.DrawingCache))
#pragma warning restore CS0618 // Type or member is obsolete
                        {
                            bitmap.Compress(Bitmap.CompressFormat.Png, 80, stream);
                        }
                    }
                    stream.Flush();
                    stream.Close();
                    taskCompletionSource.SetResult(new ToFileResult(false, path));
                }
                file.Dispose();
            }
            
        }
    }

}
