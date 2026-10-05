using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using KULMS.Local.Models;

namespace KULMS.Local.Services;

public class AssignmentService
{
    public static AssignmentService AssignmentManager = new();

    private Dictionary<string, bool> submittionFilter = [];

    public event Action? AssignmentStateUpdated;

    private static readonly JsonSerializerOptions options = new(){ WriteIndented = true };

    private AssignmentService()
    {
        LoadFilter();
    }

    public async IAsyncEnumerable<AssignmentModel> Filter(IAsyncEnumerable<AssignmentModel> assignments, Func<AssignmentModel, bool> key)
    {
        await foreach (var a in assignments)
        {
            if (submittionFilter.GetValueOrDefault(a.Id, key(a)))
            {
                yield return a;
            }
        }
    }

    public async void UpdateAssignmentFilter(string id, bool show)
    {
        if (submittionFilter.ContainsKey(id))
        {
            submittionFilter[id] = !submittionFilter[id];
        }
        else
        {
            submittionFilter[id] = show;
        }
        await SaveFilter();
        AssignmentStateUpdated?.Invoke();
    }

    private void LoadFilter()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal", "assignmentfilter.json");

        if (!Path.Exists(path))
        {
            return;
        }

        using (var jsonStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            submittionFilter = JsonSerializer.Deserialize<Dictionary<string, bool>>(jsonStream) ?? [];
        }
    }

    private async Task SaveFilter()
    {
        var dirpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal");
        if (!Path.Exists(dirpath))
        {
            Directory.CreateDirectory(dirpath);
        }
        var path = Path.Combine(dirpath, "assignmentfilter.json");

        using (var jsonStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            await JsonSerializer.SerializeAsync(jsonStream, submittionFilter, options);
        }
    }
}
