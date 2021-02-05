﻿using Android.Runtime;
using Android.Views;
using System;

namespace P42.Uno.HtmlWebViewExtensions
{
    static class Display
    {
        public static double Scale
        {
            get
            {
                using var displayMetrics = new global::Android.Util.DisplayMetrics();
                //using var service = Platform.AppContext.GetSystemService(Context.WindowService);
                using var service = global::Uno.UI.ContextHelper.Current.GetSystemService(global::Android.App.Activity.WindowService);
                using var windowManager = service?.JavaCast<IWindowManager>();
                var display = windowManager?.DefaultDisplay;
                display?.GetRealMetrics(displayMetrics);
                var density = displayMetrics?.Density ?? 1;
                return density;
            }
        }
    }

}
