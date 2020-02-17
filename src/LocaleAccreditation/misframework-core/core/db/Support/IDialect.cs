using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using MisFrameWork.core;

namespace MisFrameWork.core.db.Support
{
    public interface IDialect
    {
        List<UnCaseSenseHashTable> DoPageDataSet(System.Data.Common.DbDataReader dbreader, int offset, int maxResultCount);
        string GetSql(string tableName, string fields, string where, string orderby, string groupby, int offset, int maxResultCount);
        string ParameterPrefix { get; }
    }
}
