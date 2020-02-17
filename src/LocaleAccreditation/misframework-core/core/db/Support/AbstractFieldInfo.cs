using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace MisFrameWork.core.db.Support
{
    class AbstractFieldInfo : IFieldInfo
    {
        protected IDataBaseUtility dataBaseUtility = null;
        protected string cloumnName = null;
        protected string dataTypeText = null;
        protected int length = 0;
        protected int precision = 0;
        protected bool nullable = true;
        protected DbType dbType = DbType.String;
        protected bool isPrimaryKey = false;

        public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
        }

        public DbType DbType
        {
            get { return dbType; }
        }

        public string CloumnName
        {
            get { return cloumnName; }
        }

        public string DataTypeText
        {
            get { return dataTypeText; }
        }

        public int Length
        {
            get { return length; }
        }

        public int Precision
        {
            get { return precision; }
        }

        public bool Nullable
        {
            get { return nullable; }
        }

        #region IFieldInfo 成员


        public virtual void Loadinformation(IDataBaseUtility dbu, System.Data.DataRow cloumnInfo)
        {
            throw new NotImplementedException();
        }

        public DbParameter CreateParameter()
        {
            DbParameter result = dataBaseUtility.DbProviderFactory.CreateParameter();
            result.DbType = this.DbType;
            return result;
        }


        #endregion
    }
}
