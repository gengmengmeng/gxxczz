using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Access
{
    public class AccessDataBaseUtility : AbstractDataBaseUtility
    {
        public AccessDataBaseUtility(string connString)
        {
            this.dbProviderFactory = OleDbFactory.Instance;
            this.connectionString = connString;
        }

        public override string DataBaseRestricion(DbConnection conn)
        {
            return null;
        }

        private IDialect dialect = new AccessDialect();
        public override IDialect Dialect
        {
            get { return dialect; }
        }

        protected override ITableInfo CreateTableInfo()
        {
            return new AccessTableInfo();
        }

        public override IFieldInfo CreateFieldInfo()
        {
            return new AccessFieldInfo();
        }

        protected override SortedDictionary<String, DbType> GetDbTypeMapper()
        {
            if (dbTypeMapper == null)
            {
                dbTypeMapper = new SortedDictionary<String, DbType>();
                dbTypeMapper["SHORT"] = DbType.Int16;
                dbTypeMapper["LONG"] = DbType.Int32;
                dbTypeMapper["SINGLE"] = DbType.Single;
                dbTypeMapper["DOUBLE"] = DbType.Double;
                dbTypeMapper["CURRENCY"] = DbType.Decimal;
                dbTypeMapper["DATETIME"] = DbType.DateTime;
                dbTypeMapper["BIT"] = DbType.Boolean;
                dbTypeMapper["BYTE"] = DbType.Byte;
                dbTypeMapper["GUID"] = DbType.Guid;
                dbTypeMapper["BIGBINARY"] = DbType.Binary;
                dbTypeMapper["LONGBINARY"] = DbType.Binary;
                dbTypeMapper["VARBINARY"] = DbType.Binary;
                dbTypeMapper["LONGTEXT"] = DbType.String;
                dbTypeMapper["VARCHAR"] = DbType.String;
                dbTypeMapper["DECIMAL"] = DbType.Decimal;

                
            }
            return dbTypeMapper;
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
                else if (p.DbType.Equals(DbType.Date) || p.DbType.Equals(DbType.DateTime) || p.DbType.Equals(DbType.DateTime2))
                {
                    result.Append(newSql.Substring(0, newSql.IndexOf(fullParamName)));
                    DateTime dt;
                    if (p.Value is DateTime)
                        dt = (DateTime)p.Value;
                    else
                        dt = DateTime.Parse(p.Value.ToString());
                    result.Append("#" + dt.ToString("yyyy-MM-dd HH:mm:ss") + "#");
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
    }
}
