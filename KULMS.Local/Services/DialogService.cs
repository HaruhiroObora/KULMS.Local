using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace KULMS.Local.Services;

public class TopLevelService
{
    public static TopLevelService TopLevelServiceProvider = new();

    private TopLevel? _topLevel;

    private TopLevelService()
    {    
    }

    public void SetTopLevel(TopLevel? topLevel)
    {
        _topLevel = topLevel;
    }

    public async Task<IStorageFile?> GetFileFromDialog(FilePickerSaveOptions options)
    {
        return _topLevel is not null ? await _topLevel.StorageProvider.SaveFilePickerAsync(options) : null;
    }
}
