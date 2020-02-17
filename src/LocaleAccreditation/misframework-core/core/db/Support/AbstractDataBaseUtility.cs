using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using MisFrameWork.core.db;
using MisFrameWork.core.db.Listener;


namespace MisFrameWork.core.db.Support 
{
    public abstract class AbstractDataBaseUtility : IDataBaseUtility
    {
        protected static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(AbstractDataBaseUtility));
        protected DbProviderFactory dbProviderFactory;
        protected string connectionString = null;
        protected SortedDictionary<string, ITableInfo> tables = new SortedDictionary<string, ITableInfo>();
        protected SortedDictionary<String, DbType> dbTypeMapper = null;
        

        public DbProviderFactory DbProviderFactory
        {
            get { return dbProviderFactory; }
        }

        //要创建一个表，包含”KVALUE"字段，并且至少有一条记录。
        public Session LockDbTable(string name)
        {
            UnCaseSenseHashTable us = new UnCaseSenseHashTable();
            us["KNAME"] = name;
            us["KVALUE"] = DateTime.Now.ToString();
            Session s = this.CreateAndOpenSession();
            s.BeginTransaction(IsolationLevel.ReadCommitted);
            this.Execute(s,"update S_LOCKER_"+name+" set kvalue=1",null);
            //this.GetOneRecord(s, "S_LOCKER", us);
            return s;
        }

        public void UnlockDbTable(Session s)
        {
            s.Rollback();
            s.Close();
        }

        public ITableInfo CreateTableInfo(string tableName)
        {
            string t = tableName.ToUpper();
            if (tables.ContainsKey(t))
                return tables[t];

            ITableInfo table = this.CreateTableInfo();
            table.LoadInformation(this, t);
            tables[t] = table;
            return table;
        }

        protected abstract SortedDictionary<String, DbType> GetDbTypeMapper();//类型的对照表，必须重写
       
        protected abstract ITableInfo CreateTableInfo();

        #region IDataBaseUtility 成员
        private bool useStaticSqlStatement = false;
        public bool UseStaticSqlStatement
        {
            get { return useStaticSqlStatement;}
            set { useStaticSqlStatement = value; }
        }
        DbOperateListenerManager dbOperateListenerManager = new DbOperateListenerManager();
        public DbOperateListenerManager DbOperateListenerManager
        {
            get { return dbOperateListenerManager; }
            set { dbOperateListenerManager = value; }
        }

        public Boolean mIsOutputSql = false;
        public Boolean IsOutputSql
        {
            get { return mIsOutputSql; }
            set { mIsOutputSql = value; }
        }

        public UnCaseSenseHashTable GetOneRecord(string tableName, Object single_key)
        {
            return GetOneRecord(tableName, single_key, null);
        }

        public UnCaseSenseHashTable GetOneRecord(string tableName, Object single_key,string fields)
        {
            ITableInfo ti = this.CreateTableInfo(tableName);
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht[ti.PrimaryFields.Keys.ElementAt(0)] = single_key;
            return GetOneRecord(tableName, ht, fields);
        }


        public UnCaseSenseHashTable GetOneRecord(string tableName, System.Collections.IDictionary key)
        {
            return GetOneRecord(tableName, key, null);
        }

        public UnCaseSenseHashTable GetOneRecord(string tableName, System.Collections.IDictionary key,string fields)
        {
            Session session = this.CreateAndOpenSession();
            try
            {
                return GetOneRecord(session, tableName, key,fields);
            }
            finally
            {
                session.Close();
            }
        }

