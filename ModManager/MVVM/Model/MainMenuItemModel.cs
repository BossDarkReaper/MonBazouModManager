using System.Windows.Controls;

namespace ModManager.MVVM.Model;

public class MainMenuItemModel
{
    public string Title { get; set; }
    public string Info { get; set; }
    public string ImageSource { get; set; }
    
    public UserControl View { get; set; }
}