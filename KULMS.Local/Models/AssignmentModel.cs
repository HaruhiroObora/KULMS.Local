using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KULMS.Local.Models;

public enum AssignmentStatus
{
    OPEN,
    CLOSED,
    DUE,
    UNKNOWN
}

public class AssignmentModel : ObservableObject
{
    public string Title { get; set; } = string.Empty;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.UNKNOWN;
    
}
