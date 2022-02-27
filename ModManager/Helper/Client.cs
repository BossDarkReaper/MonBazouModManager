using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModManager.Helper;

public static class Client
{
    public static HttpClient HttpClient = new HttpClient();

    public static async Task DownloadFileTaskAsync(this HttpClient client, string uri, string fileName)
    {
        await using var s = await client.GetStreamAsync(uri);
        await using var fs = new FileStream(fileName, FileMode.CreateNew);
        await s.CopyToAsync(fs);
    }
}