﻿using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlWebViewExtensions
{
    static class Timer
    {
		public static void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			var handler = new Handler(Looper.MainLooper);
			handler.PostDelayed(() =>
			{
				if (callback?.Invoke() ?? false)
					StartTimer(interval, callback);

				handler.Dispose();
				handler = null;
			}, (long)interval.TotalMilliseconds);
		}
	}
}
