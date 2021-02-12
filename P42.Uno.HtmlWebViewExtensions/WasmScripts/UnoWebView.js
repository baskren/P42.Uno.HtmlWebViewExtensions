
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

/*
function UnoWebView_iframeURLChange(iframe, callback) {
    var unloadHandler = function () {
        // Timeout needed because the URL changes immediately after
        // the `unload` event is dispatched.
        setTimeout(function () {
            callback(iframe.contentWindow.location.href);
        }, 0);
    };

    function attachUnload() {
        // Remove the unloadHandler in case it was already attached.
        // Otherwise, the change will be dispatched twice.
        iframe.contentWindow.removeEventListener("unload", unloadHandler);
        iframe.contentWindow.addEventListener("unload", unloadHandler);
    }

    iframe.addEventListener("load", attachUnload);
    attachUnload();
}

function UnoWebView_TrackSrcChange(iframeId) {
    UnoWebView_iframeURLChange(document.getElementById(iframeId), function (newURL) {
        console.log("URL changed:", newURL);
    });
}

*/


