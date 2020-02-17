using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.MySql
{
    class MySqlFieldInfo : AbstractFieldInfo
    {
        public override void Loadinformation(IDataBaseUtility dbu, DataRow cloumnInfo)
        {
            dataBaseUtility = dbu;
            this.cloumnName = cloumnInfo["COLUMN_NAME"].ToString();
            this.dataTypeText = cloumnInfo["DATA_TYPE"].ToString().ToUpper();
            if (cloumnInfo["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                this.length = int.Parse(cloumnInfo["CHARACTER_MAXIMUM_LENGTH"].ToString());
            this.nullable = !"NO".Equals(cloumnInfo["IS_NULLABLE"].ToString().ToUpper());
            this.dbType = dbu.GetDbType(this.DataTypeText);
        }
    }
}
