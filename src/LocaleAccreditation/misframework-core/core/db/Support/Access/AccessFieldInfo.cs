using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Access
{
    class AccessFieldInfo : AbstractFieldInfo
    {
        static UnCaseSenseHashTable dbTypeMapper = new UnCaseSenseHashTable();
        static AccessFieldInfo()
        {
            dbTypeMapper["2"]="SHORT";
            dbTypeMapper["3"]="LONG";
            dbTypeMapper["4"]="SINGLE";
            dbTypeMapper["5"]="DOUBLE";
            dbTypeMapper["6"]="CURRENCY";
            dbTypeMapper["7"]="DATETIME";
            dbTypeMapper["11"]="BIT";
            dbTypeMapper["17"]="BYTE";
            dbTypeMapper["72"]="GUID";
            dbTypeMapper["128"]="BIGBINARY";
            dbTypeMapper["128"]="LONGBINARY";
            dbTypeMapper["128"]="VARBINARY";
            dbTypeMapper["130"]="LONGTEXT";
            dbTypeMapper["130"]="VARCHAR";
            dbTypeMapper["131"]="DECIMAL";
        }
        public override void Loadinformation(IDataBaseUtility dbu, DataRow cloumnInfo)
        {
            dataBaseUtility = dbu;
            this.cloumnName = cloumnInfo["COLUMN_NAME"].ToString().ToUpper();
            this.dataTypeText = dbTypeMapper[cloumnInfo["DATA_TYPE"]].ToString();
            if (cloumnInfo["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                this.length = int.Parse(cloumnInfo["CHARACTER_MAXIMUM_LENGTH"].ToString());
            this.nullable = !"False".Equals(cloumnInfo["IS_NULLABLE"].ToString().ToUpper());
            this.dbType = dbu.GetDbType(this.DataTypeText);
        }
    }
}
