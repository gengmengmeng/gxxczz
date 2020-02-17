using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.Common;
using MisFrameWork.core.xml;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Support
{
    public interface IDbOperateListener
    {
        bool Execute(IDataBaseUtility dbu, Session session, ITableInfo tableInfo, System.Collections.IDictionary record, Condition where);
    }
}
