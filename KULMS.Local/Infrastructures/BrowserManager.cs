using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using static KULMS.Local.Services.GlobalSettings;

namespace KULMS.Local.Infrastructures;

public static class WebDriverExtensions
{
    public static bool IsClosed(this IWebDriver driver)
    {
        try
        {
            return driver.WindowHandles.Count == 0;
        }
        catch (Exception)
        {
            return true;
        }
    }
}

public class BrowserManager
{
    public readonly static BrowserManager Browser = new();

    private ReadOnlyCollection<Cookie>? cookies;

    private IWebDriver? driver;

    private BrowserManager()
    {
    }

    private IWebDriver CreateDriver()
    {
        var options = new ChromeOptions
        {
            BinaryLocation = GlobalSetting.Settings.BrowserExecutablePath
        };

        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal", "ChromeProfile");
        options.AddArgument($"--user-data-dir={appDataPath}");

        return new ChromeDriver(options);
    }

    public IWebDriver GetDriver()
    {
        if (driver is null || driver.IsClosed())
        {
            driver = CreateDriver();
        }
        return driver;
    }

    public bool WindowExists()
    {
        return !(driver?.IsClosed() ?? true);
    }

    public void SaveCookies()
    {
        try
        {
            cookies = driver?.Manage().Cookies.AllCookies;
        }
        catch
        {
        }
    }

    public void ApplyCookies(List<string>? skip = null)
    {
        try
        {
            foreach (var c in cookies ?? [])
            {
                if (skip is null)
                {
                    driver?.Manage().Cookies.AddCookie(c);
                }
                else if (!skip.Contains(c.Name))
                {
                    driver?.Manage().Cookies.AddCookie(c);
                }
            }
        }
        catch
        {
        }
    }

    public void DeleteCookie(string name)
    {
        try
        {
            driver?.Manage().Cookies.DeleteCookieNamed(name);
        }
        catch
        {
        }
    }
}
