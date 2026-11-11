using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Mahjong.Utility;

namespace Mahjong.Tests.Utility;

public class SqliteUtilityTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connStr;
    private readonly IDbUtility _db;

    public SqliteUtilityTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"mahjong_sqlite_test_{Guid.NewGuid():N}.db");
        _connStr = $"Data Source={_dbPath}";
        _db = DbFactory.GetDbUtility(Db.SQLite);

        _db.Insert(_connStr, "CREATE TABLE TestPlayer (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, score REAL NOT NULL DEFAULT 0);");
    }

    [Fact]
    public void CrudOperations_WorkEndToEnd()
    {
        Assert.Null(_db.ReadFirstRow(_connStr, "SELECT * FROM TestPlayer;"));

        int id1 = _db.InsertAndGetId(_connStr, "INSERT INTO TestPlayer (name, score) VALUES (@name, @score);",
            new SQLiteParameter("@name", "Alice"),
            new SQLiteParameter("@score", 12.5));
        Assert.True(id1 > 0);

        int rows = _db.Insert(_connStr, "INSERT INTO TestPlayer (name, score) VALUES (@name, @score);",
            new SQLiteParameter("@name", "Bob"),
            new SQLiteParameter("@score", -12.5));
        Assert.Equal(1, rows);

        DataTable dt = _db.Read(_connStr, "SELECT id, name, score FROM TestPlayer ORDER BY id;");
        Assert.Equal(2, dt.Rows.Count);
        Assert.Equal("Alice", dt.Rows[0]["name"].ToString());

        int updated = _db.Update(_connStr, "UPDATE TestPlayer SET score = @score WHERE id = @id;",
            new SQLiteParameter("@score", 25.0),
            new SQLiteParameter("@id", id1));
        Assert.Equal(1, updated);

        DataRow? first = _db.ReadFirstRow(_connStr, "SELECT score FROM TestPlayer WHERE id = @id;",
            new SQLiteParameter("@id", id1));
        Assert.NotNull(first);
        Assert.Equal(25.0, Convert.ToDouble(first!["score"]));

        int deleted = _db.Delete(_connStr, "DELETE FROM TestPlayer WHERE id = @id;",
            new SQLiteParameter("@id", id1));
        Assert.Equal(1, deleted);
        Assert.Single(_db.Read(_connStr, "SELECT * FROM TestPlayer;").Rows);
    }

    [Fact]
    public void InvalidSql_ReturnsSafeFallbackWithoutThrowing()
    {
        DataTable dt = _db.Read(_connStr, "SELECT * FROM NonExistentTable;");
        Assert.Empty(dt.Rows);
        Assert.Equal(0, _db.Insert(_connStr, "INSERT INTO NonExistentTable VALUES (1);"));
    }

    public void Dispose()
    {
        SQLiteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); } catch { }
        }
        GC.SuppressFinalize(this);
    }
}
