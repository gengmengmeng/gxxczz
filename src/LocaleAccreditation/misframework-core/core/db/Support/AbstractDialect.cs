using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace MisFrameWork.core.db.Support
{
    public abstract class AbstractDialect : MisFrameWork.core.db.Support.IDialect
    {
        protected virtual bool IsDbSupportPage()
        {
            return false;
        }
        static log4net.ILog logger = log4net.LogManager.GetLogger("AbstractDialect");
        public abstract string GetSql(string tableName, string fields, string where, string orderby, string groupby, int offset, int maxResultCount);

        public virtual List<UnCaseSenseHashTable> DoPageDataSet(DbDataReader dbreader, int offset, int maxResultCount)
        {
            List<UnCaseSenseHashTable> result = new List<UnCaseSenseHashTable>();
            if (!dbreader.IsClosed && dbreader.HasRows)
            {
                //定位到第一条记录
                int i = 0;
                if (!IsDbSupportPage())
                {
                    if (offset >= 0 || maxResultCount >= 0)
                    {                        
                        for (i = 0; i < offset && dbreader.Read(); i++) { }
                    }
                    //if (DateTime.Now>new DateTime(2019,7,1))//DEAD：给客户测试版本，故意出错。
                    //    maxResultCount++;
                    while (dbreader.Read() && ((i <= offset + maxResultCount) || (offset < 0 || maxResultCount < 0)))
                    {
                        i++;
                        UnCaseSenseHashTable record = new UnCaseSenseHashTable();
                        LoadDataContent(record, dbreader);
                        result.Add(record);
                    }
                }
                else
                {
                    while (dbreader.Read() && ((i <= maxResultCount) || (offset < 0 || maxResultCount <= 0)))
                    {
                        i++;
                        UnCaseSenseHashTable record = new UnCaseSenseHashTable();
                        LoadDataContent(record, dbreader);
                        result.Add(record);
                    }
                }
            }
            return result;
        }

        protected void LoadDataContent(Hashtable toResult, DbDataReader dbreader)
        {
            string fieldName = "";
            try
            {

                for (int i = 0; i < dbreader.FieldCount; i++)
                {
                    fieldName = dbreader.GetName(i);
                    if (DBNull.Value.Equals(dbreader.GetValue(i)))
                        toResult[fieldName] = null;
                    else
                        toResult[fieldName] = dbreader.GetValue(i);
                }
            }
            catch (Exception ee)
            {
                logger.Error("最后出错字段:" + fieldName + "," + ee.ToString());
                throw ee;
            }
        }

        public virtual string ParameterPrefix
        {
            get
            {
                return String.Intern("@");
            }
        }
    }
}
