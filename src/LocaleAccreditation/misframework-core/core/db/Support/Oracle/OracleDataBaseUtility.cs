using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Oracle
{
    public class OracleDataBaseUtility : AbstractDataBaseUtility
    {
        
        public OracleDataBaseUtility( string connString )
        {
            this.dbProviderFactory = OracleClientFactory.Instance;
            this.connectionString = connString;
        }

        public override string DataBaseRestricion(DbConnection conn)
        {
            DbDataAdapter da = DbProviderFactory.CreateDataAdapter();
            
            da.SelectCommand = DbProviderFactory.CreateCommand();
            da.SelectCommand.Connection = conn;
            da.SelectCommand.CommandText = "select user from dual";
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
                return dt.Rows[0][0].ToString();
            return null;
        }

        public override string ConvertParamsToStaticString(DbCommand cmd,string sql, List<DbParameter> aparams)
        {
            StringBuilder result = new StringBuilder();
            string newSql = sql;
            foreach (DbParameter p in aparams){
                string fullParamName = this.Dialect.ParameterPrefix + p.ParameterName;
                if (System.DBNull.Value.Equals(p.Value) || p.Value == null)
                {
                    result.Append(newSql.Substring(0, newSql.IndexOf(fullParamName)));
                    result.Append("null");
                    newSql = newSql.Substring(newSql.IndexOf(fullParamName) + fullParamName.Length);
                }
                else if (p.DbType.Equals(DbType.String)){
                    result.Append(newSql.Substring(0,newSql.IndexOf(fullParamName)));
                    result.Append("'" + p.Value.ToString().Replace("'","''") + "'");
                    newSql = newSql.Substring(newSql.IndexOf(fullParamName)+fullParamName.Length);
                }
                else if (p.DbType.Equals(DbType.Date) || p.DbType.Equals(DbType.DateTime) || p.DbType.Equals(DbType.DateTime2))
                {
                    result.Append(newSql.Substring(0, newSql.IndexOf(fullParamName)));
                    DateTime dt ;
                    if (p.Value is DateTime)
                        dt = (DateTime)p.Value;
                    else
                        dt = DateTime.Parse(p.Value.ToString());
                    result.Append("to_date('"+dt.ToString("yyyy-MM-dd HH:mm:ss")+"','yyyy-mm-dd hh24:mi:ss')");
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

        private IDialect dialect = new OracleDialect();
        public override IDialect Dialect
        {
            get { return dialect; }
        }

        protected override ITableInfo CreateTableInfo()
        {
            return new OracleTableInfo();
        }

        public override IFieldInfo CreateFieldInfo()
        {
            return new OracleFieldInfo();
        }

        protected override SortedDictionary<String, DbType> GetDbTypeMapper()
        {
            if (dbTypeMapper == null)
            {
                dbTypeMapper = new SortedDictionary<String, DbType>();
                dbTypeMapper["VARCHAR2"] = DbType.String;
                dbTypeMapper["NVARCHAR2"] = DbType.String;
                dbTypeMapper["NCLOB"] = DbType.String;
                dbTypeMapper["CLOB"] = DbType.String;
                dbTypeMapper["DATE"] = DbType.DateTime;
                dbTypeMapper["NUMBER"] = DbType.Decimal;
                dbTypeMapper["BLOB"] = DbType.Binary;
                dbTypeMapper["LONG"] = DbType.Binary;                
            }
            return dbTypeMapper;
        }
    }
}
