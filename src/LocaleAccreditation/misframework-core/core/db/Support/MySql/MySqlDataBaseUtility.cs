using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.MySql
{
    public class MySqlDataBaseUtility : AbstractDataBaseUtility
    {
        public MySqlDataBaseUtility(string connString)
        {
           
            this.dbProviderFactory = MySqlClientFactory.Instance;
            this.connectionString = connString;
        }

        public override string DataBaseRestricion(DbConnection conn)
        {
            //Mysql里无需 使用这种方式
            return null;
        }

        private IDialect dialect = new MySqlDialect();
        public override IDialect Dialect
        {
            get { return dialect; }
        }

        protected override ITableInfo CreateTableInfo()
        {
            return new MySqlTableInfo();
        }

        public override IFieldInfo CreateFieldInfo()
        {
            return new MySqlFieldInfo();
        }

        protected override SortedDictionary<String, DbType> GetDbTypeMapper()
        {
            if (dbTypeMapper == null)
            {
                dbTypeMapper = new SortedDictionary<String, DbType>();
                dbTypeMapper["VARCHAR"] = DbType.String;
                dbTypeMapper["CHAR"] = DbType.String;
                dbTypeMapper["TEXT"] = DbType.String;
                dbTypeMapper["DATETIME"] = DbType.DateTime;
                dbTypeMapper["DATE"] = DbType.DateTime;
                dbTypeMapper["TIME"] = DbType.DateTime;
                dbTypeMapper["DECIMAL"] = DbType.Decimal;
                dbTypeMapper["INT"] = DbType.Decimal;
                dbTypeMapper["BLOB"] = DbType.Binary;
                
            }
            return dbTypeMapper;
        }
    }
}
