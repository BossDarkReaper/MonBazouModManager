using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModManager.Dto.Constants;
using ModManager.MVVM.Model;
using Newtonsoft.Json;
using Octokit;

namespace ModManager.Helper;

public static class GitHub
{
    #region BepInEx

    public static async Task<bool> IsLocalVersionLatestBepInEx(string localVersion)
    {
        try
        {
            var releases = await GetBepInExReleases();
    
            //Setup the versions
            var latestGitHubVersion = new Version(releases[0].TagName.Replace("v", string.Empty));
            var version = new Version(localVersion.Replace("v", string.Empty));

            //Compare the Versions
            var versionComparison = version.CompareTo(latestGitHubVersion);
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
        catch (Exception e)
        {
            //TODO: show error | failed to get update try again later
            return true;
        }
    }

    public static async Task<string> GetBepInExLatestVersionNumber()
    {
        var releases = await GetBepInExReleases();
        return releases[0].TagName;
    }
    
    public static async Task<string?> GetBepInExDownloadUrl()
    {
        var releases = await GetBepInExReleases();
        var release = releases[0].Assets.FirstOrDefault(x => x.Name.Contains("x64"));
        
        return release?.BrowserDownloadUrl;
    }

    private static async Task<IReadOnlyList<Release>> GetBepInExReleases()
    {
        var client = new GitHubClient(new ProductHeaderValue("ModManager"));
        return await client.Repository.Release.GetAll("BepInEx", "BepInEx");
    }

    #endregion

    public static async Task<List<ModItemModel>> GetModListData()
    {
        var modResponse = await Client.HttpClient.GetStringAsync(Mod.ModJsonUrl);

        try
        {
            var modList = JsonConvert.DeserializeObject<List<ModItemModel>>(modResponse);
            return modList ?? new List<ModItemModel>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //TODO: Show error
            return new List<ModItemModel>();
        }
    }
}