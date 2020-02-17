using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MisFrameWork.core;
using MisFrameWork.core.db;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Listener
{
    public class BeforeInsertLoadTakeCardTime : IDbOperateListener
    {
        static Condition emptyCdt = new Condition();
        public bool Execute(IDataBaseUtility dbu, Session session, ITableInfo tableInfo, System.Collections.IDictionary record, Condition where)
        {
            DateTime now = new DateTime();
            if (tableInfo.FieldsByName.ContainsKey("FIRST_TAKE_TIME"))
            {
                if ((record["FIRST_TAKE_TIME"] == null) || "".Equals(record["FIRST_TAKE_TIME"]))
                {

                    List<UnCaseSenseHashTable> rds = dbu.Query(tableInfo.TableName, emptyCdt, "FIRST_TAKE_TIME", "LAST_TAKE_TIME", null, 0, 2);
                    if ((rds == null) || (rds.Count == 0))
                    {
                        record["FIRST_TAKE_TIME"] = now;
                    }
                    else
                    {
                        record["LAST_TAKE_TIME"] = now;
                        record["TAKECARD_COUNT"] = rds[0].GetDecimalValue("TAKECARD_COUNT") + 1;
                    }

                }
                else
                {
                    record["FIRST_TAKE_TIME"] = now;
                }

            }
            return true;
        }
    }
}
