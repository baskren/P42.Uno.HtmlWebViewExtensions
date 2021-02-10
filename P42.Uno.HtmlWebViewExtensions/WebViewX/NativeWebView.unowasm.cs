using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Uno.UI.Runtime.WebAssembly;
using Uno.Foundation;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace P42.Uno.HtmlWebViewExtensions
{
    [HtmlElement("iframe")]
    public partial class NativeWebView : FrameworkElement
    {
        static readonly Guid SessionGuid = Guid.NewGuid();
        static readonly string Location;
        static readonly string PackageLocation;

        static NativeWebView()
        {
            WebAssemblyRuntime.InvokeJS($"sessionStorage.setItem('Uno.WebView.Session','{SessionGuid}');");
            Location = WebAssemblyRuntime.InvokeJS("window.location.href");
            PackageLocation = WebAssemblyRuntime.InvokeJS("window.scriptDirectory");
            System.Diagnostics.Debug.WriteLine("NativeWebView.STATIC location: " + Location);
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
                if (!nativeWebView._loaded)
                {
                    nativeWebView._loaded = true;
                    nativeWebView.UpdateFromInternalSource();
                }
                nativeWebView.ClearCssStyle("pointer-events");
            }
        }


        static string WebViewBridgeRootPage => PackageLocation + "Assets/UnoWebViewBridge.html";
        internal static string WebViewBridgeScriptUrl => PackageLocation + "UnoWebViewBridge.js";

        public readonly string Id;
        readonly Guid InstanceGuid;

        private object _internalSource;
        private bool _loaded;

        public NativeWebView()
        {
            InstanceGuid = Guid.NewGuid();
            Id = this.GetHtmlAttribute("id");
            Instances.Add(Id, new WeakReference<NativeWebView>(this));
            this.SetCssStyle("border", "none");
            //this.ClearCssStyle("pointer-events");  // doesn't seem to work here as it seems to get reset by Uno during layout.
            this.SetHtmlAttribute("onLoad", $"UnoWebView_OnLoad('{Id}')");
            this.SetHtmlAttribute("name", SessionGuid.ToString() + ":" + InstanceGuid.ToString());
            this.SetHtmlAttribute("src", WebViewBridgeRootPage);
        }


        void Navigate(Uri uri)
            => WebAssemblyRuntime.InvokeJS(new Message<Uri>(Id, uri));

        void NavigateToText(string text)
        {
            text = WebViewXExtensions.InjectWebBridge(text);
            var valueBytes = Encoding.UTF8.GetBytes(text);
            var base64 = Convert.ToBase64String(valueBytes);
            WebAssemblyRuntime.InvokeJS(new Message<string>(Id, "data:text/html;charset=utf-8;base64," + base64));
        }

        void NavigateWithHttpRequestMessage(HttpRequestMessage message)
        {
            throw new NotSupportedException();
        }


        internal async Task<string> InvokeScriptAsync(string script, string[] arguments)
        {

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
            public string Session { get; private set; }

            public string Method { get; private set; }

            [JsonIgnore]
            public string Id { get; private set; }

            public Message(string id, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
            {
                Session = SessionGuid.ToString();
                Id = id;
                Method = callerName;
            }

            public override string ToString() => JsonConvert.SerializeObject(this);

            public static implicit operator string(Message m) => $"UnoWebView_PostMessage('{m.Id}','{m}');";

        }

        class Message<T> : Message
        {
            public T Payload { get; private set; }

            public Message(string id, T payload, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) : base(id, callerName)
                => Payload = payload;
        }

        class ScriptMessage : Message<string[]>
        {
            public string Script { get; private set; }

            public ScriptMessage(string id, string script, string[] arguments, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) : base(id, arguments, callerName)
                => Script = script;
        }
    }
}
