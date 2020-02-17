using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace MisFrameWork.core.db.Support
{
    public class Session
    {
        DbConnection connection = null;

        public DbConnection Connection
        {
            get { return connection; }
        }
        List<DbTransaction> transactions = new List<DbTransaction>();

        public DbTransaction CurrentTransaction
        {
            get
            {
                if (transactions.Count > 0)
                    return transactions[0];
                else
                    return null;
            }
        }

        public void BeginTransaction()
        {
            transactions.Insert(0, connection.BeginTransaction());
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {            
            transactions.Insert(0,connection.BeginTransaction(isolationLevel));
        }

        public void Rollback()
        {
            if (transactions.Count > 0)
            {
                transactions[0].Rollback();
                transactions.RemoveAt(0);
            }
        }

        public void Commit()
        {
            if (transactions.Count > 0)
            {
                transactions[0].Commit();
                transactions.RemoveAt(0);
            }
        }

        public void Close()
        {
            if (connection != null && !connection.State.Equals(ConnectionState.Closed))
                connection.Close();
        }

        public Session(DbConnection conn)
        {
            this.connection = conn;
            if (conn.State.Equals(ConnectionState.Closed))
                conn.Open();
        }

        public ConnectionState State
        {
            get
            {
                return connection.State;
            }
        }

        public DbCommand CreateCommand()
        {
            DbCommand cmd = connection.CreateCommand();
            cmd.Connection = connection;
            if (transactions.Count > 0)
            {
                cmd.Transaction = CurrentTransaction;
            }
            return cmd;
        }
    }
}
