﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using P42.Uno.HtmlWebViewExtensions;
using Windows.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            _toPngButton.IsEnabled = ToPngService.IsAvailable;
            _toPdfButton.IsEnabled = ToPdfService.IsAvailable;
        }


        async void OnToPngClicked(object sender, RoutedEventArgs e)
        {
            ShowSpinner();
            if ( await _webView.ToPngAsync("WebView.png") is ToFileResult fileResult)
            {
                if (!fileResult.IsError)
                {
                    _messageTextBlock.Text = "Success: " + fileResult.Result;
                    var shareFile = new Xamarin.Essentials.ShareFile(fileResult.Result, "image/png") { FileName = "WebView.png" };
                    var shareRequest = new Xamarin.Essentials.ShareFileRequest("P42.Uno.HtmlWebViewExtensions PNG", shareFile);
                    await Xamarin.Essentials.Share.RequestAsync(shareRequest);
                }
                else
                {
                    _messageTextBlock.Text = "Error: " + fileResult.Result;
                }
            }
            HideSpinner();
        }

        async void OnToPdfClicked(object sender, RoutedEventArgs e)
        {
            ShowSpinner();
            if (await _webView.ToPdfAsync("WebView.pdf") is ToFileResult fileResult)
            {
                if (!fileResult.IsError)
                {
                    _messageTextBlock.Text = "Success: " + fileResult.Result;
                    var shareFile = new Xamarin.Essentials.ShareFile(fileResult.Result, "application/pdf") { FileName = "WebView.pdf" };
                    var shareRequest = new Xamarin.Essentials.ShareFileRequest("P42.Uno.HtmlWebViewExtensions PDF", shareFile);
                    await Xamarin.Essentials.Share.RequestAsync(shareRequest);
                }
                else
                {
                    _messageTextBlock.Text = "Error: " + fileResult.Result;
                }
            }
            HideSpinner();
        }

        async void OnPrintClicked(object sender, RoutedEventArgs e)
        {
            //ShowSpinner();
            await _webView.PrintAsync("WebView PrintJob");
            //HideSpinner();
        }

        Grid _spinner;
        ProgressRing _ring;
        void ShowSpinner()
        {
            if (_spinner is null)
            {
                _ring = new ProgressRing
                {
                    BorderBrush = new SolidColorBrush(Colors.Red),
                    Foreground = new SolidColorBrush(Colors.Blue),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                Grid.SetRow(_ring, 1);
                Grid.SetColumn(_ring, 1);

                _spinner = new Grid
                {
                    RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(50) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(50) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
                    Children =
                {
                    _ring,
                },
                    Background = new SolidColorBrush(Color.FromArgb(64, 128, 128, 128))
                };
            }
            Grid.SetRowSpan(_spinner, _grid.RowDefinitions.Count);
            Grid.SetColumnSpan(_spinner, _grid.ColumnDefinitions.Count);
            _grid.Children.Add(_spinner);
            _ring.IsActive = true;
        }

        void HideSpinner()
        {
            _grid.Children.Remove(_spinner);
            _ring.IsActive = false;
        }
    }
}
