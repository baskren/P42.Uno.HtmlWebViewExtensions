using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Uno.UI.Runtime.WebAssembly;
using Uno.Foundation;
using System.Net.Http;
using System.IO;

namespace P42.Uno.HtmlWebViewExtensions
{
    [HtmlElement("iframe")]
    public partial class NativeWebView : FrameworkElement
    {
        /*
        static string OnLoadScript(int index)
        {
            return $"$console.log('UnoWebView_OnLoad: {index}, this.contentWindow.location);" +
            "const OnCurrentWindowLocationChanged = Module.mono_bind_static_method(\"[P42.Uno.HtmlWebViewExtensions] P42.Uno.HtmlWebViewExtensions.NativeWebView:OnCurrentWindowLocationChanged\");" +
            $"OnCurrentWindowLocationChanged('{index}', location);";
        }
        */

        static string OnLoadScript(int index)
        {
            return $"";
        }



        static Dictionary<string, WeakReference<NativeWebView>> Instances = new Dictionary<string, WeakReference<NativeWebView>>();

        static NativeWebView InstanceAtId(string id)
        {
            if (Instances.TryGetValue(id, out var weakRef))
            {
                if (weakRef.TryGetTarget(out var nativeWebView))
                    return nativeWebView;
            }
            return null;
        }

        public static void OnFrameLoaded(string id)
        {
            System.Diagnostics.Debug.WriteLine("NativeWebView.OnFrameLoaded(" + id + ")");
            if (InstanceAtId(id) is NativeWebView nativeWebView)
            {

                nativeWebView._loaded = true;
                nativeWebView.UpdateFromInternalSource();
                //nativeWebView.ClearCssStyle("pointer-events");
                /*
                System.Diagnostics.Debug.WriteLine($"NativeWebView.OnFrameLoaded src=[{nativeWebView.GetHtmlAttribute("src")}]");
                System.Diagnostics.Debug.WriteLine("NativeWebView.GetLocation(): " + nativeWebView.GetLocation());
                nativeWebView.SetHtmlAttribute("sandbox", "allow-downloads allow-forms allow-modals allow-pointer-lock allow-popups allow-popups-to-escape-sandbox allow-presentation allow-same-origin allow-scripts allow-storage-access-by-user-activation allow-top-navigation-by-user-activation");
                */
            }
        }

        static string _webViewBridgeScript;
        static string WebViewBridgeScript
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_webViewBridgeScript))
                {
                    using (var stream = typeof(P42.Uno.HtmlWebViewExtensions.NativePrintService).Assembly.GetManifestResourceStream("P42.Uno.HtmlWebViewExtensions.WasmScripts.UnoWebViewBridge.js.txt"))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            _webViewBridgeScript = reader.ReadToEnd();
                        }
                    }
                }
                return _webViewBridgeScript;
            }
        }

        public readonly string Id;

        private object _internalSource;
        private bool _loaded;

        public NativeWebView()
        {
            System.Diagnostics.Debug.WriteLine("NativeWebView..ctr");
            //var script = OnLoadScript(IndexOfInstance(this));
            Id = this.GetHtmlAttribute("id");
            System.Diagnostics.Debug.WriteLine("NativeWebView.ctr id=" + Id);
            Instances.Add(Id, new WeakReference<NativeWebView>(this));
            this.SetCssStyle("border", "none");
            this.ClearCssStyle("pointer-events");
            this.SetHtmlAttribute("onLoad", $"UnoWebView_OnLoad('{Id}')");
            this.SetHtmlAttribute("srcdoc", $"<script>{WebViewBridgeScript}</script>");
            //System.Diagnostics.Debug.WriteLine($"NativeWebView.ctr script=[{WebViewBridgeScript}]");
            //System.Diagnostics.Debug.WriteLine("NativeWebView..ctr script=["+script+"]");
            //this.SetHtmlAttribute("onLoad", script);
            //this.SetHtmlAttribute("onLoad", "alert(this.contentWindow.location);");
            //this.SetHtmlAttribute("onLoad", "Window.PostMessage();");
            //this.SetHtmlAttribute("onLoad", $"alert('{this.GetHtmlAttribute("id")}');");
            //this.SetHtmlAttribute("onload", $"UnoWebView_OnLoad('{Guid}')");
            //this.SetHtmlAttribute("onload", $"");
            //this.SetHtmlAttribute("name", "NativeWebView" + InstanceIndex);
            //this.SetCssStyle("pointer-events", "auto");
            //this.ClearCssStyle("pointer-events");
            //this.SetHtmlAttribute("sandbox", "allow-downloads allow-forms allow-modals allow-pointer-lock allow-popups allow-popups-to-escape-sandbox allow-presentation allow-same-origin allow-scripts allow-storage-access-by-user-activation allow-top-navigation-by-user-activation");
            //WebAssemblyRuntime.InvokeJS($"UnoWebView_SetMessageListener();");
        }


        void Navigate(Uri uri)
            => WebAssemblyRuntime.InvokeJS(new Message<Uri>(Id, uri));

        void NavigateToText(string text)
            => WebAssemblyRuntime.InvokeJS(new Message<string>(Id, text));

        void NavigateWithHttpRequestMessage(HttpRequestMessage message)
        {
            throw new NotSupportedException();
        }

        internal void GoForward()
        {
            if (_loaded)
                WebAssemblyRuntime.InvokeJS(new Message(Id));
        }

        internal void GoBack()
        {
            if (_loaded)
                WebAssemblyRuntime.InvokeJS(new Message(Id));
        }

        internal void SetInternalSource(object source)
        {
            _internalSource = source;
            UpdateFromInternalSource();
        }

        private void UpdateFromInternalSource()
        {
            if (_loaded)
            {
                var uri = _internalSource as Uri;
                if (uri != null)
                {
                    Navigate(uri);
                    return;
                }

                var html = _internalSource as string;
                if (html != null)
                {
                    NavigateToText(html);
                }

                var message = _internalSource as HttpRequestMessage;
                if (message != null)
                {
                    NavigateWithHttpRequestMessage(message);
                }
            }
        }

        string GetLocation()
        {

            //var result = WebAssemblyRuntime.InvokeJS($"window.location.href");
            var result = WebAssemblyRuntime.InvokeJS($"$('#{this.GetHtmlAttribute("id")}').get(0).contentWindow.location");
            return result;
        }



        class Message
        {
            //public string Type => GetType().Name;

            public string Method { get; private set; }

            public string Id { get; private set; }

            public Message(string id, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
            {
                Id = id;
                Method = callerName;
            }

            public override string ToString() => Newtonsoft.Json.JsonConvert.SerializeObject(this);

            public static implicit operator string(Message m) => $"UnoWebView_PostMessage('{m.ToString()}');";

        }

        class Message<T> : Message
        {
            public T Payload { get; private set; }

            public Message(string id, T payload, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) : base(id, callerName)
                => Payload = payload;
        }


    }
}
