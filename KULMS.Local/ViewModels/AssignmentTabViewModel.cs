using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Services.KULMSApiService;
using static KULMS.Local.Services.AssignmentService;
using Avalonia.Threading;
using System.Threading;

using System.Linq;

namespace KULMS.Local.ViewModels;

public partial class AssignmentTabViewModel : ViewModelBase
{
    public ObservableCollection<AssignmentViewModel> Assignments { get; } = [];

    [ObservableProperty]
    public partial AssignmentViewModel? SelectedAssignment { get; set; }

    [ObservableProperty]
    public partial bool NotStartedOnly { get; set; } = true;

    public DateTime LastUpdated => KULMSApi.assignmentsUpdate;

    public AssignmentTabViewModel()
    {
        KULMSApi.AssignmentsUpdated += async () => await LoadAssignments(false);
    }

    [RelayCommand]
    public async Task Reload()
    {
        await LoadAssignments();
    }

    public async Task LoadAssignments(bool refresh = true)
    {
        var assignments = KULMSApi.GetAssignments(refresh: refresh);
        if (NotStartedOnly)
        {
            assignments = AssignmentManager.Filter(assignments, a => a.SubmissionStatus == SubmissionStatus.NotStarted || a.SubmissionStatus == SubmissionStatus.UnderWay);
        }

        var assignmentsList = await assignments.ToListAsync(CancellationToken.None);

        await Dispatcher.UIThread.InvokeAsync(Assignments.Clear);

        foreach (var assignment in assignmentsList)
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
                OnPropertyChanged(nameof(LastUpdated));
            }
        }
    }
}
