using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KULMS.Local.Models;

public partial class SiteModel : ObservableObject
{
    public string Title { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool? Shown { get; set; } = false;
}
