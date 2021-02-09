using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlWebViewExtensions
{
    public sealed class WebViewXUnsupportedUriSchemeIdentifiedEventArgs
    {
        public bool Handled
        {
            get;
            set;
        }

        public Uri Uri
        {
            get;
            private set;
        }

        public WebViewXUnsupportedUriSchemeIdentifiedEventArgs(Uri uri)
        {
            Uri = uri;
        }
    }
}
