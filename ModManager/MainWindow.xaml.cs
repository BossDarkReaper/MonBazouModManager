using System;
using System.Net.Mime;
using System.Windows;
using System.Windows.Input;
using AutoUpdaterDotNET;
using ModManager.Helper;

namespace ModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void Maximize_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState =
                    Application.Current.MainWindow.WindowState == WindowState.Normal
                        ? WindowState.Maximized
                        : WindowState.Normal;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.Shutdown();
        }

        private void Update_OnClick(object sender, RoutedEventArgs e)
        {
            AutoUpdater.InstalledVersion = new Version(VersionChecker.GetLocalManagerVersion());
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
            AutoUpdater.Mandatory = true;
            AutoUpdater.Start("https://changelog.monbazou.level2studios.co.uk/UpdateXml.txt");
        }
    }
}