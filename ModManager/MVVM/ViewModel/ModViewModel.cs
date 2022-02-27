using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using ModManager.Core;
using ModManager.Helper;
using ModManager.MVVM.Model;
using Process = System.Diagnostics.Process;

namespace ModManager.MVVM.ViewModel;

public class ModViewModel : ObservableObject
{
    #region Commands

    public RelayCommand RefreshModsCommand { get; set; }
    public RelayCommand InstallUninstallCommand { get; set; }
    public RelayCommand SelectedModCommand { get; set; }
    public RelayCommand OpenConfigFolderCommand { get; set; }
    public RelayCommand InstallBepInExCommand { get; set; }

    #endregion

    #region Mod Items

    public ObservableCollection<ModItemModel> ModItems { get; set; }
    private ModItemModel _modSelectedItem { get; set; }

    public ModItemModel ModSelectedItem
    {
        get => _modSelectedItem;
        set
        {
            _modSelectedItem = value;
            OnPropertyChanged();
        }
    }

    private string _modSelectedItemInstallText { get; set; }

    public string ModSelectedItemInstallText
    {
        get => _modSelectedItemInstallText;
        set
        {
            _modSelectedItemInstallText = value;
            OnPropertyChanged();
        }
    }

    private Visibility _modSelectedItemInstallButtonVisibility { get; set; }

    public Visibility ModSelectedItemInstallButtonVisibility
    {
        get => _modSelectedItemInstallButtonVisibility;
        set
        {
            _modSelectedItemInstallButtonVisibility = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Mod Visibility

    private Visibility _modsVisibility { get; set; }

    public Visibility ModsVisibility
    {
        get => _modsVisibility;
        set
        {
            _modsVisibility = value;
            OnPropertyChanged();
        }
    }

    private Visibility _bepInExInstallVisibility { get; set; }

    public Visibility BepInExInstallVisibility
    {
        get => _bepInExInstallVisibility;
        set
        {
            _bepInExInstallVisibility = value;
            OnPropertyChanged();
        }
    }

    private bool _disableBepInExInstallButton { get; set; }

    public bool DisableBepInExInstallButton
    {
        get => _disableBepInExInstallButton;
        set
        {
            _disableBepInExInstallButton = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public ModViewModel()
    {
        ModItems = new ObservableCollection<ModItemModel>();
        _modsVisibility = Visibility.Hidden;
        DisableBepInExInstallButton = !string.IsNullOrWhiteSpace(Install.GetLocation());

        #region Setup Commands

        RefreshModsCommand = new RelayCommand(c => { SetupModList(); });

        InstallBepInExCommand = new RelayCommand(async c =>
        {
            DisableBepInExInstallButton = true;
            await Install.InstallBepInEx();
            CheckForBepInEx();
        });

        OpenConfigFolderCommand = new RelayCommand(c =>
        {
            var installLocation = Install.GetLocation();
            var bepInExDirectory = $@"{installLocation}\BepInEx";
            var configPath = $@"{bepInExDirectory}\config";
            if (!Directory.Exists(configPath)) return;
            using var process = new Process();
            process.StartInfo.FileName = configPath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        });

        SelectedModCommand = new RelayCommand(c =>
        {
            var isModInstalled = Install.IsModInstalled(ModSelectedItem);
            ModSelectedItemInstallText = isModInstalled ? "Uninstall" : "Install";
        });

        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != "ModSelectedItem") return;
            if (ModSelectedItem == null || ModSelectedItem.Disabled == "true")
            {
                ModSelectedItemInstallText = string.Empty;
                ModSelectedItemInstallButtonVisibility = Visibility.Hidden;
                return;
            }

            ModSelectedItemInstallButtonVisibility = Visibility.Visible;
            var isModInstalled = Install.IsModInstalled(ModSelectedItem);

            if (isModInstalled && ModSelectedItem.InfoText.Contains("Update"))
            {
                ModSelectedItemInstallText = "Update";
                return;
            }
            
            ModSelectedItemInstallText = isModInstalled ? "Uninstall" : "Install";
        };

        InstallUninstallCommand = new RelayCommand(async c =>
        {
            // This is in case the user clicks another item before install is finished
            var selectedItem = ModSelectedItem;
            
            var isModInstalled = Install.IsModInstalled(selectedItem);
            if (!isModInstalled)
                await Install.InstallMod(selectedItem);
            else
            {
                if (selectedItem.InfoText.Contains("Update"))
                {
                    Install.UninstallMod(selectedItem);
                    await Install.InstallMod(selectedItem);
                }
                else
                {
                    Install.UninstallMod(selectedItem);
                }
            }

            isModInstalled = Install.IsModInstalled(selectedItem);
            var index = ModItems.IndexOf(selectedItem);
            ModItems[index].InfoText = isModInstalled ? "Installed" : string.Empty;
            ModItems[index].InfoTextColour = "#3bff6f";
            if (ModSelectedItem == selectedItem)
                ModSelectedItemInstallText = isModInstalled ? "Uninstall" : "Install";
        });

        #endregion

        SetupModList();
        CheckForBepInEx();
    }

    private async void SetupModList()
    {
        var modListData = await GitHub.GetModListData();
        ModItems.Clear();
        foreach (var mod in modListData)
        {
            if (mod.Disabled == "true")
            {
                mod.InfoText = "Disabled";
                mod.InfoTextColour = "Red";
                mod.Reason = $"Disabled Reason\n{mod.Reason}";
            }
            else
            {
                var modInstalled = Install.IsModInstalled(mod);
                mod.InfoText = modInstalled ? "Installed" : string.Empty;
                mod.InfoTextColour = "#3bff6f";

                if (modInstalled)
                {
                    var localVersion = Install.GetInstalledModVersion(mod.Name);
                    if (string.IsNullOrWhiteSpace(localVersion))
                    {
                        //TODO: Show error | unable to get local version needs reinstall of mod
                        continue;
                    }

                    var localModVersion = new Version(localVersion);
                    var remoteModVersion = new Version(mod.Version);
                    
                    var versionComparison = localModVersion.CompareTo(remoteModVersion);
                    var isLocalVersionLatest = versionComparison switch
                    {
                        < 0 =>
                            //The remote version is more up to date than this local version
                            false,
                        > 0 =>
                            //This local version is greater than the remote version
                            true,
                        _ => true
                    };

                    if (!isLocalVersionLatest)
                    {
                        mod.InfoText = "Update\nAvailable";
                        mod.InfoTextColour = "#FEE75C";
                    }
                }
            }
            ModItems.Add(mod);
        }
    }

    private async void CheckForBepInEx()
    {
        var installLocation = Install.GetLocation();

        if (string.IsNullOrWhiteSpace(installLocation))
        {
            //TODO: Show Error
            return;
        }

        var bepInExDirectory = $@"{installLocation}\BepInEx";

        if (!Directory.Exists(bepInExDirectory))
        {
            //TODO: Show Error | Install BepInEx (Show install button)
            return;
        }

        var versionFile = $@"{bepInExDirectory}\version.txt";

        if (!File.Exists(versionFile))
        {
            //TODO: Error
            return;
        }

        var version = await File.ReadAllTextAsync(versionFile);
        if (await GitHub.IsLocalVersionLatestBepInEx(version))
        {
            ModsVisibility = Visibility.Visible;
            BepInExInstallVisibility = Visibility.Hidden;
        }
        else
        {
            // Update BepInEx
            await Install.InstallBepInEx();
            ModsVisibility = Visibility.Visible;
            BepInExInstallVisibility = Visibility.Hidden;
        }
    }
}