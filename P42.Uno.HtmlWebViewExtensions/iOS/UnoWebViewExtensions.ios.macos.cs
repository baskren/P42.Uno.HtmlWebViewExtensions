﻿#if __IOS__ || __MACOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace P42.Uno.HtmlWebViewExtensions
{
    static class UnoWebViewExtensions
    {
        public static Windows.UI.Xaml.Controls.UnoWKWebView GetNativeWebView(this Windows.UI.Xaml.Controls.WebView unoWebView)
        {
            var count = VisualTreeHelper.GetChildrenCount(unoWebView);
            for (int i = 0; i < count; i++)
            {
                if (VisualTreeHelper.GetChild(unoWebView, i) is Windows.UI.Xaml.Controls.UnoWKWebView nativeWebView)
                    return nativeWebView;
            }
            return null;
        }
    }
}
#endif