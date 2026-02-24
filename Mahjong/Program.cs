using Mahjong.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Infinite, rollOnFileSizeLimit: true, fileSizeLimitBytes: 10 * 1024 * 1024)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddSystemWebAdapters()
.AddWrappedAspNetCoreSession()
.AddJsonSessionSerializer(options =>
{
    options.RegisterKey<string>("MachineName");
    options.RegisterKey<string>("SessionStartTime");
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseSystemWebAdapters();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Mahjong}/{action=Stats}/{id?}")
    .RequireSystemWebAdapterSession();


// Migration hack for missing columns in SQLite
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var connection = dbContext.Database.GetDbConnection();
    connection.Open();
    using (var command = connection.CreateCommand())
    {
        command.CommandText = "PRAGMA table_info(AspNetUsers);";
        var hasConcurrencyStamp = false;
        var hasNormalizedUserName = false;
        var hasNormalizedEmail = false;
        var hasLockoutEnd = false;
        var hasLockoutEnabled = false;
        var hasAccessFailedCount = false;

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (name == "ConcurrencyStamp") hasConcurrencyStamp = true;
                if (name == "NormalizedUserName") hasNormalizedUserName = true;
                if (name == "NormalizedEmail") hasNormalizedEmail = true;
                if (name == "LockoutEnd") hasLockoutEnd = true;
                if (name == "LockoutEnabled") hasLockoutEnabled = true;
                if (name == "AccessFailedCount") hasAccessFailedCount = true;
            }
        }

        if (!hasConcurrencyStamp)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN ConcurrencyStamp TEXT;";
            alterCmd.ExecuteNonQuery();
        }
        if (!hasNormalizedUserName)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN NormalizedUserName TEXT;";
            alterCmd.ExecuteNonQuery();
        }
        if (!hasNormalizedEmail)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN NormalizedEmail TEXT;";
            alterCmd.ExecuteNonQuery();
        }
        if (!hasLockoutEnd)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN LockoutEnd TEXT;";
            alterCmd.ExecuteNonQuery();
        }
        if (!hasLockoutEnabled)
        {
            using var alterCmd = connection.CreateCommand();
            // SQLite doesn't have a boolean type, so define as INTEGER (0 or 1)
            // But EF Core maps bool to INTEGER anyway.
            // Also need to be careful if user wants a default value.
            // Using INTEGER DEFAULT 0 (false) seems safe for LockoutEnabled in this context
            alterCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN LockoutEnabled INTEGER NOT NULL DEFAULT 0;";
            alterCmd.ExecuteNonQuery();
        }
        if (!hasAccessFailedCount)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN AccessFailedCount INTEGER NOT NULL DEFAULT 0;";
            alterCmd.ExecuteNonQuery();
        }
    }

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "PRAGMA table_info(AspNetRoles);";
        var hasConcurrencyStamp = false;
        var hasNormalizedName = false;

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (name == "ConcurrencyStamp") hasConcurrencyStamp = true;
                if (name == "NormalizedName") hasNormalizedName = true;
            }
        }

        if (!hasConcurrencyStamp)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetRoles ADD COLUMN ConcurrencyStamp TEXT;";
            alterCmd.ExecuteNonQuery();
        }
        if (!hasNormalizedName)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetRoles ADD COLUMN NormalizedName TEXT;";
            alterCmd.ExecuteNonQuery();
        }

        // Fix missing normalized data
        using var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
                UPDATE AspNetUsers SET NormalizedUserName = UPPER(UserName) WHERE NormalizedUserName IS NULL;
                UPDATE AspNetUsers SET NormalizedEmail = UPPER(Email) WHERE NormalizedEmail IS NULL;
                UPDATE AspNetRoles SET NormalizedName = UPPER(Name) WHERE NormalizedName IS NULL;
            ";
        updateCmd.ExecuteNonQuery();
    }

    // Fix missing ProviderDisplayName in AspNetUserLogins (for Core Identity migration)
    using (var command = connection.CreateCommand())
    {
        command.CommandText = "PRAGMA table_info(AspNetUserLogins);";
        var hasProviderDisplayName = false;

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (name == "ProviderDisplayName") hasProviderDisplayName = true;
            }
        }

        if (!hasProviderDisplayName)
        {
            using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE AspNetUserLogins ADD COLUMN ProviderDisplayName TEXT;";
            alterCmd.ExecuteNonQuery();
        }
    }

    // Fix missing tables for Identity
    using (var command = connection.CreateCommand())
    {
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS 'AspNetRoleClaims' (
                'Id' INTEGER NOT NULL CONSTRAINT 'PK_AspNetRoleClaims' PRIMARY KEY AUTOINCREMENT,
                'RoleId' TEXT NOT NULL,
                'ClaimType' TEXT NULL,
                'ClaimValue' TEXT NULL,
                CONSTRAINT 'FK_AspNetRoleClaims_AspNetRoles_RoleId' FOREIGN KEY ('RoleId') REFERENCES 'AspNetRoles' ('Id') ON DELETE CASCADE
            );
            
            CREATE TABLE IF NOT EXISTS 'AspNetUserClaims' (
                'Id' INTEGER NOT NULL CONSTRAINT 'PK_AspNetUserClaims' PRIMARY KEY AUTOINCREMENT,
                'UserId' TEXT NOT NULL,
                'ClaimType' TEXT NULL,
                'ClaimValue' TEXT NULL,
                CONSTRAINT 'FK_AspNetUserClaims_AspNetUsers_UserId' FOREIGN KEY ('UserId') REFERENCES 'AspNetUsers' ('Id') ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS 'AspNetUserLogins' (
                'LoginProvider' TEXT NOT NULL,
                'ProviderKey' TEXT NOT NULL,
                'ProviderDisplayName' TEXT NULL,
                'UserId' TEXT NOT NULL,
                CONSTRAINT 'PK_AspNetUserLogins' PRIMARY KEY ('LoginProvider', 'ProviderKey'),
                CONSTRAINT 'FK_AspNetUserLogins_AspNetUsers_UserId' FOREIGN KEY ('UserId') REFERENCES 'AspNetUsers' ('Id') ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS 'AspNetUserTokens' (
                'UserId' TEXT NOT NULL,
                'LoginProvider' TEXT NOT NULL,
                'Name' TEXT NOT NULL,
                'Value' TEXT NULL,
                CONSTRAINT 'PK_AspNetUserTokens' PRIMARY KEY ('UserId', 'LoginProvider', 'Name'),
                CONSTRAINT 'FK_AspNetUserTokens_AspNetUsers_UserId' FOREIGN KEY ('UserId') REFERENCES 'AspNetUsers' ('Id') ON DELETE CASCADE
            );
            
            CREATE INDEX IF NOT EXISTS 'IX_AspNetRoleClaims_RoleId' ON 'AspNetRoleClaims' ('RoleId');
            CREATE INDEX IF NOT EXISTS 'IX_AspNetUserClaims_UserId' ON 'AspNetUserClaims' ('UserId');
            CREATE INDEX IF NOT EXISTS 'IX_AspNetUserLogins_UserId' ON 'AspNetUserLogins' ('UserId');
        ";
        command.ExecuteNonQuery();
    }
}

app.Run();
