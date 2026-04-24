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

public enum SubmissionStatus
{
    NotStarted,
    UnderWay,
    Submitted,
    Returned,
    ResubmissionRequired,
    UNKNOWN
}

public class AssignmentModel : ObservableObject
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SiteId { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.UNKNOWN;
    public SubmissionStatus SubmissionStatus { get; set; } = SubmissionStatus.UNKNOWN;

    public static AssignmentStatus AssignmentStatusFromString(string status)
    {
        if (Enum.TryParse<AssignmentStatus>(status, true, out var e))
        {
            return e;
        }
        else
        {
            return AssignmentStatus.UNKNOWN;
        }
    }

    public static SubmissionStatus SubmissionStatusFromString(string status)
    {
        if (status == "未開始") return SubmissionStatus.NotStarted;
        else if (status == "取組中") return SubmissionStatus.UnderWay;
        else if (status == "提出済み") return SubmissionStatus.Submitted;
        else if (status.Contains("要再提出")) return SubmissionStatus.ResubmissionRequired;
        else if (status.Contains("返却済み")) return SubmissionStatus.Returned;
        else return SubmissionStatus.UNKNOWN;
    }
}
