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
using Newtonsoft.Json.Linq;

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
        static Dictionary<string, TaskCompletionSource<string>> TCSs = new Dictionary<string, TaskCompletionSource<string>>();

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
                if (!nativeWebView._bridgeConnected)
                {
                    nativeWebView._bridgeConnected = true;
                    nativeWebView.UpdateFromInternalSource();
                }
                nativeWebView.ClearCssStyle("pointer-events");
            }
        }

        public static void OnMessageReceived(string json)
        {
            System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived: " + json);
            var message = JObject.Parse(json);
            if (message.TryGetValue("Target", out var target) && target.ToString() == SessionGuid.ToString())
            {
                if (message.TryGetValue("TaskId", out var taskId) && TCSs.TryGetValue(taskId.ToString(), out var tcs))
                {
                    TCSs.Remove(taskId.ToString());
                    if (message.TryGetValue("Result", out var result))
                        tcs.SetResult(result.ToString());
                    else if (message.TryGetValue("Error", out var error))
                        tcs.SetException(new Exception("Javascript Error: " + error.ToString()));
                    else
                        tcs.SetException(new Exception("Javascript failed for unknown reason"));
                }
            }
        }


        static string WebViewBridgeRootPage => PackageLocation + "Assets/UnoWebViewBridge.html";
        internal static string WebViewBridgeScriptUrl => PackageLocation + "UnoWebViewBridge.js";

        public readonly string Id;
        readonly Guid InstanceGuid;

        private object _internalSource;
        private bool _bridgeConnected;

        public NativeWebView()
        {
            InstanceGuid = Guid.NewGuid();
            Id = this.GetHtmlAttribute("id");
            Instances.Add(Id, new WeakReference<NativeWebView>(this));
            this.SetCssStyle("border", "none");
            //this.ClearCssStyle("pointer-events");  // doesn't seem to work here as it seems to get reset by Uno during layout.
            this.SetHtmlAttribute("name", SessionGuid.ToString() + ":" + InstanceGuid.ToString());
            this.SetHtmlAttribute("onLoad", $"UnoWebView_OnLoad('{Id}')");
            this.SetHtmlAttribute("src", WebViewBridgeRootPage);
        }


        void Navigate(Uri uri)
        {
            _bridgeConnected = false;
            _internalSource = null;
            WebAssemblyRuntime.InvokeJS(new Message<Uri>(this, uri));
        }

        void NavigateToText(string text)
        {
            text = WebViewXExtensions.InjectWebBridge(text);
            var valueBytes = Encoding.UTF8.GetBytes(text);
            var base64 = Convert.ToBase64String(valueBytes);
            _bridgeConnected = false;
            _internalSource = null;
            WebAssemblyRuntime.InvokeJS(new Message<string>(this, "data:text/html;charset=utf-8;base64," + base64));
        }

        void NavigateWithHttpRequestMessage(HttpRequestMessage message)
        {
            throw new NotSupportedException();
        }


        internal async Task<string> InvokeScriptAsync(string functionName, string[] arguments)
        {
            var tcs = new TaskCompletionSource<string>();
            var taskId = Guid.NewGuid().ToString();
            TCSs.Add(taskId, tcs);
            WebAssemblyRuntime.InvokeJS(new ScriptMessage(this, taskId, functionName, arguments));
            return await tcs.Task;
        }

        

        internal void SetInternalSource(object source)
        {
            _internalSource = source;
            UpdateFromInternalSource();
        }

        private void UpdateFromInternalSource()
        {
            if (_bridgeConnected)
            {
                if (_internalSource is Uri uri)
                {
                    Navigate(uri);
                    return;
                }
                if (_internalSource is string html)
                {
                    NavigateToText(html);
                }
                if (_internalSource is HttpRequestMessage message)
                {
                    NavigateWithHttpRequestMessage(message);
                }
            }
        }


        class Message
        {
            public string Source { get; private set; }

            public string Method { get; private set; }

            public string Target { get; private set; }

            [JsonIgnore]
            public string Id { get; private set; }

            public Message(NativeWebView nativeWebView, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
            {
                Source = SessionGuid.ToString();
                Id = nativeWebView.Id;
                Target = nativeWebView.InstanceGuid.ToString();
                Method = callerName;
            }

            public override string ToString() => JsonConvert.SerializeObject(this);

            public static implicit operator string(Message m) => $"UnoWebView_PostMessage('{m.Id}','{m}');";

        }

        class Message<T> : Message
        {
            public T Payload { get; private set; }

            public Message(NativeWebView nativeWebView, T payload, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) 
                : base(nativeWebView, callerName)
                => Payload = payload;
        }

        class ScriptMessage : Message<string[]>
        {
            public string FunctionName { get; private set; }

            public string TaskId { get; private set; }

            public ScriptMessage(NativeWebView nativeWebView, string taskId, string functionName, string[] arguments, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) 
                : base(nativeWebView, arguments, callerName)
            {
                FunctionName = functionName;
                TaskId = taskId;
            }
        }
    }
}
