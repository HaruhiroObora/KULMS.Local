using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Services.KULMSApiService;
using static KULMS.Local.Services.AssignmentService;
using Avalonia.Threading;

namespace KULMS.Local.ViewModels;

public partial class AssignmentTabViewModel : ViewModelBase
{
    public ObservableCollection<AssignmentViewModel> Assignments { get; } = [];

    [ObservableProperty]
    public partial AssignmentViewModel? SelectedAssignment { get; set; }

    public AssignmentTabViewModel()
    {
        _ = LoadAssignments();
    }

    [RelayCommand]
    public async Task LoadAssignments()
    {
        var assignments = AssignmentManager.Filter(KULMSApi.GetAssignments(), a => true);

        await Dispatcher.UIThread.InvokeAsync(Assignments.Clear);

        await foreach (var assignment in assignments)
        {
            {
                int idx = 0;
                foreach (var a in Assignments)
                {
                    if (assignment.DueDate < a.AssignmentModel.DueDate)
                    {
                        break;
                    }
                    idx++;
                }
                await Dispatcher.UIThread.InvokeAsync(() => Assignments.Insert(idx, new AssignmentViewModel(assignment)));
            }
        }
    }
}
