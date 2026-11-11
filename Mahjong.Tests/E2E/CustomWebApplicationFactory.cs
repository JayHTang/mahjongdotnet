using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Mahjong.Tests.E2E;

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<CustomWebApplicationFactory<Program>>
{
}

public class CustomWebApplicationFactory<TProgram> : IDisposable where TProgram : class
{
    private readonly Process _process;
    private readonly string _testDbDir;
    public string ServerAddress => "http://127.0.0.1:15199";

    public CustomWebApplicationFactory()
    {
        string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Mahjong/Mahjong.csproj"));
        string projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;

        _testDbDir = Path.Combine(Path.GetTempPath(), $"mahjong_e2e_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDbDir);
        string dbPath = Path.Combine(_testDbDir, "Mahjong.db");
        string authDbPath = Path.Combine(_testDbDir, "Authentication.db");

        string templateDir = Path.Combine(projectDir, "App_Data", "db");
        File.Copy(Path.Combine(templateDir, "Mahjong.db.template"), dbPath, overwrite: true);
        File.Copy(Path.Combine(templateDir, "Authentication.db.template"), authDbPath, overwrite: true);
        RefreshOngoingGameTimestamps(dbPath);

#if DEBUG
        string config = "Debug";
#else
        string config = "Release";
#endif

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build -c {config} --project \"{projectPath}\" --urls=\"{ServerAddress}\"",
            WorkingDirectory = projectDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.EnvironmentVariables["ConnectionStrings__Mahjong"] = $"Data Source={dbPath}";
        startInfo.EnvironmentVariables["ConnectionStrings__DefaultConnection"] = $"Data Source={authDbPath}";

        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the Mahjong web application process.");

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        _process.OutputDataReceived += (_, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        _process.ErrorDataReceived += (_, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        var sw = Stopwatch.StartNew();
        bool isUp = false;
        while (sw.ElapsedMilliseconds < 30000)
        {
            if (_process.HasExited)
            {
                throw new Exception($"Test web server exited unexpectedly with code {_process.ExitCode}.\nStdout: {outputBuilder}\nStderr: {errorBuilder}");
            }

            try
            {
                using var tcpClient = new TcpClient();
                tcpClient.Connect("127.0.0.1", 15199);
                isUp = true;
                break;
            }
            catch
            {
                System.Threading.Thread.Sleep(300);
            }
        }

        if (!isUp)
        {
            throw new Exception($"Test web server failed to start within 30 seconds.\nStdout: {outputBuilder}\nStderr: {errorBuilder}");
        }
    }

    private static void RefreshOngoingGameTimestamps(string dbPath)
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        string game6Start = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");
        string game6Round = DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss");
        cmd.CommandText = @"
            UPDATE Game SET created = @g6Start, finished = 0 WHERE id = 6;
            UPDATE Round SET created = @g6Round WHERE game_id = 6;
            UPDATE RoundDetail SET created = @g6Round WHERE round_id IN (SELECT id FROM Round WHERE game_id = 6);
            UPDATE Dice SET created = @g6Round WHERE game_id = 6;";
        cmd.Parameters.AddWithValue("@g6Start", game6Start);
        cmd.Parameters.AddWithValue("@g6Round", game6Round);
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            _process.Dispose();
        }
        if (Directory.Exists(_testDbDir))
        {
            try { Directory.Delete(_testDbDir, recursive: true); } catch { }
        }
        GC.SuppressFinalize(this);
    }
}
