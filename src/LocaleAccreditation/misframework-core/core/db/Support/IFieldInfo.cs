using System;
using System.Data;
using System.Data.Common;
namespace MisFrameWork.core.db.Support
{
    public interface IFieldInfo
    {
        string CloumnName { get; }
        string DataTypeText { get; }
        int Length { get; }
        bool Nullable { get; }
        int Precision { get; }
        DbType DbType { get; }
        bool IsPrimaryKey { get; }
        void Loadinformation(IDataBaseUtility dbu, DataRow cloumnInfo);
        DbParameter CreateParameter();
    }
}
