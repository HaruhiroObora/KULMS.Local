using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using KULMS.Local.Models;

using static KULMS.Local.Infrastructures.BrowserManager;
using static KULMS.Local.Services.GlobalSettings;

namespace KULMS.Local.ViewModels;

public class AssignmentViewModel : ViewModelBase
{
    public AssignmentModel AssignmentModel = null!;

    public string Title => AssignmentModel.Title;

    public DateTime Due => AssignmentModel.DueDate;
    
    public SubmissionStatus SubmissionStatus => AssignmentModel.SubmissionStatus;

    public async Task OpenPage()
    {
        var driver = Browser.GetDriver();
        await driver.Navigate().GoToUrlAsync(GlobalSetting.Settings.Domain);
        Browser.ApplyCookies();
        await driver.Navigate().GoToUrlAsync(AssignmentModel.Url);
    }
}
