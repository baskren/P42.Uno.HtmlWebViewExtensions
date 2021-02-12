
function UnoWebView_PostMessage(id, message) {
    console.log('UnoWebView_PostMessage[' + id + ']: ' + message);
    let m = JSON.parse(message);
    let target = document.getElementById(id);
    target.contentWindow.postMessage(m, "*");
}

function UnoWebView_SetMessageListener() {
    window.addEventListener("message", (event) => {
        let ignore = false;
        if (typeof event.data === "string" || event.data instanceof String) {
            ignore = event.data.toString().startsWith("setImmediate");
        }
        if (!ignore) {
            console.log("UnoWebView: message received: " + JSON.stringify(event.data));
            if (event.data.Target == sessionStorage.getItem('Uno.WebView.Session')) {
                const OnMessageReceived = Module.mono_bind_static_method("[P42.Uno.HtmlWebViewExtensions] P42.Uno.HtmlWebViewExtensions.NativeWebView:OnMessageReceived");
                var json = JSON.stringify(event.data);
                OnMessageReceived(json);
            }
        }
    }, false);
}

function UnoWebView_OnLoad(index) {
    console.log("UnoWebView_OnLoad: " + index);
    UnoWebView_SetMessageListener();
    const OnFrameLoaded = Module.mono_bind_static_method("[P42.Uno.HtmlWebViewExtensions] P42.Uno.HtmlWebViewExtensions.NativeWebView:OnFrameLoaded");
    OnFrameLoaded(index);
}

