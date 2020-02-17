using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Sqlite3
{
    class Sqlite3TableInfo : AbstractTableInfo
    {
        public override void LoadInformation(IDataBaseUtility dbu, string tableName)
        {
            this.DataBaseUtility = dbu;
            this.tableName = tableName;

            DbConnection conn = dbu.CreateConnection();
            conn.Open();
            string db_user = dbu.DataBaseRestricion(conn);
            try
            {
                StringBuilder sbFields = new StringBuilder();
                sbFields.Append("Sqlite3TableInfo LoadInformation tableName="+tableName+":  ");
                DataTable columnInfor = dbu.GetSchema(conn, "Columns", new string[] { db_user,null, this.TableName });
                //读取字段基本信息
                for (int i = 0; i < columnInfor.Rows.Count; i++)
                {
                    IFieldInfo f = dbu.CreateFieldInfo();
                    f.Loadinformation(dbu, columnInfor.Rows[i]);
                    Fields.Add(f);
                    FieldsByName[f.CloumnName] = f;
                    sbFields.Append(f.CloumnName+"  :  " +f.DataTypeText+"  :   "+f.DbType.ToString()+ "   |\r\n");
                    if (f.IsPrimaryKey)
                        PrimaryFields[f.CloumnName] = f;
                }
                sbFields.Append("    PK: \n");
                for (int i = 0; i < columnInfor.Rows.Count; i++)
                {
                    IFieldInfo f = dbu.CreateFieldInfo();
                    f.Loadinformation(dbu, columnInfor.Rows[i]);
                    if (f.IsPrimaryKey)
                    {
                        sbFields.Append(f.CloumnName);
                    }
                }
                if (System.IO.Directory.Exists("c:\\hblogs"))
                    logger.Debug(sbFields);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