        public UnCaseSenseHashTable GetOneRecord(Session session, string tableName, System.Collections.IDictionary key, string fields)
        {
            ITableInfo ti = this.CreateTableInfo(tableName);
            if (ti.FieldsByName.ContainsKey("DELETED_MARK"))
                key["DELETED_MARK"] = 0;
            Condition cl = new Condition();
            foreach (string k in ti.PrimaryFields.Keys)
            {
                if (!key.Contains(k))
                {
                    logger.Error(tableName + " 获取单条记录时出错！,主键值不足.");
                    throw new Exception("主键值不足：" + k);
                }
                cl.SubConditions.Add(new Condition("and", k, "=", key[k]));
            }
            List<UnCaseSenseHashTable> records = Query(session, tableName, cl, fields, null);
            if (records.Count <= 0)
            {
                string k_string = "";
                foreach (string k in ti.PrimaryFields.Keys)
                {
                    k_string += key[k] + ",";
                }
                NullReferenceException e = new NullReferenceException("表名："+tableName+"，键值:"+k_string + " 记录已被删除！！");
                if (key!=null && key is UnCaseSenseHashTable)
                    logger.Error(tableName + " 获取单条记录时出错！KEY="+((UnCaseSenseHashTable)key).ToJsonString(false), e);
                else
                    logger.Error(tableName+" 获取单条记录时出错！", e);
                throw e;
            }
            return records[0];
        }

        public UnCaseSenseHashTable GetOneRecord(Session session, string tableName, System.Collections.IDictionary key)
        {
            return GetOneRecord(session, tableName, key, null);
        }

