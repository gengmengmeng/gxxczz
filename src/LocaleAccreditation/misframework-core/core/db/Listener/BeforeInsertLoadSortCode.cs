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
    public class BeforeInsertLoadSortCode : IDbOperateListener
    {
        static Condition emptyCdt = new Condition();//不想整天重复创建，所以建一个全局的。            
        public bool Execute(IDataBaseUtility dbu, Session session, ITableInfo tableInfo, System.Collections.IDictionary record, Condition where)
        {            
            if (tableInfo.FieldsByName.ContainsKey("SORT_CODE"))
            {
                if ((record["SORT_CODE"] == null)||"".Equals(record["SORT_CODE"]))
                {
                    List<UnCaseSenseHashTable> rds = dbu.Query(tableInfo.TableName, emptyCdt, "SORT_CODE", "SORT_CODE DESC", null, 0, 2);
                    if ((rds == null) || (rds.Count == 0))
                        record["SORT_CODE"] = 0;
                    else
                        record["SORT_CODE"] = rds[0].GetDecimalValue("SORT_CODE") + 1;
                }
            }
            return true;
        }
    }
}
