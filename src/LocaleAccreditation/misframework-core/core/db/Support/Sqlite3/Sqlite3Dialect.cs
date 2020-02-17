using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MisFrameWork.core.db.Support.Sqlite3
{
    class Sqlite3Dialect : AbstractDialect
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
                result.Append(" limit " + maxResultCount.ToString() + " Offset " + offset.ToString());
            }
            return result.ToString();
        }
    }
}
