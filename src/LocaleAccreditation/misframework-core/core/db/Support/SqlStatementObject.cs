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

    [Serializable]
    public class SqlStatementObject
    {
        private List<DbParameter> parameters = new List<DbParameter>();

        public List<DbParameter> Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }
        public string Sql { get; set; }

        public SqlStatementObject() { }
        public SqlStatementObject(string sql, List<DbParameter> parameters)
        {
            this.Sql = sql;
            this.parameters = parameters;
        }
    }
}
