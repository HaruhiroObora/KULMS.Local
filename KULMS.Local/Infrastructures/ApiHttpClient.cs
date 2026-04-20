using System;

using System.IO;

using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Xml.Linq;

using static KULMS.Local.Services.GlobalSettings;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;

namespace KULMS.Local.Infrastructures;

public class ApiHttpClient
{
    private HttpClient? client;

    // public async Task<bool> SetHttpClient()
    // {
    //     using var playwright = await Playwright.CreateAsync();
    //     IBrowser browser;
    //     try
    //     {
    //         browser = await playwright.Chromium.LaunchAsync(
    //         new()
    //         {
    //             ExecutablePath = GlobalSetting.Settings.BrowserExecutablePath,
    //             Headless = false
    //         });
    //     }
    //     catch
    //     {
    //         return false;
    //     }

    //     var context = await browser.NewContextAsync();
    //     var page = await context.NewPageAsync();
    //     try
    //     {
    //         await page.GotoAsync(GlobalSetting.Settings.Domain + GlobalSetting.Settings.LoginPath);
    //     }
    //     catch
    //     {
    //         return false;
    //     }
    //     try
    //     {
    //         await page.FillAsync("#username_input", GlobalSetting.Settings.ID);
    //         if (GlobalSetting.Settings.ID != "")
    //         {
    //             await page.FocusAsync("#password_input");
    //         }
    //     }
    //     catch
    //     {
    //     }

    //     try
    //     {
    //         await page.WaitForURLAsync(GlobalSetting.Settings.Domain + GlobalSetting.Settings.TopPagePath, new PageWaitForURLOptions { Timeout = 0 });
    //     }
    //     catch
    //     {
    //         return false;
    //     }

    //     var cookies = await context.CookiesAsync();

    //     await browser.CloseAsync();

    //     var handler = new HttpClientHandler
    //     {
    //         CookieContainer = new()
    //     };

    //     client = new HttpClient(handler);

    //     foreach (var c in cookies)
    //     {
    //         var cookie = new System.Net.Cookie(c.Name, c.Value, c.Path, c.Domain);
    //         handler.CookieContainer.Add(cookie);
    //     }

    //     client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

    //     return true;
    // }

    public async Task<bool> SetHttpClientSelenium()
    {
        var options = new ChromeOptions
        {
            BinaryLocation = GlobalSetting.Settings.BrowserExecutablePath
        };

        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal", "ChromeProfile");
        options.AddArgument($"--user-data-dir={appDataPath}");

        using IWebDriver driver = new ChromeDriver(options);

        try
        {
            await driver.Navigate().GoToUrlAsync(GlobalSetting.Settings.Domain + GlobalSetting.Settings.LoginPath);
            try
            {
                // ID入力欄を探す
                var usernameInput = driver.FindElement(By.Name("username"));

                // 値を入力 (FillAsync 相当: 既存のテキストを消してから入力)
                usernameInput.Clear();
                usernameInput.SendKeys(GlobalSetting.Settings.ID);

                // IDが空でない場合、パスワード入力欄にフォーカスを当てる
                if (!string.IsNullOrEmpty(GlobalSetting.Settings.ID))
                {
                    var passwordInput = driver.FindElement(By.Name("password"));

                    // Seleniumで「フォーカスを当てる」には、要素をクリックするか
                    // 空の文字列を送信するのが一般的です
                    passwordInput.Click();
                }
            }
            catch
            {
            }
            var wait = new WebDriverWait(driver, TimeSpan.FromDays(1));
            wait.Until(d => d.Url.StartsWith(GlobalSetting.Settings.Domain + GlobalSetting.Settings.TopPagePath));

            var handler = new HttpClientHandler();
            var seleniumCookies = driver.Manage().Cookies.AllCookies;

            foreach (var seleniumCookie in seleniumCookies)
            {
                var netCookie = new System.Net.Cookie(
                    seleniumCookie.Name,
                    seleniumCookie.Value,
                    seleniumCookie.Path,
                    seleniumCookie.Domain
                );
                handler.CookieContainer.Add(netCookie);
            }

            client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }
        catch
        {
            return false;
        }
        finally
        {
            driver.Quit();
        }
        return true;
    }

    public async Task DownloadAsync(string uriPath, FileStream fileStream)
    {
        if (client is null)
        {
            throw new Exception("Client is not set.");
        }

        var response = await client.GetAsync(GlobalSetting.Settings.Domain + uriPath);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Request failed.");
        }
        var stream = await response.Content.ReadAsStreamAsync();

        await stream.CopyToAsync(fileStream);
        fileStream.Close();
    }

    public async IAsyncEnumerable<XElement> GetXmlAsync(string uriPath, string name)
    {
        if (client is null)
        {
            throw new Exception("Client is not set.");
        }
        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(GlobalSetting.Settings.Domain + uriPath);
        }
        catch
        {
            throw new Exception();
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Request failed.");
        }
        var xmlStream = await response.Content.ReadAsStreamAsync();
        var elements = XDocument.Load(xmlStream).Descendants(name);

        foreach (var e in elements)
        {
            if (e is not null)
            {
                yield return e;
            }
        }
    }
}
