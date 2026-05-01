using System;
using System.Threading.Tasks;
using KULMS.Local.Models;

using static KULMS.Local.Infrastructures.BrowserManager;
using static KULMS.Local.Services.KULMSApiService;
using static KULMS.Local.Services.GlobalSettings;

namespace KULMS.Local.ViewModels;

public class AssignmentViewModel(AssignmentModel model) : ViewModelBase
{
    public AssignmentModel AssignmentModel = model;

    public string Title => AssignmentModel.Title;

    public string Site => KULMSApi.SearchSiteFromId(AssignmentModel.SiteId)?.Title ?? string.Empty;

    public DateTime Due => AssignmentModel.DueDate;
    
    public SubmissionStatus SubmissionStatus => AssignmentModel.SubmissionStatus;

    public async Task OpenPage()
    {
        var browserState = Browser.WindowExists();
        var driver = Browser.GetDriver();
        await driver.Navigate().GoToUrlAsync(GlobalSetting.Settings.Domain);
        Browser.ApplyCookies();
        await driver.Navigate().GoToUrlAsync(GlobalSetting.Settings.Domain + GlobalSetting.Settings.LoginPath);
        await driver.Navigate().GoToUrlAsync(AssignmentModel.Url);
    }
}
