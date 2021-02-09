
function UnoWebView_PostMessage(message) {
    console.log('UnoWebView_PostMessage: ' + message);
    let m = JSON.parse(message);
    console.log('message.id: ' + m.Id);
    let target = document.getElementById(m.Id);
    target.contentWindow.postMessage(message);
    console.log('UnoWebView_PostMessage EXIT');
}

function UnoWebView_SetMessageListener() {
    window.addEventListener("message", (event) => {
        console.log("UnoWebView: message received: " + event.data);
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


