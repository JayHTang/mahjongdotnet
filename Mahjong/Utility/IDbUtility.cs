#nullable enable
using System.Data;
using System.Data.Common;

namespace Mahjong.Utility
{
    public interface IDbUtility
    {
        DataTable Read(string db, string sql, params DbParameter[] parameters);
        DataRow? ReadFirstRow(string db, string sql, params DbParameter[] parameters);
        int Insert(string db, string sql, params DbParameter[] parameters);
        int InsertAndGetId(string db, string sql, params DbParameter[] parameters);
        int Update(string db, string sql, params DbParameter[] parameters);
        int Delete(string db, string sql, params DbParameter[] parameters);
    }
}