        public int RecordCount(string tableName, Condition condition)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return RecordCount(s,tableName, condition);
            }
            finally
            {
                s.Close();
            }
        }
        public int RecordCount(Session s,string tableName, Condition condition)
        {
            List<UnCaseSenseHashTable> r = this.Query(s,tableName, condition, "count(*) as cnt", null, null, -1, -1);
            if (r.Count() > 0)
                return int.Parse(r[0]["CNT"].ToString());
            else
                return 0;
        }
        public List<UnCaseSenseHashTable> Query( string tableName, Condition condition, string fields, string orderby)
        {
            return this.Query(tableName, condition, fields, orderby, null, -1, -1);
        }
        public List<UnCaseSenseHashTable> Query(Session session, string tableName, Condition condition, string fields, string orderby)
        {
            return this.Query(session, tableName, condition, fields, orderby,null, -1, -1);
        }

        public List<UnCaseSenseHashTable> Query(string tableName, Condition condition, string fields, string orderby, string groupby, int offset, int maxResultCount)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return Query(s, tableName, condition, fields, orderby, groupby, offset, maxResultCount);
            }
            finally
            {
                s.Close();
            }
        }
        public List<UnCaseSenseHashTable> Query(Session session, string tableName, Condition condition, string fields, string orderby, string groupby, int offset, int maxResultCount)
        {
            List<UnCaseSenseHashTable> result = null;
            if (condition == null)
                condition = new Condition();
            SqlStatementObject where = condition.GetSqlStatementObject(this,tableName,null);
            string query_sql = this.Dialect.GetSql(tableName, fields, where.Sql, orderby, groupby, offset, maxResultCount);
            DbCommand cmd = session.CreateCommand();          
            if (this.UseStaticSqlStatement)//如果使用静态语句的话，就转换
            {
                query_sql = this.ConvertParamsToStaticString(cmd,query_sql, where.Parameters);
            }
            else //如果不转换的，把当前的参数加入到command对象当中。
            {
                foreach (DbParameter p in where.Parameters)
                {
                    cmd.Parameters.Add(p);
                }
            }
            cmd.CommandText = query_sql;
            //记录日志
            if (IsOutputSql && logger.IsDebugEnabled)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(query_sql + "\r\n");
                if (where.Parameters != null)
                {
                    foreach (DbParameter p in where.Parameters)
                    {
                        sb.Append("\r\n" + p.ParameterName + "[" + p.DbType.ToString() + "]" + ":" + p.Value);
                    }
                }
                logger.Debug(sb.ToString());
            }
            
            try
            {
                DbDataReader dbreader = cmd.ExecuteReader();
                try
                {
                    result = this.Dialect.DoPageDataSet(dbreader, offset, maxResultCount);
                }
                finally
                {
                    dbreader.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                return new List<UnCaseSenseHashTable>();
            }
            return result;
        }

        public List<UnCaseSenseHashTable> Query(string query_sql, int offset, int maxResultCount)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return this.Query(s, query_sql, offset, maxResultCount);
            }
            finally
            {
                s.Close();
            }
        }

        public List<UnCaseSenseHashTable> Query(Session session, string query_sql, int offset, int maxResultCount)
        {
            return Query(session, query_sql, null, -1, -1);
        }

        public List<UnCaseSenseHashTable> Query(Session session, string query_sql,List<DbParameter> parameters, int offset, int maxResultCount)
        {
            List<UnCaseSenseHashTable> result = null;
            
            DbCommand cmd = session.CreateCommand();
            cmd.CommandText = query_sql;
            try
            {
                if (parameters != null)
                {
                    foreach(DbParameter p in parameters)
                        cmd.Parameters.Add(p);
                }
                DbDataReader dbreader = cmd.ExecuteReader();
                try
                {
                    result = this.Dialect.DoPageDataSet(dbreader, offset, maxResultCount);
                }
                finally
                {
                    dbreader.Close();
                }
                if (parameters != null)
                {
                    foreach (DbParameter p in parameters)
                        cmd.Parameters.Remove(p);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw e;
            }
            return result;
        }

        public int Execute(Session session, string query_sql, List<DbParameter> parameters)
        {
             int result = 0;

            DbCommand cmd = session.CreateCommand();
            cmd.CommandText = query_sql;
            try
            {
                if (logger.IsDebugEnabled)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(query_sql + "\r\n");
                    if (parameters != null)
                    {
                        foreach (DbParameter p in parameters)
                        {
                            sb.Append("\r\n" + p.ParameterName + "[" + p.DbType.ToString() + "]" + ":" + p.Value);
                        }
                    }
                    logger.Debug(sb.ToString());
                }
                if (parameters != null)
                {
                    foreach (DbParameter p in parameters)
                        cmd.Parameters.Add(p);
                }
                result = cmd.ExecuteNonQuery();
                if (parameters != null)
                {
                    foreach (DbParameter p in parameters)
                        cmd.Parameters.Remove(p);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw e;
            }
            return result;
        }

        public int Execute(string query_sql, List<DbParameter> parameters)
        {
            Session s = this.CreateAndOpenSession();
            int result = Execute(s,query_sql, null);
            return result;
        }
        public int Execute(string query_sql)
        {
            return Execute(query_sql, null);
        }

        public List<UnCaseSenseHashTable> QueryOne2Many(List<UnCaseSenseHashTable> records, string primaryTabFK, string manyTab, string manyTabPK, string fields, bool isMerge, string newPropertyPrefix)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return this.QueryOne2Many(s, records, primaryTabFK, manyTab, manyTabPK, fields, isMerge, newPropertyPrefix);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                s.Close();
            }
        }
        public List<UnCaseSenseHashTable> QueryOne2Many(Session session, List<UnCaseSenseHashTable> records, string primaryTabFK, string manyTab, string manyTabPK, string fields, bool isMerge, string newPropertyPrefix)
        {
            if (newPropertyPrefix == null)
                newPropertyPrefix = "";
            List<UnCaseSenseHashTable> result = new List<UnCaseSenseHashTable>();
            Condition cdt = new Condition("and", manyTabPK, "=", "");
            foreach (System.Collections.IDictionary r in records)
            {
                cdt.Tag = r[primaryTabFK];
                IList<UnCaseSenseHashTable> manyTabRecords = this.Query(session, manyTab, cdt, fields, null);
                if (manyTabRecords.Count > 0)
                {
                    foreach (UnCaseSenseHashTable mr in manyTabRecords)
                    {
                        UnCaseSenseHashTable ouputRecord = new UnCaseSenseHashTable();
                        ouputRecord.LoadFormDictionary(r, false);
                        if (isMerge)
                        {
                            ouputRecord.LoadFormDictionary(mr, null, newPropertyPrefix, true);
                        }
                        else
                        {
                            ouputRecord[newPropertyPrefix + manyTab] = mr;
                        }
                        result.Add(ouputRecord);
                    }
                }
                else
                {
                    UnCaseSenseHashTable ouputRecord = new UnCaseSenseHashTable();
                    ouputRecord.LoadFormDictionary(r, false);
                    result.Add(ouputRecord);
                }
            }
            return result;
        }

        #region Insert Update Delete 等重载方法

        public int UpdateRecord(string tableName, System.Collections.IDictionary record, bool checkVer)
        {
            Session session = this.CreateAndOpenSession();
            session.BeginTransaction();
            try
            {
                int result = UpdateRecord(session, tableName, record, checkVer);
                session.Commit();
                return result;
            }
            catch (Exception e)
            {
                session.Rollback();
                logger.Error("UpdateRecord "+tableName,e);
                logger.Debug("UpdateRecord "+tableName,e);
                throw e;
            }
            finally
            {
                session.Close();
            }
        }

        public int UpdateRecord(Session session, string tableName, System.Collections.IDictionary record, bool checkVer)
        {
            ITableInfo tableInfo = this.CreateTableInfo(tableName);
            Condition where = tableInfo.GetPrimaryCondition(record);
            int result = this.UpdateRecord(session, tableName, record, where, checkVer);
            if (result >= 1 && tableInfo.HaveVerField && record.Contains("VER"))//有版本字段的，要重新获取
            {
                if (checkVer)
                    record["VER"] = int.Parse(record["VER"].ToString()) + 1;
                else//如果保存时不检查版本，就从数据库里查重新查找
                {
                    UnCaseSenseHashTable verRecord = this.Query(session, tableName,where,"VER",null)[0];
                    record["VER"] = verRecord["VER"];
                }
            }
            return result;
        }

        public int SoftDeleteRecord(string tableName, System.Collections.IDictionary record)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return SoftDeleteRecord(s, tableName, record);
            }
            finally
            {
                s.Close();
            }
        }

        public int SoftDeleteRecord(Session session, string tableName, System.Collections.IDictionary record)
        {
            ITableInfo ti = CreateTableInfo(tableName);
            if (ti.FieldsByName.ContainsKey("DELETED_MARK"))
            {
                record["DELETED_MARK"] = 1;
                return UpdateRecord(session, tableName, record, false);
            }
            return DeleteRecord(session, tableName, record);
        }

        public int SoftDeleteRecord(Session session, string tableName, Condition delCdt)
        {
            int result = 0;
            int updateCnt = 0;
            List<UnCaseSenseHashTable> rs = this.Query(session, tableName, delCdt,"*",null);
            if (rs != null)
            {
                foreach (UnCaseSenseHashTable r in rs)
                {
                    result = this.SoftDeleteRecord(session, tableName, r);
                    if (result == 0)
                    {
                        updateCnt = 0;
                        break;
                    }
                    else
                        updateCnt++;
                }
            }
            return updateCnt;
        }

        public int SoftDeleteRecord(string tableName, Condition delCdt)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return SoftDeleteRecord(s, tableName, delCdt);
            }
            finally
            {
                s.Close();
            }
        }

        public int DeleteRecord(Session session, string tableName, System.Collections.IDictionary record)
        {
            ITableInfo tableInfo = this.CreateTableInfo(tableName);
            Condition where = tableInfo.GetPrimaryCondition(record);
            int result = this.DeleteRecord(session, tableName,record,where);
            if (result <= 0)
                throw new Exception("该记录已删除:"+record[tableInfo.PrimaryFields.ElementAt(0).Value.CloumnName].ToString());
            return result;
        }

        public int DeleteRecord(String tableName, System.Collections.IDictionary record)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return DeleteRecord(s, tableName, record);
            }
            finally
            {
                s.Close();
            }
        }

        #endregion

        public string Dm2Mc(string tableName, string dm)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return this.Dm2Mc(s, tableName, dm);
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw e;
            }
            finally
            {
                s.Close();
            }
        }

        public string Dm2Mc(Session s,string tableName, string dm)
        {
            System.Text.StringBuilder result = new StringBuilder();
            if (dm == null || "".Equals(dm))
                return result.ToString();
            else
            {
                string[] dms = dm.Split(',');
                string dm_v = "";
                string comma = "";
                foreach (string d in dms)
                {
                    dm_v += comma + "'" + d + "'";
                    comma = String.Intern(",");
                }
                string sql_where = "DM in (" + dm_v + ")";
                DbCommand cmd = s.CreateCommand();
                cmd.CommandText = "Select MC as MC from " + tableName + " where " + sql_where;
                try
                {
                    DbDataReader dr = cmd.ExecuteReader();
                    comma = "";
                    while (dr.Read())
                    {
                        result.Append(comma + dr["MC"]);
                        comma = String.Intern(",");
                    }
                    dr.Close();
                }
                catch (Exception e)
                {
                    logger.Error("转换字典：" + tableName + " 字典值：" + dm + "。",e);
                    throw e;
                }
                return result.ToString();
            }
        }

        public string Mc2Dm(Session s, string tableName, string mc)
        {
            if (mc == null || "".Equals(mc))
                return "";
            else
            {
                try
                {
                    Condition cdt = new Condition("AND","MC","=",mc);
                    List<UnCaseSenseHashTable> records = this.Query(s, tableName, cdt, "*", null);
                    if (records == null || records.Count == 0)
                        return "";
                    return records[0]["DM"].ToString();
                }
                catch (Exception e)
                {
                    logger.Error("转换字典：" + tableName + " 字典名称：" + mc + "。", e);
                    throw e;
                }
            }
        }

        public List<UnCaseSenseHashTable> Dm2Record(string tableName, string dm)
        {
            if (dm == null || "".Equals(dm))
                return new List<UnCaseSenseHashTable>();
            else
            {
                string[] dms = dm.Split(',');
                string dm_v = "";
                string comma = "";
                foreach (string d in dms)
                {
                    dm_v += comma + "'" + d + "'";
                    comma = String.Intern(",");
                }
                Condition c = new Condition("and", "DM", "in", "EXPR:(" + dm_v + ")");
                Session s = this.CreateAndOpenSession();
                try
                {
                    return this.Query(s, tableName, c, "*", "DM");
                }
                catch (Exception e)
                {
                    logger.Error("转换字典：" + tableName + " 字典值：" + dm + "。",e);
                    throw e;
                }
                finally
                {
                    s.Close();
                }
            }
        }

        

        public int InsertRecord(String tableName, System.Collections.IDictionary record)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return InsertRecord(s,tableName, record);
            }
            finally
            {
                s.Close();
            }
        }

        public int InsertRecord(Session session, String tableName, System.Collections.IDictionary record)
        {
            ITableInfo tableInfo = this.CreateTableInfo(tableName);
            SqlStatementObject sso = null;
            string actionSql = "";
            int result = -1;
            //避免数据库中这些必要字段没有设置默认值 
            record["DELETED_MARK"] = 0;
            record["VER"] = 1;
            try
            {
                bool do_not_cancel = true;//是否取消执行当前这条
                List<IDbOperateListener> beforelist = this.DbOperateListenerManager.GetListenerList("*", DbOperateListenerManager.LISTENSER_TYPE_BEFORE_INSERT);
                foreach (IDbOperateListener listener in beforelist)
                {
                    if (IsOutputSql)
                        logger.Debug("执行了:" + listener.ToString());
                    do_not_cancel = do_not_cancel && listener.Execute(this, session, tableInfo, record, null);
                    if (!do_not_cancel)
                        break;
                }
                beforelist = this.DbOperateListenerManager.GetListenerList(tableInfo.TableName, DbOperateListenerManager.LISTENSER_TYPE_BEFORE_INSERT);
                foreach (IDbOperateListener listener in beforelist)
                {
                    if (IsOutputSql)
                        logger.Debug("执行了:" + listener.ToString());
                    do_not_cancel = do_not_cancel && listener.Execute(this, session, tableInfo, record, null);
                    if (!do_not_cancel)
                        break;
                }
                if (do_not_cancel)
                {
                    sso = tableInfo.GetInsertSqlStatement(record);
                    DbCommand cmd = session.CreateCommand();
                    actionSql = sso.Sql;
                    if (this.UseStaticSqlStatement)
                    {
                        actionSql = this.ConvertParamsToStaticString(cmd,sso.Sql, sso.Parameters);
                    }
                    else
                    {
                        foreach (DbParameter p in sso.Parameters)
                            cmd.Parameters.Add(p);
                    }
                    cmd.CommandText = actionSql;
                    //记录日志
                    if (IsOutputSql && logger.IsDebugEnabled)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(actionSql + "\r\n");
                        if (sso.Parameters != null)
                        {
                            foreach (DbParameter p in sso.Parameters)
                            {
                                sb.Append("\r\n" + p.ParameterName + "[" + p.DbType.ToString() + "]" + ":" + p.Value);
                            }
                        }
                        logger.Debug(sb.ToString());
                    }

                    result = cmd.ExecuteNonQuery();

                    List<IDbOperateListener> afterListner = this.DbOperateListenerManager.GetListenerList("*", DbOperateListenerManager.LISTENSER_TYPE_AFTER_INSERT);
                    foreach (IDbOperateListener listener in afterListner)
                    {
                        if (IsOutputSql)
                            logger.Debug("执行了:" + listener.ToString());
                        listener.Execute(this, session, tableInfo, record, null);
                    }
                    afterListner = this.DbOperateListenerManager.GetListenerList(tableInfo.TableName, DbOperateListenerManager.LISTENSER_TYPE_AFTER_INSERT);
                    foreach (IDbOperateListener listener in afterListner)
                    {
                        if (IsOutputSql)
                            logger.Debug("执行了:" + listener.ToString());
                        listener.Execute(this, session, tableInfo, record, null);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("出错：" + actionSql + "\r\n");
                if (sso != null)
                {
                    foreach (DbParameter p in sso.Parameters)
                    {
                        sb.Append(p.ParameterName + "[" + p.DbType.ToString() + "]" + ":" + p.Value + "\r\n");
                    }
                }
                logger.Error(sb, e);
                throw e;
            }
        }

        /// <summary>
        /// 这种修改方法不调用触发器
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <param name="record"></param>
        /// <param name="where"></param>
        /// <param name="checkVer"></param>
        /// <returns></returns>
        public int UpdateRecord(Session session, string tableName, System.Collections.IDictionary record, Condition where, bool checkVer)
        {
            ITableInfo tableInfo = this.CreateTableInfo(tableName);
            SqlStatementObject sso = null;
            string actionSql = "";
            int result = -1;
            try
            {
                bool do_not_cancel = true;//是否取消执行当前这条
                List<IDbOperateListener> beforelist = this.DbOperateListenerManager.GetListenerList("*", DbOperateListenerManager.LISTENSER_TYPE_BEFORE_UPDATE);
                foreach (IDbOperateListener listener in beforelist)
                {
                    if (IsOutputSql)
                        logger.Debug("Update 执行了:" + listener.ToString());
                    do_not_cancel = do_not_cancel && listener.Execute(this, session, tableInfo, record, where);
                    if (!do_not_cancel)
                        break;
                }
                beforelist = this.DbOperateListenerManager.GetListenerList(tableInfo.TableName, DbOperateListenerManager.LISTENSER_TYPE_BEFORE_UPDATE);
                foreach (IDbOperateListener listener in beforelist)
                {
                    if (IsOutputSql)
                        logger.Debug("Update 执行了:" + listener.ToString());
                    do_not_cancel = do_not_cancel && listener.Execute(this, session, tableInfo, record, where);
                    if (!do_not_cancel)
                        break;
                }
                if (do_not_cancel)
                {
                    sso = tableInfo.GetUpdateSqlStatement(record,where,checkVer);
                    DbCommand cmd = session.CreateCommand();
                    actionSql = sso.Sql;
                    if (this.UseStaticSqlStatement)
                    {
                        actionSql = this.ConvertParamsToStaticString(cmd, actionSql, sso.Parameters);
                    }
                    else
                    {
                        foreach (DbParameter p in sso.Parameters)
                            cmd.Parameters.Add(p);
                    }
                    cmd.CommandText = actionSql;


                    //记录日志
                    if (IsOutputSql && logger.IsDebugEnabled)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(actionSql + "\r\n");
                        if (sso.Parameters != null)
                        {
                            foreach (DbParameter p in sso.Parameters)
                            {
                                sb.Append("\r\n" + p.ParameterName +"["+p.DbType.ToString()+"]" + ":" + p.Value);
                            }
                        }
                        logger.Debug(sb.ToString());
                    }
                    result = cmd.ExecuteNonQuery();

                    List<IDbOperateListener> afterListner = this.DbOperateListenerManager.GetListenerList("*", DbOperateListenerManager.LISTENSER_TYPE_AFTER_UPDATE);
                    foreach (IDbOperateListener listener in afterListner)
                    {
                        if (IsOutputSql)
                            logger.Debug("执行了:" + listener.ToString());
                        listener.Execute(this, session, tableInfo, record, where);
                    }
                    afterListner = this.DbOperateListenerManager.GetListenerList(tableInfo.TableName, DbOperateListenerManager.LISTENSER_TYPE_AFTER_UPDATE);
                    foreach (IDbOperateListener listener in afterListner)
                    {
                        if (IsOutputSql)
                            logger.Debug("执行了:" + listener.ToString());
                        listener.Execute(this, session, tableInfo, record, where);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("更新" + tableName + "出错\r\n");
                sb.Append("出错：" + actionSql + "\r\n");
                foreach (DbParameter p in sso.Parameters)
                {
                    sb.Append(p.ParameterName + "[" + p.DbType.ToString() + "]" + ":" + p.Value + "\r\n");
                }
                logger.Error(sb.ToString(), e);
                throw e;
            }
        }


        /// <summary>
        /// 这种删除方式不调用触发器
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int DeleteRecord(Session session, string tableName, System.Collections.IDictionary record, Condition where)
        {
            ITableInfo tableInfo = this.CreateTableInfo(tableName);
            int result = -1;
            try
            {
                bool do_not_cancel = true;//是否取消执行当前这条
                List<IDbOperateListener> beforelist = this.DbOperateListenerManager.GetListenerList("*", DbOperateListenerManager.LISTENSER_TYPE_BEFORE_DELETE);
                foreach (IDbOperateListener listener in beforelist)
                {
                    if (IsOutputSql)
                        logger.Debug("执行了:" + listener.ToString());
                    do_not_cancel = do_not_cancel && listener.Execute(this, session, tableInfo, record, null);
                    if (!do_not_cancel)
                        break;
                }
                beforelist = this.DbOperateListenerManager.GetListenerList(tableInfo.TableName, DbOperateListenerManager.LISTENSER_TYPE_BEFORE_DELETE);
                foreach (IDbOperateListener listener in beforelist)
                {
                    if (IsOutputSql)
                        logger.Debug("执行了:" + listener.ToString());
                    do_not_cancel = do_not_cancel && listener.Execute(this, session, tableInfo, record, null);
                    if (!do_not_cancel)
                        break;
                }
                if (do_not_cancel)
                {
                    SqlStatementObject sso = tableInfo.GetDeleteSqlStatement(where);
                    DbCommand cmd = session.CreateCommand();
                    cmd.CommandText = sso.Sql;
                    foreach (DbParameter p in sso.Parameters)
                        cmd.Parameters.Add(p);

                    //记录日志
                    if (IsOutputSql && logger.IsDebugEnabled)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(sso.Sql + "\r\n");
                        if (sso.Parameters != null)
                        {
                            foreach (DbParameter p in sso.Parameters)
                            {
                                sb.Append("\r\n" + p.ParameterName + "[" + p.DbType.ToString() + "]" + ":" + p.Value);
                            }
                        }
                        logger.Debug(sb.ToString());
                    }
                    result = cmd.ExecuteNonQuery();

                    List<IDbOperateListener> afterListner = this.DbOperateListenerManager.GetListenerList("*", DbOperateListenerManager.LISTENSER_TYPE_AFTER_DELETE);
                    foreach (IDbOperateListener listener in afterListner)
                    {
                        listener.Execute(this, session, tableInfo, record, null);
                    }
                    afterListner = this.DbOperateListenerManager.GetListenerList(tableInfo.TableName, DbOperateListenerManager.LISTENSER_TYPE_AFTER_DELETE);
                    foreach (IDbOperateListener listener in afterListner)
                    {
                        listener.Execute(this, session, tableInfo, record, null);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 清空数据库中表信息的缓存，如果修改了数据库结构后，要重新启运服务器，获调用这个方法。
        /// </summary>
        public void initCache()
        {
            tables.Clear();
        }

        public abstract IDialect Dialect
        {
            get;
        }
        public DbType GetDbType(string type)
        {
            string t = type.ToUpper();
            if (GetDbTypeMapper().ContainsKey(t))
                return GetDbTypeMapper()[t];
            return DbType.String;
        }

        public virtual string DataBaseRestricion(DbConnection conn)
        {
            throw new NotImplementedException();
        }

        public abstract IFieldInfo CreateFieldInfo();

        public DataTable GetAllTableInfo(DbConnection conn)
        {
            string u = this.DataBaseRestricion(conn);
            bool closed = !conn.State.Equals(ConnectionState.Open);
            if (closed)
                conn.Open();
            DataTable result = conn.GetSchema("tables", new string[] { u, null });
            if (closed)
                conn.Close();
            return result;
        }

        public DataTable GetSchema(string collectionName, string[] restrictValue)
        {
            Session s = this.CreateAndOpenSession();
            try
            {
                return GetSchema(s.Connection, collectionName, restrictValue);
            }
            finally
            {
                s.Close();
            }
        }
        public DataTable GetSchema(DbConnection conn, string collectionName, string[] restrictValue)
        {
            bool closed = !conn.State.Equals(ConnectionState.Open);
            if (closed)
                conn.Open();
            DataTable result;
            if (collectionName == null || "".Equals(collectionName))
                result = conn.GetSchema();
            else
                result = conn.GetSchema(collectionName, restrictValue);
            if (closed)
                conn.Close();
            return result;
        }

        public UnCaseSenseHashTable GetDatabaseSchemaInfo()
        {
            UnCaseSenseHashTable result = new UnCaseSenseHashTable();
            Session s = this.CreateAndOpenSession();
            try
            {
                string db_user = this.DataBaseRestricion(s.Connection);
                DataTable dtAllTable = GetAllTableInfo(s.Connection);
                foreach(DataRow drTable in dtAllTable.Rows)
                {
                    UnCaseSenseHashTable table = new UnCaseSenseHashTable();
                    //找字段
                    DataTable dtAllField = this.GetSchema(s.Connection, "Columns", new string[] { db_user,null, drTable["TABLE_NAME"].ToString() });
                    foreach(DataRow drField in dtAllField.Rows)
                    {
                        table[drField["COLUMN_NAME"]] = drField["DATA_TYPE"].ToString().Trim()+ ",default="+drField["COLUMN_DEFAULT"]+",len="+drField["CHARACTER_MAXIMUM_LENGTH"]+",nullable="+drField["IS_NULLABLE"];
                    }
                    result[drTable["TABLE_NAME"]]=table;
                }
            }
            finally
            {
                s.Close();
            }
            return result;
        }

        public DbConnection CreateConnection()
        {
            DbConnection result = this.DbProviderFactory.CreateConnection();
            result.ConnectionString = connectionString;
            return result;
        }

        public Session CreateAndOpenSession()
        {
            return new Session(CreateConnection());
        }

        public virtual string ConvertParamsToStaticString(DbCommand cmd,string sql, List<DbParameter> aparams)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
