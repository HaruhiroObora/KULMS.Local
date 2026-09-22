using System;
using static KULMS.Local.Services.SyncService;

namespace KULMS.Local.Models;

public class FileModel : FileModelBase
{
    
}

public class URLModel : FileModel
{
    public string URL = string.Empty;
    public string NoExtentionPath { get => SanitizeFileName((Parent.TrimEnd('/') + "/" + Name).Replace('/', System.IO.Path.DirectorySeparatorChar).TrimStart(System.IO.Path.DirectorySeparatorChar)); }
}
