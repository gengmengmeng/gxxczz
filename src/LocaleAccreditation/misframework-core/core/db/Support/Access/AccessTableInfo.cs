using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Access
{
    
    class AccessTableInfo : AbstractTableInfo
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
                DataTable columnInfor = dbu.GetSchema(conn, "Columns", new string[] { null,null, this.TableName });
                //读取字段基本信息
                for (int i = 0; i < columnInfor.Rows.Count; i++)
                {
                    IFieldInfo f = dbu.CreateFieldInfo();
                    f.Loadinformation(dbu, columnInfor.Rows[i]);
                    Fields.Add(f);
                    FieldsByName[f.CloumnName] = f;
                }
                //读取表的主键信息
                DataTable pk_name = dbu.GetSchema(conn, "Indexes", new string[] { null, null, "PrimaryKey" });
                if (pk_name.Rows.Count > 0)
                {
                    for (int i = 0; i < pk_name.Rows.Count; i++)
                    {
                        if (pk_name.Rows[i]["TABLE_NAME"].ToString().ToUpper().Equals(this.TableName))
                        {
                            string cloumn_name = pk_name.Rows[i]["COLUMN_NAME"].ToString();
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
