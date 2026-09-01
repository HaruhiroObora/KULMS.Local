using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Services.SyncService;
using static KULMS.Local.Services.TopLevelService;
using static KULMS.Local.Services.GlobalSettings;
using KULMS.Local.Services;
using Avalonia.Input;

namespace KULMS.Local.ViewModels;

public partial class FileViewModel : FileViewModelBase
{
    public FileModel FileModel = null!;

    public string? Name { get => FileModel?.Name; }
    public string? Type { get => FileModel?.Type; }
    public DateTime? LastModified { get => FileModel?.LastModified; }
    public Status? DownloadStatus { get => FileModel?.DownloadStatus; }

    public TopLevel? topLevel;

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

    [RelayCommand]
    public void OpeninFileApp()
    {
        Syncer.OpeninFileApp(FileModel);
    }

    [RelayCommand]
    public async Task SaveFileAsync()
    {
        // 現在のウィンドウの StorageProvider を取得
        var options = new FilePickerSaveOptions
        {
            Title = "名前をつけて保存",
            DefaultExtension = FileModel.Type,
            SuggestedFileName = Path.GetFileName(FileModel.Path),
        };

        // ダイアログを表示して結果を受け取る
        var file = await TopLevelServiceProvider.GetFileFromDialog(options);

        if (file != null)
        {
            await Syncer.Download(FileModel, file.Path.LocalPath, false);
        }
    }

    public async Task DoDragAsync(PointerPressedEventArgs e)
    {
        if (FileModel.DownloadStatus != Status.Offline)
        {
            return;
        }
        var path = Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, FileModel.Path);
        var file = await TopLevelServiceProvider.GetFileFromPath(path);
        if (file != null)
        {
            var dragData = new DataTransfer();
            dragData.Add(DataTransferItem.CreateFile(file));
            var result = await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Copy|DragDropEffects.Move|DragDropEffects.Link);
        }
    }
}
