if (window.parent !== window) {
    console.log("UnoWebBridge ==== ENTER ====");

    function GetGuids() {
        var session = "";
        var instance = "";
        if (window.name !== undefined && window.name !== null) {
            var guids = window.name.split(':');
            session = guids[0];
            if (guids.length > 1)
                instance = guids[1];
        }
        return [session, instance];
    }

    function UnoWebViewBridge_PostMessage(payload) {
        let guids = GetGuids();
        let obj = new Object();
        obj.Session = guids[0]; // sessionStorage.getItem('Uno.WebView.Session');
        obj.Instance = guids[1]; // sessionStorage.getItem('Uno.WebView.Instance');
        obj.Payload = payload;
        let message = JSON.stringify(obj);
        console.log("UnoWebViewBridge_PostMessage: message: " + message);
        window.parent.postMessage(message, "*");
    }

    function UnoWebViewBridge_ReceiveMessage(event) {
        console.log("UnoWebViewBridge_ReceiveMessage: event.data: " + event.data);
        var guids = GetGuids();
        console.log("UnoWebViewBridge_ReceiveMessage: session:  " + guids[0]);
        console.log("UnoWebViewBridge_ReceiveMessage: instance: " + guids[1]);
        //if (event.data.Session == sessionStorage.getItem('Uno.WebView.Session')) {

        if (event.data.Method == 'Navigate') {
            UnoWebViewBridge_Assign(event.data.Payload);
        }
        else if (event.data.Method == 'NavigateToText') {
            var newsrc = 'data:text/html;charset=utf-8;base64,' + event.data.Payload;
            console.log('UnoWebViewBridge_ReceiveMessage NavigateToText newsrc=' + newsrc);
            window.location.assign(newsrc);
            //document.write(atob(event.data.Payload));
        }
        //}
    }

    function UnoWebViewBridge_Initiate() {

        const currentWindowOnLoad = window.onload;
        window.onload = function () {
            if (currentWindowOnLoad !== undefined && currentWindowOnLoad !== null)
                currentWindowOnLoad();

            window.addEventListener("message", UnoWebViewBridge_ReceiveMessage, false);

            console.log("history.length: " + window.history.length);
            console.log("history.state: " + window.history.state);

            console.log("UnoWebViewBridge_initiate: window.onLoad: EXIT");
        }
        console.log("UnoWebViewBridge_initiate: EXIT");
    }

    function UnoWebViewBridge_Assign(x) {
        window.location.assign(x);
    }

    function UnoWebViewBridge_Reload(fromNetwork) {
        window.location.reload(fromNetwork);
    }

    function UnoWebViewBridge_GoBack() {
        window.history.back();
    }

    function UnoWebViewBridge_GoForward() {
        window.history.forward();
    }

    UnoWebViewBridge_Initiate();

    console.log('UnoWebBridge.IsFrame: ' + (window.parent !== window));
    console.log("UnoWebBridge ==== EXIT ====");
}
