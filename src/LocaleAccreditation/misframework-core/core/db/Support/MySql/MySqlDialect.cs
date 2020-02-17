using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace MisFrameWork.core.db.Support.MySql
{
    class MySqlDialect : AbstractDialect
    {
        protected override bool IsDbSupportPage()
        {
            return true;
        }
        public override string GetSql(string tableName, string fields, string where, string orderby, string groupby, int offset, int maxResultCount)
        {
            StringBuilder result = new StringBuilder();
            result.Append("select ");
            
            if (fields != null && !"".Equals(fields))
                result.Append(fields);
            else
                result.Append("*");
            result.Append(" from " + tableName);
            result.Append(" where " + where);
            if (groupby != null && !"".Equals(groupby))
            {
                result.Append(" group by " + groupby);
            }
            if (orderby != null && !"".Equals(orderby))
            {
                result.Append(" order by " + orderby);
            }
            if (offset >= 0 && maxResultCount >= 0)
            {
                result.Append(" limit " + offset.ToString() + ","+maxResultCount.ToString());
            }
            return result.ToString();
        }

        public override List<UnCaseSenseHashTable> DoPageDataSet(DbDataReader dbreader, int offset, int maxResultCount)
        {
            List<UnCaseSenseHashTable> result = new List<UnCaseSenseHashTable>();
            if (!dbreader.IsClosed && dbreader.HasRows)
            {
                while (dbreader.Read())
                {
                    UnCaseSenseHashTable record = new UnCaseSenseHashTable();
                    LoadDataContent(record, dbreader);
                    result.Add(record);
                }
            }
            return result;
        }
    }
}
