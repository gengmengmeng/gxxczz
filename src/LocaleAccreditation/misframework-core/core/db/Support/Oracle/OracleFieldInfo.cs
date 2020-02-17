using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Oracle
{
    class OracleFieldInfo : AbstractFieldInfo
    {
        
        public override void Loadinformation(IDataBaseUtility dbu, DataRow cloumnInfo)
        {
            dataBaseUtility = dbu;
            this.cloumnName = cloumnInfo["COLUMN_NAME"].ToString();
            this.dataTypeText = cloumnInfo["DATATYPE"].ToString().ToUpper();
            this.length = int.Parse(cloumnInfo["LENGTH"].ToString());
            if (cloumnInfo["PRECISION"]!=DBNull.Value)
                this.precision = int.Parse(cloumnInfo["PRECISION"].ToString());
            this.nullable = !"N".Equals(cloumnInfo["NULLABLE"].ToString());
            this.dbType = dbu.GetDbType(this.DataTypeText);
        }
    }
}
