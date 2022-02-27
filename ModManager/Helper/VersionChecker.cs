using System;
using System.Reflection;
using System.Threading.Tasks;
using ModManager.Dto.Constants;

namespace ModManager.Helper;

public static class VersionChecker
{
    public static async Task<bool> IsManagerUpToDate()
    {
        var localVersion = GetLocalManagerVersion();
        var remoteVersion = await GetLatestManagerVersion();
        var versionComparison = string.Compare(localVersion, remoteVersion, StringComparison.Ordinal);
        return versionComparison switch
        {
            < 0 =>
                //The version on GitHub is more up to date than this local release.
                false,
            > 0 =>
                //This local version is greater than the release version on GitHub.
                true,
            _ => true
        };
    }

    public static async Task<string> GetLatestManagerVersion() =>
        await Client.HttpClient.GetStringAsync(Changelog.Url + Changelog.Latest);
    
    public static string GetLocalManagerVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    public static async Task<string> GetChangelog() =>
        await Client.HttpClient.GetStringAsync(Changelog.Url + Changelog.Base);
}