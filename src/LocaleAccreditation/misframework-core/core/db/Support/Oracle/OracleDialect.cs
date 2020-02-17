using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support.Oracle
{
    class OracleDialect : AbstractDialect
    {
        public override string GetSql(string tableName, string fields, string where, string orderby, string groupby, int offset, int maxResultCount)
        {
            StringBuilder result = new StringBuilder();
            result.Append("select ");
            if (fields != null && !"".Equals(fields))
                result.Append(fields);
            else
                result.Append("*");
            result.Append(" from " + tableName);
            result.Append(" where "+where);
            if (groupby != null && !"".Equals(groupby))
            {
                result.Append(" group by " + groupby);
            }
            if (orderby != null && !"".Equals(orderby))
            {
                result.Append(" order by "+ orderby);
            }
            if (offset >= 0 && maxResultCount >= 0)
            {
                return "select * from (" + result.ToString() + ") where rownum <= " + (offset + maxResultCount).ToString() ;
            }
            return result.ToString();
        }

        public override string ParameterPrefix
        {
            get
            {
                return String.Intern(":");
            }
        }
    }
}
