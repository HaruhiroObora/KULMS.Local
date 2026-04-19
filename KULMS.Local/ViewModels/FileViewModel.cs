using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Services.SyncService;

namespace KULMS.Local.ViewModels;

public partial class FileViewModel : FileViewModelBase
{
    public FileModel FileModel = null!;

    public string? Name { get => FileModel?.Name; }
    public string? Type { get => FileModel?.Type; }
    public DateTime? LastModified { get => FileModel?.LastModified; }
    public Status? DownloadStatus { get => FileModel?.DownloadStatus; }

    public FileViewModel(FileModel model)
    {
        FileModel = model;

        model.PropertyChanged += (s, e) => OnPropertyChanged(nameof(DownloadStatus));
    }

    [RelayCommand]
    public async Task Open()
    {
        if (!(DownloadStatus == Status.Offline))
                {
                    await Download();
                }
                if (DownloadStatus == Status.Offline)
                {
                    Syncer.OpenFile(FileModel);
                }
    }

    [RelayCommand]
    public async Task Download()
    {
        await Syncer.Download(FileModel);
    }
}
