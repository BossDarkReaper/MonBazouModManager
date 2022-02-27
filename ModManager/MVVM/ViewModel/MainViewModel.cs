using System;
using System.Collections.ObjectModel;
using System.Windows;
using ModManager.Core;
using ModManager.Helper;
using ModManager.MVVM.Model;
using ModManager.MVVM.View;

namespace ModManager.MVVM.ViewModel;

public class MainViewModel: ObservableObject
{
    #region Main Menu Items

    public ObservableCollection<MainMenuItemModel> MainMenuItems { get; set; }

    private MainMenuItemModel _mainMenuSelectedItem { get; set; }
    public MainMenuItemModel MainMenuSelectedItem
    {
        get => _mainMenuSelectedItem;
        set
        {
            _mainMenuSelectedItem = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Changelog

    private string _changelogText { get; set; }
    public string ChangelogText
    {
        get => _changelogText;
        set
        {
            _changelogText = value;
            OnPropertyChanged();
        }
    }

    #endregion
    
    #region Versioning
    
    private Visibility _versionUpdateButtonVisibility { get; set; }

    public Visibility VersionUpdateButtonVisibility
    {
        get => _versionUpdateButtonVisibility;
        set
        {
            _versionUpdateButtonVisibility = value;
            OnPropertyChanged();
        }
    }
    private string _versionNumber { get; set; }
    public string VersionNumber
    {
        get => _versionNumber;
        set
        {
            _versionNumber = value;
            OnPropertyChanged();
        }
    }
    private string _versionUpdateText { get; set; }
    public string VersionUpdateText
    {
        get => _versionUpdateText;
        set
        {
            _versionUpdateText = value;
            OnPropertyChanged();
        }
    }
    private string _versionColour { get; set; }
    public string VersionColour
    {
        get => _versionColour;
        set
        {
            _versionColour = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public MainViewModel()
    {
        MainMenuItems = new ObservableCollection<MainMenuItemModel>
        {
            new()
            {
                Title = "Main Menu",
                Info = "",
                View = new MainMenuView(),
                ImageSource = "SolidHome"
            },
            new()
            {
                Title = "Mods",
                Info = "Mod Stuff",
                View = new ModMenuView(),
                ImageSource = "SolidHammer"
            }
        };

        CheckManagerVersion();
        SetupChangelog();
    }
    
    private async void CheckManagerVersion()
    {
        var isManagerUpToDate = await VersionChecker.IsManagerUpToDate();
        VersionColour = isManagerUpToDate ? "#3bff6f" : "Red";
        VersionUpdateText = isManagerUpToDate ? "Up to date!" : "Out of date!";
        VersionUpdateButtonVisibility = isManagerUpToDate ? Visibility.Hidden : Visibility.Visible;
        VersionNumber = $"v{VersionChecker.GetLocalManagerVersion()}";
    }

    private async void SetupChangelog()
    {
        ChangelogText = await VersionChecker.GetChangelog();
    }
}