using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using MisFrameWork.core.db;
namespace MisFrameWork.core.db.Support
{
    public interface ITableInfo
    {
        void LoadInformation(IDataBaseUtility dbu, string tableName);
        string TableName { get; }
        string GetXmlString();
        bool HaveVerField { get; }
        string GetDicMC_Field(string fieldName);
        IDataBaseUtility DataBaseUtility { get; }
        List<IFieldInfo> Fields { get; }
        SortedDictionary<String, IFieldInfo> FieldsByName { get; }
        SortedDictionary<String, IFieldInfo> PrimaryFields { get; }

        SqlStatementObject GetInsertSqlStatement(System.Collections.IDictionary record);
        SqlStatementObject GetUpdateSqlStatement(System.Collections.IDictionary record, Condition where, bool checkVer);
        SqlStatementObject GetDeleteSqlStatement(Condition where);
        Condition GetPrimaryCondition(System.Collections.IDictionary record);
    }
}
