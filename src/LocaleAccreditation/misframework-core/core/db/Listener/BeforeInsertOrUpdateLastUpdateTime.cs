using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

using MisFrameWork.core;
using MisFrameWork.core.db;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Listener
{
    public class BeforeInsertOrUpdateLastUpdateTime : IDbOperateListener
    {
        private string m_FieldName = "LAST_UPDATE_TIME";
        public string FieldName
        {
            get { return m_FieldName; }
            set { m_FieldName = value; }
        }
        #region IDbOperateListener 成员

        public bool Execute(IDataBaseUtility dbu, Session session, ITableInfo tableInfo, System.Collections.IDictionary record, Condition where)
        {
            if (tableInfo.FieldsByName.ContainsKey(m_FieldName))
            {
                record[m_FieldName] = DateTime.Now;
            }
            return true;
        }

        #endregion
    }
}
