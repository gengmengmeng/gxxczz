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
    public class BeforeInsertOrUpdateAutoConvertAllDic : IDbOperateListener
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(BeforeInsertOrUpdateConvertDic));
        private IDictionary tableConvertStringMapper = new UnCaseSenseHashTable();

        public IDictionary TableConvertStringMapper
        {
            get { return tableConvertStringMapper; }
            set { tableConvertStringMapper = value; }
        }

        public BeforeInsertOrUpdateAutoConvertAllDic() { }
        #region IDbOperateListener 成员

        public bool Execute(IDataBaseUtility dbu, Session session, ITableInfo tableInfo, IDictionary record, Condition where)
        {
            UnCaseSenseHashTable dicMc = new UnCaseSenseHashTable();
            string msg = "";
            foreach (string k in record.Keys)
            {
                string fieldName = tableInfo.GetDicMC_Field(k);
                if (!String.IsNullOrEmpty(fieldName))
                {
                    if (record.Contains(k) && record[k] != null)
                    {
                        string dicTableName = fieldName.Substring(k.Length + 1, fieldName.Length - k.Length - 5);
                        string[] values = record[k].ToString().Split(',');
                        Boolean addBool = false;
                        string realDM = "";
                        string realMC = "";
                        string errorMsg = "";
                        foreach (string v in values)
                        {                            
                            string mc = dbu.Dm2Mc(session, dicTableName, v);
                            //没有这个代码的话就应该把原来的值删删掉
                            if (!String.IsNullOrEmpty(mc))
                            {
                                if (addBool)
                                {
                                    realMC += ",";
                                    realDM += ",";
                                }
                                realMC += mc;
                                realDM += v;
                                addBool = true;
                            }
                            else//如果DM没有找到的话，有可能传的是中文描述，所以也查一下有没有。
                            {
                                string dm = dbu.Mc2Dm(session, dicTableName, v);
                                if (!String.IsNullOrEmpty(dm))
                                {
                                    if (addBool)
                                    {
                                        realMC += ",";
                                        realDM += ",";
                                    }
                                    realMC += v;
                                    realDM += dm;
                                    addBool = true;
                                }
                                else
                                {
                                    errorMsg += v + ",";
                                }
                            }
                        }
                        dicMc[k] = realDM;
                        dicMc[fieldName] = realMC;
                        if (errorMsg!="")
                        {
                            msg += "转换字典错误：字段：" + k + ",表名：" + dicTableName+",错误值："+ errorMsg;
                        }
                    }
                    else
                    {
                        record[fieldName] = "";
                    }
                }
            }            
            foreach (string k in dicMc.Keys)
            {
                record[k] = dicMc[k];
            }
            if (msg != "")
                logger.Error(msg);
            return true;
        }
        #endregion
    }
}
