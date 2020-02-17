using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.MySql
{
    class MySqlTableInfo : AbstractTableInfo
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
                DataTable columnInfor = dbu.GetSchema(conn, "Columns", new string[] { null,conn.Database, this.TableName });
                //读取字段基本信息
                for (int i = 0; i < columnInfor.Rows.Count; i++)
                {
                    IFieldInfo f = dbu.CreateFieldInfo();
                    f.Loadinformation(dbu, columnInfor.Rows[i]);
                    Fields.Add(f);
                    FieldsByName[f.CloumnName] = f;
                }
                //读取表的主键信息
                DataTable pk_name = dbu.GetSchema(conn, "Indexes", new string[] { null,conn.Database, this.TableName });
                if (pk_name.Rows.Count > 0)
                {
                    DataRow pk_record = null;
                    for (int i = 0; i < pk_name.Rows.Count; i++)
                    {
                        if (pk_name.Rows[i]["INDEX_NAME"].ToString().ToUpper().Equals("PRIMARY") || pk_name.Rows[i]["INDEX_NAME"].ToString().ToUpper().StartsWith("PK_"))
                        {
                            pk_record = pk_name.Rows[i];
                            break;
                        }
                    }
                    if (pk_record != null)
                    {
                        string primary_key_name = pk_record["INDEX_NAME"].ToString();
                        DataTable pk_cloumns = dbu.GetSchema(conn, "IndexColumns", new string[] { null, conn.Database, this.TableName, primary_key_name });
                        for (int i = 0; i < pk_cloumns.Rows.Count; i++)
                        {
                            string cloumn_name = pk_cloumns.Rows[i]["COLUMN_NAME"].ToString().ToUpper();
                            IFieldInfo field_info = FieldsByName[cloumn_name];
                            if (field_info != null)
                            {
                                PrimaryFields[cloumn_name] = field_info;
                            }
                        }
                    }
                }

            }
            finally
            {
                conn.Close();
            }
        }
    }
}
