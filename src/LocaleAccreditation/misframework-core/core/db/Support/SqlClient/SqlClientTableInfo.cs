using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.SqlClient
{
    class SqlClientTableInfo : AbstractTableInfo
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
                sbFields.Append("SqlClientTableInfo LoadInformation tableName="+tableName+":  ");
                DataTable columnInfor = dbu.GetSchema(conn, "Columns", new string[] { db_user,null, this.TableName });
                
                //读取字段基本信息
                for (int i = 0; i < columnInfor.Rows.Count; i++)
                {
                    IFieldInfo f = dbu.CreateFieldInfo();
                    f.Loadinformation(dbu, columnInfor.Rows[i]);
                    Fields.Add(f);
                    FieldsByName[f.CloumnName] = f;
                    sbFields.Append(f.CloumnName+"  :  " +f.DataTypeText+"  :   "+f.DbType.ToString()+ "   |\r\n");
                }
                sbFields.Append("    PK: \r\n");
                //读取表的主键信息
                DataTable pk_name = dbu.GetSchema(conn, "Indexes", new string[] { db_user, null, this.TableName });
                if (pk_name.Rows.Count > 0)
                {
                    DataRow pk_record = null;
                    for (int i = 0; i < pk_name.Rows.Count; i++)
                    {
                        if (pk_name.Rows[i]["index_name"].ToString().ToUpper().StartsWith("PK_"))
                        {
                            pk_record = pk_name.Rows[i];
                            break;
                        }
                    }
                    if (pk_record != null)
                    {
                        string primary_key_name = pk_record["index_name"].ToString();
                        DataTable pk_cloumns = dbu.GetSchema(conn, "IndexColumns", new string[] { db_user,null,this.TableName , primary_key_name });
                        for (int i = 0; i < pk_cloumns.Rows.Count; i++)
                        {
                            string cloumn_name = pk_cloumns.Rows[i]["column_name"].ToString().ToUpper();
                            IFieldInfo field_info = FieldsByName[cloumn_name];
                            if (field_info != null)
                            {
                                PrimaryFields[cloumn_name] = field_info;
                                sbFields.Append(field_info.CloumnName);
                            }
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
