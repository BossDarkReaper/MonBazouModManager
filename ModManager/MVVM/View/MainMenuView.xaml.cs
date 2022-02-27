using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ModManager.MVVM.View;

public partial class MainMenuView : UserControl
{
    public string IsInstalledText { get; set; }
    
    public MainMenuView()
    {
        IsInstalledText = "test";
        InitializeComponent();
    }

    private void Discord_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = e.Uri.AbsoluteUri;
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            e.Handled = true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}