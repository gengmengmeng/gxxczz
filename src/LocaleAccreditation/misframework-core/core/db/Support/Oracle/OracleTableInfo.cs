using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Oracle
{
    public class OracleTableInfo : AbstractTableInfo
    {

        public override void LoadInformation(IDataBaseUtility dbu , string tableName)
        {
            this.DataBaseUtility = dbu;
            this.tableName = tableName;
            DbConnection conn = dbu.CreateConnection();
            conn.Open();
            string db_user = dbu.DataBaseRestricion(conn);
            string tn = this.tableName;
            if (tn.IndexOf(".") > 0)
            {
                db_user = tn.Substring(0, tn.IndexOf("."));
                tn = tn.Substring(tn.IndexOf(".") + 1);
            }
            //如果表名使用了前缀：如 Oracle中的"用户名.表名的形式"就要使用另外的方法区分.
            try
            {
                StringBuilder sbFields = new StringBuilder();
                sbFields.Append("OracleTableInfo LoadInformation tableName="+tableName+":  ");
                DataTable columnInfor = dbu.GetSchema(conn, "Columns", new string[] { db_user, tn });
                //读取字段基本信息
                for (int i = 0; i < columnInfor.Rows.Count; i++)
                {
                    IFieldInfo f = dbu.CreateFieldInfo();
                    f.Loadinformation(dbu, columnInfor.Rows[i]);
                    Fields.Add(f);
                    FieldsByName[f.CloumnName] = f;
                    sbFields.Append(f.CloumnName+"  :  " +f.DataTypeText+"  :   "+f.DbType.ToString()+ "   |\r\n");
                }
                sbFields.Append("    PK: \n");
                //读取表的主键信息
                DataTable pk_name = dbu.GetSchema(conn, "PrimaryKeys", new string[] { db_user, tn });
                if (pk_name.Rows.Count > 0)
                {
                    string primary_key_name = pk_name.Rows[0]["CONSTRAINT_NAME"].ToString();
                    DataTable pk_cloumns = dbu.GetSchema(conn, "IndexColumns", new string[] { db_user, primary_key_name , db_user , tn });
                    for (int i = 0; i < pk_cloumns.Rows.Count; i++)
                    {
                        string cloumn_name = pk_cloumns.Rows[i]["COLUMN_NAME"].ToString();
                        IFieldInfo field_info = FieldsByName[cloumn_name];
                        if (field_info!=null) 
                        {
                            PrimaryFields[cloumn_name] = field_info;
                            sbFields.Append(field_info.CloumnName);
                        }
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
