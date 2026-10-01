using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KULMS.Local.Models;

public enum Status
{
    Cloud,
    Offline,
    Folder,
    Unknown,
    Failed,
    Downloading
}

public partial class FileModelBase : ObservableObject
{ 
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    
    [ObservableProperty]
    public Status downloadStatus = Status.Unknown;

    public string UrlPath { get; set; } = string.Empty;
    public string Parent { get; set; } = string.Empty;
    public string Path { get => (Parent.TrimEnd('/') + "/" + Name + (Type == string.Empty ? "" : $".{Type}")).Replace('/', System.IO.Path.DirectorySeparatorChar).TrimStart(System.IO.Path.DirectorySeparatorChar); }
}
