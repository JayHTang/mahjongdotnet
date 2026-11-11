using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Mahjong.Tests.Controllers;

public class IsolatedWebAppFixture : IDisposable
{
    public string TempDir { get; }
    public WebApplicationFactory<Program> Factory { get; }

    public IsolatedWebAppFixture()
    {
        TempDir = Path.Combine(Path.GetTempPath(), $"mahjong_http_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(TempDir);

        string templateDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Mahjong/App_Data/db"));
        string mahjongDb = Path.Combine(TempDir, "Mahjong.db");
        string authDb = Path.Combine(TempDir, "Authentication.db");

        File.Copy(Path.Combine(templateDir, "Mahjong.db.template"), mahjongDb, overwrite: true);
        File.Copy(Path.Combine(templateDir, "Authentication.db.template"), authDb, overwrite: true);

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Mahjong"] = $"Data Source={mahjongDb}",
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={authDb}"
                });
            });
        });
    }

    public void Dispose()
    {
        Factory.Dispose();
        System.Data.SQLite.SQLiteConnection.ClearAllPools();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (Directory.Exists(TempDir))
        {
            try { Directory.Delete(TempDir, recursive: true); } catch { }
        }
        GC.SuppressFinalize(this);
    }
}

public class AccountAndAccessTests(IsolatedWebAppFixture fixture) : IClassFixture<IsolatedWebAppFixture>
{
    private readonly WebApplicationFactory<Program> _factory = fixture.Factory;

    private static string ExtractAntiForgeryToken(string html)
    {
        var match = Regex.Match(html, @"name=""__RequestVerificationToken""\s+type=""hidden""\s+value=""([^""]+)""");
        if (!match.Success)
        {
            match = Regex.Match(html, @"value=""([^""]+)""\s+name=""__RequestVerificationToken""");
        }
        Assert.True(match.Success, "Expected anti-forgery token in HTML form.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var loginGet = await client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, loginGet.StatusCode);
        string loginHtml = await loginGet.Content.ReadAsStringAsync();
        string token = ExtractAntiForgeryToken(loginHtml);

        var loginPost = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["UserName"] = username,
            ["Password"] = password,
            ["RememberMe"] = "false"
        }));

        Assert.Equal(HttpStatusCode.Redirect, loginPost.StatusCode);
        return client;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Mahjong/Stats/0")]
    [InlineData("/Mahjong/LeaderBoard")]
    [InlineData("/Mahjong/History")]
    [InlineData("/Mahjong/Records")]
    [InlineData("/Mahjong/RoundRecords")]
    [InlineData("/Mahjong/Game?gameId=6")]
    [InlineData("/Account/Login")]
    public async Task PublicEndpoints_Return200ForAnonymousVisitor(string url)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Mahjong/AddPlayer")]
    [InlineData("/Mahjong/NewGame")]
    [InlineData("/Mahjong/Sub?gameId=6")]
    [InlineData("/Admin/Index")]
    [InlineData("/Account/Register")]
    [InlineData("/Manage/Index")]
    public async Task ProtectedEndpoints_RedirectAnonymousVisitorToLogin(string url)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task RoleBasedAuthorization_AllowsPlayerGameplay_RestrictsAdminAndRegisterToAdmin()
    {
        using var playerClient = await CreateAuthenticatedClientAsync("testplayer", "Test1234!");

        var addPlayerResp = await playerClient.GetAsync("/Mahjong/AddPlayer");
        Assert.Equal(HttpStatusCode.OK, addPlayerResp.StatusCode);

        var playerRegisterResp = await playerClient.GetAsync("/Account/Register");
        Assert.NotEqual(HttpStatusCode.OK, playerRegisterResp.StatusCode);

        var playerAdminResp = await playerClient.GetAsync("/Admin/Index");
        Assert.NotEqual(HttpStatusCode.OK, playerAdminResp.StatusCode);

        using var adminClient = await CreateAuthenticatedClientAsync("testadmin", "Test1234!");
        var adminIndexResp = await adminClient.GetAsync("/Admin/Index");
        Assert.Equal(HttpStatusCode.OK, adminIndexResp.StatusCode);

        var adminRegisterResp = await adminClient.GetAsync("/Account/Register");
        Assert.Equal(HttpStatusCode.OK, adminRegisterResp.StatusCode);
    }

    [Fact]
    public async Task Login_LocksOutAccountAfterFiveFailedAttempts()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        string lastBody = string.Empty;
        for (int attempt = 1; attempt <= 5; attempt++)
        {
            var getResp = await client.GetAsync("/Account/Login");
            string token = ExtractAntiForgeryToken(await getResp.Content.ReadAsStringAsync());

            var postResp = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["UserName"] = "lockoutuser",
                ["Password"] = "WrongPassword999!",
                ["RememberMe"] = "false"
            }));

            Assert.Equal(HttpStatusCode.OK, postResp.StatusCode);
            lastBody = await postResp.Content.ReadAsStringAsync();
        }

        Assert.Contains("账号已锁定", lastBody);
    }
}
