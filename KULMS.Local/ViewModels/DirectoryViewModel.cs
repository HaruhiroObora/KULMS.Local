using System;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Services.SyncService;

namespace KULMS.Local.ViewModels;

public partial class DirectoryViewModel(DirectoryModel model, ContentsTabViewModel contentsTab) : FileViewModelBase
{
    public DirectoryModel DirectoryModel = model;

    public readonly ContentsTabViewModel contentsTab = contentsTab;

    public string? Name { get => DirectoryModel?.Name; }
    public string? Type { get => DirectoryModel?.Type; }
    public DateTime? LastModified { get => DirectoryModel?.LastModified; }
    public Status? DownloadStatus { get => DirectoryModel?.DownloadStatus; }

    [RelayCommand]
    public void OpeninFileApp()
    {
        Syncer.OpeninFileApp(DirectoryModel);
    }
}
