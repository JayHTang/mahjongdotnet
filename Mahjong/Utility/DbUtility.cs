using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using Microsoft.Data.SqlClient;
using Serilog;

namespace Mahjong.Utility
{
    public enum Db
    {
        SQLServer,
        SQLite
    }

    public class DbFactory
    {
        public static DbUtility GetDbUtility(Db db)
        {
            if (db == Db.SQLServer)
            {
                return new SqlServerUtility();
            }
            else if (db == Db.SQLite)
            {
                return new SqliteUtility();
            }
            throw new NotImplementedException();
        }
    }

    public abstract class DbUtility
    {
        public abstract DataTable Read(string db, string sql, params DbParameter[] parameters);

        public DataRow ReadFirstRow(string db, string sql, params DbParameter[] parameters)
        {
            return Read(db, sql, parameters).Rows[0];
        }

        public abstract int Insert(string db, string sql, params DbParameter[] parameters);

        public abstract int InsertAndGetId(string db, string sql, params DbParameter[] parameters);

        public int Update(string db, string sql, params DbParameter[] parameters)
        {
            return Insert(db, sql, parameters);
        }

        public int Delete(string db, string sql, params DbParameter[] parameters)
        {
            return Insert(db, sql, parameters);
        }

        public static readonly int Zero = 0;

    }

    public class SqlServerUtility : DbUtility
    {
        public override DataTable Read(string db, string sql, params DbParameter[] parameters)
        {
            DataTable dt = new();

            try
            {
                using SqlConnection sqlConn = new(db);
                SqlCommand sqlCommand = sqlConn.CreateCommand();
                sqlCommand.CommandText = sql;

                if (parameters.Length > 0)
                {
                    foreach (SqlParameter parameter in parameters.Cast<SqlParameter>())
                    {
                        sqlCommand.Parameters.Add(parameter);
                    }
                }

                sqlConn.Open();
                SqlDataAdapter adapter = new(sqlCommand);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception when executing {Sql} on {Db}", sql, db);
            }

            return dt;
        }

        public override int Insert(string db, string sql, params DbParameter[] parameters)
        {
            try
            {
                using SqlConnection sqlConn = new(db);
                SqlCommand sqlCommand = sqlConn.CreateCommand();
                sqlCommand.CommandText = sql;

                if (parameters.Length > 0)
                {
                    foreach (SqlParameter parameter in parameters.Cast<SqlParameter>())
                    {
                        sqlCommand.Parameters.Add(parameter);
                    }
                }

                sqlConn.Open();
                return sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception when executing {Sql} on {Db}", sql, db);
            }

            return 0;
        }

        public override int InsertAndGetId(string db, string sql, params DbParameter[] parameters)
        {
            try
            {
                using SqlConnection sqlConn = new(db);
                SqlCommand sqlCommand = sqlConn.CreateCommand();
                sqlCommand.CommandText = sql;

                if (parameters.Length > 0)
                {
                    foreach (SqlParameter parameter in parameters.Cast<SqlParameter>())
                    {
                        sqlCommand.Parameters.Add(parameter);
                    }
                }

                sqlConn.Open();
                return (int)sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception when executing {Sql} on {Db}", sql, db);
            }

            return 0;
        }
    }

    public class SqliteUtility : DbUtility
    {
        /// <summary>
        /// Read from a SQLite DB table with a query
        /// </summary>
        /// <param name="db">connection string</param>
        /// <param name="sql">query</param>
        /// <param name="parameters">paramters in the query</param>
        /// <returns>Data in a DataTable</returns>
        public override DataTable Read(string db, string sql, params DbParameter[] parameters)
        {
            DataTable dt = new();

            try
            {
                using SQLiteConnection conn = new(db);
                SQLiteCommand command = conn.CreateCommand();
                command.CommandText = sql;

                if (parameters.Length > 0)
                {
                    foreach (SQLiteParameter parameter in parameters.Cast<SQLiteParameter>())
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                conn.Open();
                SQLiteDataAdapter adapter = new(command);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception when executing {Sql} on {Db}", sql, db);
            }

            return dt;
        }

        /// <summary>
        /// Insert data into a SQLite DB table
        /// </summary>
        /// <param name="db">connection string</param>
        /// <param name="sql">query</param>
        /// <param name="parameters">paramters in the query</param>
        /// <returns>Number of rows inserted</returns>
        public override int Insert(string db, string sql, params DbParameter[] parameters)
        {
            try
            {
                using SQLiteConnection conn = new(db);
                SQLiteCommand command = conn.CreateCommand();
                command.CommandText = sql;

                if (parameters.Length > 0)
                {
                    foreach (SQLiteParameter parameter in parameters.Cast<SQLiteParameter>())
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                conn.Open();
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception when executing {Sql} on {Db}", sql, db);
            }

            return 0;
        }

        /// <summary>
        /// Insert data into a SQLite DB table and get the id of the first row inserted
        /// Only works if id is the first column in the table
        /// Otherwise the first column will be returned and may cause errors if it cannot be cast into int
        /// </summary>
        /// <param name="db">connection string</param>
        /// <param name="sql">query</param>
        /// <param name="parameters">paramters in the query</param>
        /// <returns>Id of row inserted</returns>
        public override int InsertAndGetId(string db, string sql, params DbParameter[] parameters)
        {
            try
            {
                using SQLiteConnection conn = new(db);
                SQLiteCommand command = conn.CreateCommand();
                command.CommandText = sql;

                if (parameters.Length > 0)
                {
                    foreach (SQLiteParameter parameter in parameters.Cast<SQLiteParameter>())
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                conn.Open();
                command.ExecuteNonQuery();
                return (int)conn.LastInsertRowId;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception when executing {Sql} on {Db}", sql, db);
            }

            return 0;
        }
    }
}