using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace KULMS.Local.Services;

public class DialogService
{
    public static DialogService Dialog = new();

    private TopLevel? _topLevel;

    private DialogService()
    {    
    }

    public void SetTopLevel(TopLevel? topLevel)
    {
        _topLevel = topLevel;
    }

    public async Task<IStorageFile?> GetFile(FilePickerSaveOptions options)
    {
        return _topLevel is not null ? await _topLevel.StorageProvider.SaveFilePickerAsync(options) : null;
    }
}
