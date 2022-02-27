using System.ComponentModel;
using ModManager.Core;
using Newtonsoft.Json;

namespace ModManager.MVVM.Model;

public class ModItemModel: ObservableObject
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("author")]
    public string Author { get; set; }

    [JsonProperty("imageurl")]
    public string ImageUrl { get; set; }

    [JsonProperty("downloadurl")]
    public string DownloadUrl { get; set; }

    [JsonProperty("dllname")]
    public string DllName { get; set; }

    [JsonProperty("zipname")]
    public string ZipName { get; set; }

    [JsonProperty("isZip")]
    public string IsZip { get; set; }

    [JsonProperty("disabled")]
    public string Disabled { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("gameversion")]
    public string GameVersion { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("releasedate")]
    public string ReleaseDate { get; set; }
    
    private string _infoText { get; set; }

    public string InfoText
    {
        get => _infoText;
        set
        {
            _infoText = value;
            OnPropertyChanged();
        }
    }
    
    private string _infoTextColour { get; set; }

    public string InfoTextColour
    {
        get => _infoTextColour;
        set
        {
            _infoTextColour = value;
            OnPropertyChanged();
        }
    }
}