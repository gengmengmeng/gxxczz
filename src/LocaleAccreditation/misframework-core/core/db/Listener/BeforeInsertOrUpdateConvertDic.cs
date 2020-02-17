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
    public class BeforeInsertOrUpdateConvertDic : IDbOperateListener
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(BeforeInsertOrUpdateConvertDic));
        private IDictionary tableConvertStringMapper = new UnCaseSenseHashTable();

        public IDictionary TableConvertStringMapper
        {
            get { return tableConvertStringMapper; }
            set { tableConvertStringMapper = value; }
        }

        public BeforeInsertOrUpdateConvertDic() { }
        public BeforeInsertOrUpdateConvertDic(IDictionary tableConvertStringMapper)
        {
            this.tableConvertStringMapper = tableConvertStringMapper;
        }
        #region IDbOperateListener 成员

        public bool Execute(IDataBaseUtility dbu, Session session, ITableInfo tableInfo, IDictionary record, Condition where)
        {
            if (this.TableConvertStringMapper != null)
            {
                string s = (string)TableConvertStringMapper["*"];
                if (s != null && !"".Equals(s))
                    ConvertDicFields(dbu, session, record, s);
                s = (string)TableConvertStringMapper[tableInfo.TableName];
                if (s != null && !"".Equals(s))
                    ConvertDicFields(dbu, session, record, s);
            }
            return true;
        }

        private void ConvertDicFields(IDataBaseUtility dbu,Session session, IDictionary record, String strDicMap)
        {
            if (record != null && strDicMap != null && !"".Equals(strDicMap))
            {
                String[] s = strDicMap.Split(',');
                for (int j = 0; j < s.Count(); j++)
                {
                    string[] fms = s[j].Split(':');
                    String k = fms[0];
                    String dicName = fms[1];
                    if (record[k] != null && !"".Equals(record[k]))
                    {
                        try
                        {
                            record[k + "__MC"] = dbu.Dm2Mc(session, dicName, record[k].ToString());
                        }
                        catch (Exception e)
                        {
                            logger.Error("[" + dicName + "]转换字段：" + k + "[" + record[k] + "]时出错"+e.Message);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
