using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Mahjong.Tests.E2E;

[Collection("E2E")]
public class SmokeTests(CustomWebApplicationFactory<Program> factory) : PageTest
{
    private readonly string _baseUrl = factory.ServerAddress;

    private async Task BlockExternalAnalyticsNetworkAsync(IPage page)
    {
        // Stub external googletagmanager script fetch so E2E never sends real hits and runs deterministically offline/CI
        await page.RouteAsync("**/googletagmanager.com/**", async route =>
        {
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/javascript",
                Body = "/* mocked gtag.js */"
            });
        });
    }

    [Fact]
    public async Task PublicPages_DesktopAndMobile_RenderChartsAnalyticsAndThemeWithoutErrors()
    {
        await BlockExternalAnalyticsNetworkAsync(Page);

        var pageErrors = new List<string>();
        Page.PageError += (_, err) => pageErrors.Add(err);

        await Page.SetViewportSizeAsync(1440, 900);

        // 1. Stats page (/) — verify GA4 script, unencoded Chinese page_title, zero NaN, and DependencyWheel chart
        var statsResp = await Page.GotoAsync($"{_baseUrl}/");
        Assert.NotNull(statsResp);
        Assert.Equal(200, statsResp!.Status);

        var gtagSrc = await Page.Locator("head script[src*='G-3HGH3YFG6R']").GetAttributeAsync("src");
        Assert.Contains("G-3HGH3YFG6R", gtagSrc);

        bool hasConfigAndChineseTitle = await Page.EvaluateAsync<bool>(@"() => {
            if (!Array.isArray(window.dataLayer)) return false;
            let hasConfig = false;
            let hasCleanTitle = false;
            for (const entry of window.dataLayer) {
                const arr = Array.from(entry);
                if (arr[0] === 'config' && arr[1] === 'G-3HGH3YFG6R') hasConfig = true;
                if (arr[0] === 'set' && arr[1] && arr[1].page_title === '数据') hasCleanTitle = true;
            }
            return hasConfig && hasCleanTitle;
        }");
        Assert.True(hasConfigAndChineseTitle, "Expected GA4 G-3HGH3YFG6R config and unencoded Chinese page_title in window.dataLayer.");

        string statsText = await Page.Locator("body").InnerTextAsync();
        Assert.DoesNotContain("$NaN", statsText);
        Assert.DoesNotContain("NaN%", statsText);

        await Expect(Page.Locator("#chart svg.highcharts-root")).ToBeVisibleAsync();
        await Expect(Page.Locator("#chart .highcharts-title")).ToHaveTextAsync("");

        // 2. Toggle Light <-> Dark theme and verify persistence + GA4 theme_toggle event
        string initialTheme = await Page.Locator("html").GetAttributeAsync("data-theme") ?? "light";
        await Page.ClickAsync("#themeToggle");
        string toggledTheme = initialTheme == "dark" ? "light" : "dark";
        await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", toggledTheme);

        bool hasThemeEvent = await Page.EvaluateAsync<bool>(@"expectedTheme => {
            return Array.from(window.dataLayer).some(e => {
                const arr = Array.from(e);
                return arr[0] === 'event' && arr[1] === 'theme_toggle' && arr[2] && arr[2].theme === expectedTheme;
            });
        }", toggledTheme);
        Assert.True(hasThemeEvent, "Expected theme_toggle event in window.dataLayer.");

        // 3. Navigate to LeaderBoard — verify theme persisted and spline chart renders without duplicate title
        var lbResp = await Page.GotoAsync($"{_baseUrl}/Mahjong/LeaderBoard");
        Assert.Equal(200, lbResp!.Status);
        await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", toggledTheme);
        await Expect(Page.Locator("#chart svg.highcharts-root")).ToBeVisibleAsync();
        await Expect(Page.Locator("#chart .highcharts-title")).ToHaveTextAsync("");

        // 4. Check History, Records, RoundRecords at mobile viewport (375x812)
        await Page.SetViewportSizeAsync(375, 812);
        foreach (var path in new[] { "/Mahjong/History", "/Mahjong/Records", "/Mahjong/RoundRecords" })
        {
            var resp = await Page.GotoAsync($"{_baseUrl}{path}");
            Assert.Equal(200, resp!.Status);
            if (path == "/Mahjong/History")
            {
                string selectAppearance = await Page.EvaluateAsync<string>(@"() => {
                    const el = document.querySelector('.dataTables_length select');
                    return el ? window.getComputedStyle(el).appearance : '';
                }");
                Assert.Equal("none", selectAppearance);
            }
        }

        Assert.Empty(pageErrors);
    }

    [Fact]
    public async Task AuthenticatedGameplayAndAdminFlows_VerifyDiceModalEditRoundAndValidator()
    {
        await BlockExternalAnalyticsNetworkAsync(Page);

        var pageErrors = new List<string>();
        Page.PageError += (_, err) => pageErrors.Add(err);

        await Page.SetViewportSizeAsync(1440, 900);

        // Log in as testadmin
        await Page.GotoAsync($"{_baseUrl}/Account/Login");
        await Page.FillAsync("#UserName", "testadmin");
        await Page.FillAsync("#Password", "Test1234!");
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForURLAsync(url => !url.Contains("/Account/Login"));

        // Verify Admin Index loads cleanly
        var adminResp = await Page.GotoAsync($"{_baseUrl}/Admin/Index");
        Assert.Equal(200, adminResp!.Status);
        await Expect(Page.Locator("body")).ToContainTextAsync("testadmin");

        // Navigate to active ongoing Game (gameId=6)
        var gameResp = await Page.GotoAsync($"{_baseUrl}/Mahjong/Game?gameId=6");
        Assert.Equal(200, gameResp!.Status);
        await Expect(Page.Locator("#chart svg.highcharts-root")).ToBeVisibleAsync();
        await Expect(Page.Locator("#chart .highcharts-title")).ToHaveTextAsync("");

        // Dice modal auto-opens for unfinished game; click dice '24'
        await Expect(Page.Locator("#dice-modal")).ToBeVisibleAsync();
        await Page.ClickAsync("#dice-table a[id='24']");
        await Expect(Page.Locator("#result_form #Dice")).ToHaveValueAsync("24");

        bool hasDiceEvent = await Page.EvaluateAsync<bool>(@"() => {
            return Array.from(window.dataLayer).some(e => {
                const arr = Array.from(e);
                return arr[0] === 'event' && arr[1] === 'dice_roll_select' && arr[2] && arr[2].dice === '24';
            });
        }");
        Assert.True(hasDiceEvent, "Expected dice_roll_select event in window.dataLayer.");

        // Open Edit Round modal TWICE and verify action URL does not duplicate roundId query params
        var firstEditBtn = Page.Locator(".modal-edit-round").First;
        await firstEditBtn.ClickAsync();
        await Expect(Page.Locator("#editRound")).ToBeVisibleAsync();
        string actionFirstOpen = await Page.Locator("#edit_form").GetAttributeAsync("action") ?? "";
        await Page.ClickAsync("#editRound button[data-dismiss='modal']");
        await Expect(Page.Locator("#editRound")).ToBeHiddenAsync();

        await firstEditBtn.ClickAsync();
        await Expect(Page.Locator("#editRound")).ToBeVisibleAsync();
        string actionSecondOpen = await Page.Locator("#edit_form").GetAttributeAsync("action") ?? "";
        Assert.Equal(actionFirstOpen, actionSecondOpen);
        await Page.ClickAsync("#editRound button[data-dismiss='modal']");
        await Expect(Page.Locator("#editRound")).ToBeHiddenAsync();

        // Test mahjong-game-validator.js: select Winner without Zimo/Dianpao -> triggers .validation-alert, then allow retry!
        await Page.SelectOptionAsync("#result_form #WinnerId", "1");
        await Page.ClickAsync("#result_form button[type='submit']");
        await Expect(Page.Locator("#result_form .validation-alert").First).ToContainTextAsync("必须自摸或点炮");

        // Fix the form (select HandId + Zimo) and submit -> round records and reloads cleanly!
        await Page.SelectOptionAsync("#result_form #HandId", "1");
        await Page.FillAsync("#result_form #HuaCount", "4");
        await Page.CheckAsync("#result_form #Zimo");
        await Page.ClickAsync("#result_form button[type='submit']");
        await Expect(Page.Locator("#details")).ToContainTextAsync("测试东 自摸");

        Assert.Empty(pageErrors);
    }
}
