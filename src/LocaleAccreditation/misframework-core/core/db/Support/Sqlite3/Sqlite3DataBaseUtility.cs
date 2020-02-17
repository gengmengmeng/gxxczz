using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Sqlite3
{
    public class Sqlite3DataBaseUtility : AbstractDataBaseUtility
    {
        public Sqlite3DataBaseUtility(string connString)
        {
            this.dbProviderFactory = SQLiteFactory.Instance;
            this.connectionString = connString;
        }

        public override string DataBaseRestricion(DbConnection conn)
        {
            string result = null;
            bool closed = !conn.State.Equals(ConnectionState.Open);
            if (closed)
                conn.Open();
            result = conn.Database;
            if (closed)
                conn.Close();
            return result;
        }

        public override string ConvertParamsToStaticString(DbCommand cmd, string sql, List<DbParameter> aparams)
        {
            StringBuilder result = new StringBuilder();
            string newSql = sql;
            foreach (DbParameter p in aparams)
            {
                string fullParamName = this.Dialect.ParameterPrefix + p.ParameterName;
                if (System.DBNull.Value.Equals(p.Value) || p.Value == null)
                {
                    result.Append(newSql.Substring(0, newSql.IndexOf(fullParamName)));
                    result.Append("null");
                    newSql = newSql.Substring(newSql.IndexOf(fullParamName) + fullParamName.Length);
                }
                else if (p.DbType.Equals(DbType.String))
                {
                    result.Append(newSql.Substring(0, newSql.IndexOf(fullParamName)));
                    result.Append("'" + p.Value.ToString().Replace("'", "''") + "'");
                    newSql = newSql.Substring(newSql.IndexOf(fullParamName) + fullParamName.Length);
                }
                else if (p.DbType.Equals(DbType.Date) || p.DbType.Equals(DbType.DateTime) || p.DbType.Equals(DbType.DateTime2) || p.DbType.Equals(DbType.Time))
                {
                    result.Append(newSql.Substring(0, newSql.IndexOf(fullParamName)));
                    DateTime dt;
                    if (p.Value is DateTime)
                        dt = (DateTime)p.Value;
                    else
                        dt = DateTime.Parse(p.Value.ToString());
                    result.Append("'" + dt.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                    newSql = newSql.Substring(newSql.IndexOf(fullParamName) + fullParamName.Length);
                }
                else if (p.DbType.Equals(DbType.Decimal))
                {
                    result.Append(newSql.Substring(0, newSql.IndexOf(fullParamName)));
                    result.Append(p.Value.ToString());
                    newSql = newSql.Substring(newSql.IndexOf(fullParamName) + fullParamName.Length);
                }
                else
                {
                    cmd.Parameters.Add(p);
                }
            }
            result.Append(newSql);
            return result.ToString();
        }

        private IDialect dialect = new Sqlite3Dialect();
        public override IDialect Dialect
        {
            get { return dialect; }
        }

        protected override ITableInfo CreateTableInfo()
        {
            return new Sqlite3TableInfo();
        }

        public override IFieldInfo CreateFieldInfo()
        {
            return new Sqlite3FieldInfo();
        }

        protected override SortedDictionary<String, DbType> GetDbTypeMapper()
        {
            if (dbTypeMapper == null)
            {
                dbTypeMapper = new SortedDictionary<String, DbType>();
                dbTypeMapper["INT"] = DbType.Decimal;
                dbTypeMapper["INTEGER"] = DbType.Decimal;
                dbTypeMapper["FLOAT"] = DbType.Decimal;
                dbTypeMapper["REAL"] = DbType.Decimal;
                dbTypeMapper["NUMERIC"] = DbType.Decimal;
                dbTypeMapper["BOOLEAN"] = DbType.Byte;
                dbTypeMapper["TIME"] = DbType.Time;
                dbTypeMapper["DATE"] = DbType.DateTime;
                dbTypeMapper["DATETIME"] = DbType.DateTime;
                dbTypeMapper["VARCHAR"] = DbType.String;
                dbTypeMapper["NVARCHAR"] = DbType.String;
                dbTypeMapper["TEXT"] = DbType.String;
                dbTypeMapper["BLOB"] = DbType.Binary;                
            }
            return dbTypeMapper;
        }
    }
}
