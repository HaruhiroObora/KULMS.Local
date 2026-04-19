using System;

namespace KULMS.Local.Models;

public class FileModel : FileModelBase
{
    
}

public class URLModel : FileModel
{
    public string URL = string.Empty;
    public string NoExtentionPath { get => (Parent.TrimEnd('/') + "/" + Name).Replace('/', System.IO.Path.DirectorySeparatorChar).TrimStart(System.IO.Path.DirectorySeparatorChar); }
}
