using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using ModManager.MVVM.Model;
using Registry = ModManager.Dto.Constants.Registry;

namespace ModManager.Helper;

public static class Install
{
    public static string GetLocation()
    {
        try
        {
            using var registry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey? registryKey;
            try
            {
                registryKey = registry.OpenSubKey(Registry.GameRegistryKey);
            }
            catch (Exception e)
            {
                //TODO: Show Error
                return string.Empty;
            }

            if (registryKey == null)
                return string.Empty;

            try
            {
                var installLocation = registryKey.GetValue("InstallLocation");
                if (installLocation == null)
                    return string.Empty;

                if (!string.IsNullOrWhiteSpace(installLocation.ToString()))
                    return installLocation.ToString();

                //TODO: Show Error
                return string.Empty;
            }
            catch (Exception e)
            {
                //TODO: Show Error
                return string.Empty;
            }
        }
        catch (Exception e)
        {
            //TODO: Show Error
            return string.Empty;
        }
    }

    public static async Task InstallBepInEx()
    {
        var bepInExDownloadUrl = await GitHub.GetBepInExDownloadUrl();

        if (string.IsNullOrWhiteSpace(bepInExDownloadUrl))
        {
            //TODO: Show error
            return;
        }

        try
        {
            var installLocation = GetLocation();
            var fileName = bepInExDownloadUrl.Split("/").Last();
            await Client.HttpClient.DownloadFileTaskAsync(bepInExDownloadUrl, $@"{installLocation}\{fileName}");
            ZipFile.ExtractToDirectory($@"{installLocation}\{fileName}", installLocation, true);
            await File.WriteAllTextAsync($@"{installLocation}\BepInEx\version.txt",
                await GitHub.GetBepInExLatestVersionNumber());
            File.Delete($@"{installLocation}\{fileName}");
            Directory.CreateDirectory($@"{installLocation}\BepInEx\plugins");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Debug.WriteLine(e);
            //TODO: Show error
        }
    }

    public static bool IsModInstalled(ModItemModel mod)
    {
        var installLocation = GetLocation();
        var pluginsFolder = $@"{installLocation}\BepInEx\plugins";

        return File.Exists($@"{pluginsFolder}\{mod.DllName}");
    }

    public static string? GetInstalledModVersion(string name)
    {
        var dataFolder = Directory.CreateDirectory(GetApplicationDataFolderPath());

        var file = File.Exists($@"{dataFolder}\{name}.txt");

        return file ? File.ReadLines($@"{dataFolder}\{name}.txt").FirstOrDefault() : null;
    }

    private static IEnumerable<string> GetInstalledModInfo(string modName)
        => File.ReadAllLines($@"{GetApplicationDataFolderPath()}\{modName}.txt");

    private static string GetApplicationDataFolderPath()
    {
        var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Directory.CreateDirectory($@"{dataPath}\Mon Bazou Mod Manager");
        return $@"{dataPath}\Mon Bazou Mod Manager";
    }

    private static void WriteInstalledMod(ModItemModel mod, List<string>? folderNames)
    {
        var modData = $"{mod.Version}\n";

        modData += $"{mod.DllName}\n";
        folderNames?.ForEach(x => modData += $"{x}\n");

        File.WriteAllText($@"{GetApplicationDataFolderPath()}\{mod.Name}.txt", modData);
    }

    public static async Task InstallMod(ModItemModel mod)
    {
        try
        {
            // This is due to the ModDb not having correct types -_-
            if (mod.IsZip == "true")
                await InstallZippedMod(mod);
            else
                await InstallNonZippedMod(mod);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Debug.WriteLine(e);
            //TODO: Show error
        }
    }

    private static async Task InstallZippedMod(ModItemModel mod)
    {
        var installLocation = GetLocation();
        await Client.HttpClient.DownloadFileTaskAsync(mod.DownloadUrl,
            $@"{installLocation}\BepInEx\plugins\{mod.ZipName}");
        ZipFile.ExtractToDirectory($@"{installLocation}\BepInEx\plugins\{mod.ZipName}",
            $@"{installLocation}\BepInEx\plugins\", true);
        var files = ZipFile.Open($@"{installLocation}\BepInEx\plugins\{mod.ZipName}", ZipArchiveMode.Read);
        var folderNames = new List<string>();
        foreach (var zipArchiveEntry in files.Entries)
        {
            var startName = zipArchiveEntry.FullName.Split("/")[0];
            if (!folderNames.Contains(startName) && !startName.Contains('.'))
                folderNames.Add(startName);
        }
        
        files.Dispose();
        File.Delete($@"{installLocation}\BepInEx\plugins\{mod.ZipName}");

        WriteInstalledMod(mod, folderNames);
    }

    private static async Task InstallNonZippedMod(ModItemModel mod)
    {
        var installLocation = GetLocation();
        await Client.HttpClient.DownloadFileTaskAsync(mod.DownloadUrl,
            $@"{installLocation}\BepInEx\plugins\{mod.DllName}");

        WriteInstalledMod(mod, null);
    }

    public static void UninstallMod(ModItemModel mod)
    {
        var installLocation = GetLocation();
        var pluginsLocation = $@"{installLocation}\BepInEx\plugins\";
        var dataFolder = GetApplicationDataFolderPath();


        var modInfo = GetInstalledModInfo(mod.Name);
        File.Delete($@"{dataFolder}\{mod.Name}.txt");

        var modDll = modInfo.Skip(1).Take(1).FirstOrDefault();

        File.Delete($@"{pluginsLocation}\{modDll}");

        try
        {
            var modFolders = modInfo.Skip(2);
            foreach (var modFolder in modFolders)
            {
                Directory.Delete($@"{pluginsLocation}\{modFolder}", true);
            }
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}