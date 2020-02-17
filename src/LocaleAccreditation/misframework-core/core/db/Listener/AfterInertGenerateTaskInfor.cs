using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data.Common;

using MisFrameWork.core;
using MisFrameWork.core.db;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Listener
{
    /// <summary>
    /// 业务表创建任务记录的监听器
    /// </summary>
    public class AfterInertGenerateTaskInfor : IDbOperateListener
    {
        string m_TaskTableName = "B_TASKS";
        public string TaskTableName
        {
            get { return m_TaskTableName; }
            set { m_TaskTableName = value; }
        }

        IList m_FieldMaps = new ArrayList();
        public IList FieldMaps
        {
            get { return m_FieldMaps; }
            set { m_FieldMaps = value; }
        }

        #region IDbOperateListener 成员

        public bool Execute(IDataBaseUtility dbu, Session session, ITableInfo tableInfo, IDictionary record, Condition where)
        {
            if (FieldMaps != null)
            {
                foreach (string FieldMap in FieldMaps)
                {
                    if (FieldMap != null && !"".Equals(FieldMap))
                    {
                        UnCaseSenseHashTable task = new UnCaseSenseHashTable();
                        string[] mapInfor = FieldMap.Split(',');
                        foreach (string m in mapInfor)
                        {
                            if ("".Equals(m))
                                continue;
                            string[] s = m.Split(':');
                            if (s.Length != 2 && s.Length != 3)
                                continue;
                            if ("EXPR".Equals(s[0]) && s.Length == 3)
                            {
                                task[s[2]] = s[1];
                            }
                            else if (s.Length == 2)
                            {
                                task[s[1]] = record[s[0]];
                            }
                        }
                        dbu.InsertRecord(session, this.TaskTableName, task);
                    }                    
                }
            }
            return true;
        }

        #endregion
    }
}
