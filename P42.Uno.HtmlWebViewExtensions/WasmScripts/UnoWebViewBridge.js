if (window.parent !== window) {

    function GetGuids() {
        var session = "";
        var instance = "";
        if (window.name !== undefined && window.name !== null) {
            var guids = window.name.split(':');
            session = guids[0];
            if (guids.length > 1)
                instance = guids[1];
        }
        return { session: session, instance: instance };
    }

    function UnoWebViewBridge_PostMessage(message) {
        let guids = GetGuids();
        message.Target = guids.session;
        message.Source = guids.instance;
        window.parent.postMessage(message, "*");
    }

    function UnoWebViewBridge_ReceiveMessage(event) {
        var guids = GetGuids();

        if (event.data.Source == guids.session && event.data.Target == guids.instance) {

            if (event.data.Method == 'Navigate') {
                window.location.assign(event.data.Payload);
                return;
            }
            else if (event.data.Method == 'NavigateToText') {
                window.location.assign(event.data.Payload);
                return;
            }
            else if (event.data.Method == 'Reload') {
                window.location.reload(event.data.Payload);
                return;
            }
            else if (event.data.Method == 'GoForward') {
                window.history.forward();
            }
            else if (event.data.Method == 'GoBack') {
                window.history.back();
            }
            else if (event.data.Method == 'InvokeScriptAsync') {
                try {
                    let args = "";
                    if (event.data.Payload !== undefined && event.data.Payload !== null)
                        args = event.data.Payload.join();
                    let script = event.data.FunctionName + '(' + args + ');'
                    console.log('script: ' + script);
                    var result = eval(script);
                    console.log('resutl: ' + result);
                    if (result === undefined || result === null)
                        result = "";
                    UnoWebViewBridge_PostMessage({ Method: event.data.Method, TaskId: event.data.TaskId, Result: result });
                } catch (error) {
                    UnoWebViewBridge_PostMessage({ Method: event.data.Method, TaskId: event.data.TaskId, Error: error });
                }
                return;
            }

            // note that navigation echo messages never get sent
            UnoWebViewBridge_PostMessage({ Method: "echo", Arguments: [event.data.Method] });
        }
        else     {
            console.log('unknown message: ' + JSON.stringify(event.data));
        }
    }

    function UnoWebViewBridge_InvokeScriptAsync(json) {
        try {

        } catch (error) {

        }
    }

    function UnoWebViewBridge_Initiate() {

        const currentWindowOnLoad = window.onload;
        window.onload = function () {

            let title = "";
            if (document.title !== undefined && document.title !== null)
                title = document.title;

            if (!history.state && typeof (history.replaceState) == "function")
                history.replaceState({ page: history.length, href: location.href, title: title }, title);

            if (currentWindowOnLoad !== undefined && currentWindowOnLoad !== null)
                currentWindowOnLoad();

            window.addEventListener("message", UnoWebViewBridge_ReceiveMessage, false);

            UnoWebViewBridge_PostMessage({ Method: "OnBridgeLoaded", Pages: window.history.length, Page: window.history.state.page, Href: window.location.href });
        }
    }

    UnoWebViewBridge_Initiate();
}
