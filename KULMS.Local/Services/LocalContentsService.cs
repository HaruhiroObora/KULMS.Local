using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using KULMS.Local.Models;


namespace KULMS.Local.Services;

public interface ILocalContentsService
{
    public IAsyncEnumerable<SiteModel> FilterSites(IAsyncEnumerable<SiteModel> sites);

    public void UpdateSiteFilter(string id, bool show);
}

public class LocalContentsService : ILocalContentsService
{
    public static ILocalContentsService LocalService { get; } = new LocalContentsService();

    private Dictionary<string, bool> siteFilter = [];

    private static readonly JsonSerializerOptions options = new(){ WriteIndented = true };

    private LocalContentsService()
    {
        LoadFilter();
    }
    
    public async IAsyncEnumerable<SiteModel> FilterSites(IAsyncEnumerable<SiteModel> sites)
    {
        await foreach (var s in sites)
        {
            if (siteFilter.ContainsKey(s.Id))
            {
                if (siteFilter[s.Id])
                {
                    s.Shown = true;
                    yield return s;
                }
                else
                {
                    s.Shown = false;
                }
            }
            else
            {
                UpdateSiteFilter(s.Id, false);
                s.Shown = false;
            }
        }
    }

    public async void UpdateSiteFilter(string id, bool show)
    {
        siteFilter[id] = show;
        await SaveFilter();
    }

    private void LoadFilter()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal", "sitefilter.json");

        if (!Path.Exists(path))
        {
            return;
        }

        using (var jsonStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            siteFilter = JsonSerializer.Deserialize<Dictionary<string, bool>>(jsonStream) ?? [];
        }
    }

    private async Task SaveFilter()
    {
        var dirpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal");
        if (!Path.Exists(dirpath))
        {
            Directory.CreateDirectory(dirpath);
        }
        var path = Path.Combine(dirpath, "sitefilter.json");

        using (var jsonStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            await JsonSerializer.SerializeAsync(jsonStream, siteFilter, options);
        }
    }
}