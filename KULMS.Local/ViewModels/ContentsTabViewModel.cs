using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Services.KULMSApiService;
using static KULMS.Local.Services.SyncService;

namespace KULMS.Local.ViewModels;

public partial class ContentsTabViewModel : ViewModelBase
{
    public ObservableCollection<SiteModel> Sites { get; } = new();
    public ObservableCollection<FileViewModelBase> Files { get; } = new();

    [ObservableProperty]
    public FileViewModelBase? selectedFile;

    [ObservableProperty]
    public string? currentDirectory;

    [ObservableProperty]
    public SiteModel? selectedSite;

    public SiteModel? browsedSite;

    public DirectoryViewModel? browsedDirectory;

    private CancellationTokenSource? _cts;

    public ContentsTabViewModel()
    {
        _ = Login();
        StartUIRefresh();
    }

    public async Task Login()
    {
        if (!KULMSApi.LoginStatus)
        {
            await KULMSApi.Login();
        }
        List<SiteModel> sites;
        try
        {
            sites = await KULMSApi.GetSites().ToListAsync(CancellationToken.None);
            foreach (var s in sites)
            {
                int idx = 0;
                foreach (var site in Sites)
                {
                    if (string.Compare(WeekDayToInt(site.Title), WeekDayToInt(s.Title)) == 1)
                    {
                        break;
                    }
                    idx++;
                }
                await Dispatcher.UIThread.InvokeAsync(() => Sites.Insert(idx, s));
            }
        }
        catch
        {
        }
    }

    public void StartUIRefresh()
    {
        _cts = new CancellationTokenSource();
        // 戻り値を待たずに非同期ループを開始
        _ = PeriodicUIRefresh(_cts.Token);
    }

    private async Task PeriodicUIRefresh(CancellationToken ct)
    {
        List<SiteModel> sites;
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                sites = await KULMSApi.GetSites(false).ToListAsync(CancellationToken.None);

                List<SiteModel> removeSites = [];

                foreach (var s in Sites)
                {
                    if (!sites.Any(x => x.Id == s.Id))
                    {
                        removeSites.Add(s);
                    }
                    else
                    {
                        sites.RemoveAll(x => x.Id == s.Id);
                    }
                }
                foreach (var s in removeSites)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => Sites.Remove(s));
                }
                foreach (var s in sites)
                {
                    int idx = 0;
                    foreach (var site in Sites)
                    {
                        if (string.Compare(WeekDayToInt(site.Title), WeekDayToInt(s.Title)) == 1)
                        {
                            break;
                        }
                        idx++;
                    }
                    await Dispatcher.UIThread.InvokeAsync(() => Sites.Insert(idx, s));
                }
            }
            catch
            {
            }
        }
    }

    [RelayCommand]
    public async Task ListEnterPressed()
    {
        if (SelectedFile is DirectoryViewModel directory)
        {
            CurrentDirectory = directory.DirectoryModel.Path;
            browsedDirectory = directory;
            _ = ChangeDirectory();
        }
        else if (SelectedFile is FileViewModel file)
        {
            _ = file.Open();
        }
    }

    [RelayCommand]
    public async Task GoToUpperDirectory()
    {
        if (Path.GetDirectoryName(CurrentDirectory) == "")
        {
            return;
        }
        CurrentDirectory = Path.GetDirectoryName(CurrentDirectory);
        var dir = await KULMSApi.GetDirectory(browsedSite!, false, CurrentDirectory!);
        browsedDirectory = dir is not null ? new DirectoryViewModel(dir, this) : null;
        await ChangeDirectory();
    }

    [RelayCommand]
    public void DownloadCurrentDirectory()
    {
        if (browsedDirectory is null || browsedSite is null)
        {
            return;
        }
        _ = Syncer.DownloadAll(browsedSite, browsedDirectory.DirectoryModel);
    }

    [RelayCommand]
    public async Task RefreshDirectory()
    {
        await ChangeDirectory(null, true);
    }

    [RelayCommand]
    public void DownloadAll()
    {
        if (browsedDirectory is null || browsedSite is null)
        {
            return;
        }
        _ = Syncer.DownloadAll(browsedSite, browsedDirectory.DirectoryModel, true);
    }

    [RelayCommand]
    public async Task ChangeDirectory()
    {
        await ChangeDirectory(null, false);
    }

    public async Task ChangeDirectory(IAsyncEnumerable<FileModelBase>? files = null, bool refresh = false)
    {
        if (CurrentDirectory is null)
        {
            return;
        }
        if (files is null)
        {
            if (browsedSite is not null)
            {
                try
                {
                    files = KULMSApi.GetFiles(browsedSite, refresh);
                }
                catch
                {
                    await Dispatcher.UIThread.InvokeAsync(Files.Clear);
                    return;
                }
            }
            else
            {
                return;
            }
        }
        if (browsedDirectory is null)
        {
            var dir = await KULMSApi.GetDirectory(browsedSite!, false);
            browsedDirectory = dir is not null ? new DirectoryViewModel(dir, this) : null;
        }
        var filtered = Syncer.DirectoryFilter(files, CurrentDirectory);
        await Syncer.CheckDownloaded(filtered);
        await Dispatcher.UIThread.InvokeAsync(Files.Clear);
        await foreach (var f in filtered)
        {
            if (f is FileModel fileModel)
            {
                await Dispatcher.UIThread.InvokeAsync(() => Files.Add(new FileViewModel(fileModel)));
            }
            else if (f is DirectoryModel directoryModel)
            {
                await Dispatcher.UIThread.InvokeAsync(() => Files.Add(new DirectoryViewModel(directoryModel, this)));
            }
        }
        SelectedFile = null;
    }

    public void StopService()
    {
        _cts?.Cancel();
    }

    private string WeekDayToInt(string origin)
    {
        return origin.Replace("月", "0").Replace("火", "1").Replace("水", "2").Replace("木", "3").Replace("金", "4").Replace("前", "0").Replace("後", "1");
    }
}
