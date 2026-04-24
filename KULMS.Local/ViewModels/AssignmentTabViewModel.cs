using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KULMS.Local.ViewModels;

public partial class AssignmentTabViewModel : ViewModelBase
{
    public ObservableCollection<AssignmentViewModel> Assignments { get; } = [];
    
    [ObservableProperty]
    public partial AssignmentViewModel? SelectedAssignment { get; set; }
}
