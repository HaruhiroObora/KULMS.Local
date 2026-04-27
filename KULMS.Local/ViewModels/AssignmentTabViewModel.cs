using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Services.KULMSApiService;

namespace KULMS.Local.ViewModels;

public partial class AssignmentTabViewModel : ViewModelBase
{
    public ObservableCollection<AssignmentViewModel> Assignments { get; } = [];
    
    [ObservableProperty]
    public partial AssignmentViewModel? SelectedAssignment { get; set; }

    [RelayCommand]
    public async Task LoadAssignments()
    {
        var assignments = KULMSApi.GetAssignments();
    }
}
