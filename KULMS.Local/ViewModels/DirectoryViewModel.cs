using System;
using KULMS.Local.Models;

namespace KULMS.Local.ViewModels;

public class DirectoryViewModel(DirectoryModel model, ContentsTabViewModel contentsTab) : FileViewModelBase
{
    public DirectoryModel DirectoryModel = model;

    public readonly ContentsTabViewModel contentsTab = contentsTab;

    public string? Name { get => DirectoryModel?.Name; }
    public string? Type { get => DirectoryModel?.Type; }
    public DateTime? LastModified { get => DirectoryModel?.LastModified; }
    public Status? DownloadStatus { get => DirectoryModel?.DownloadStatus; }
}
