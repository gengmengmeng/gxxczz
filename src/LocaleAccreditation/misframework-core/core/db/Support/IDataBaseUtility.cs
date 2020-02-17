using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using MisFrameWork.core;
using MisFrameWork.core.db;
using MisFrameWork.core.db.Listener;

namespace MisFrameWork.core.db.Support
{
    public interface IDataBaseUtility
    {
        void initCache();
        ITableInfo CreateTableInfo(string tableName);
        IFieldInfo CreateFieldInfo();
        Session CreateAndOpenSession();
        DbProviderFactory DbProviderFactory { get; }
        DbType GetDbType(string type);
        IDialect Dialect { get; }
        DbOperateListenerManager DbOperateListenerManager { get; set; }
        Session LockDbTable(string name);

        void UnlockDbTable(Session s);
        /// <summary>
        /// 用于过滤Oracle中的当前用户
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        string DataBaseRestricion(System.Data.Common.DbConnection conn);
        System.Data.DataTable GetAllTableInfo(System.Data.Common.DbConnection conn);

        UnCaseSenseHashTable GetDatabaseSchemaInfo();
        DataTable GetSchema(string collectionName, string[] restrictValue);
        System.Data.DataTable GetSchema(System.Data.Common.DbConnection conn, string collectionName, string[] restrictValue);
        System.Data.Common.DbConnection CreateConnection();
        string Dm2Mc(string tableName, string dm);
        string Dm2Mc(Session session,string tableName, string dm);
        List<UnCaseSenseHashTable> Dm2Record(string tableName, string dm);
        Boolean IsOutputSql{get;set;}
        string Mc2Dm(Session s, string tableName, string mc);
        /// <summary>
        ///  把sql语句里的参数转换为静态的SQL语句（不带参数）。
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="aparams">参数列表</param>
        /// <returns>生成的静太语句</returns>
        string ConvertParamsToStaticString(DbCommand cmd,string sql,List<DbParameter> aparams);
        /// <summary>
        /// 使用静态的SQL查询语句
        /// </summary>
        bool UseStaticSqlStatement { get; set; }
        /// <summary>
        /// 本方法若数据库中不存在该记录，会抛异常
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="single_key">主键(string,int,datetime等类型)</param>
        /// <returns></returns>
        UnCaseSenseHashTable GetOneRecord(string tableName, Object single_key);
        UnCaseSenseHashTable GetOneRecord(string tableName, Object single_key,string fields);
        UnCaseSenseHashTable GetOneRecord(string tableName, System.Collections.IDictionary key, string fields);
        UnCaseSenseHashTable GetOneRecord(string tableName, System.Collections.IDictionary key);
        UnCaseSenseHashTable GetOneRecord(Session session, string tableName, System.Collections.IDictionary key);
        UnCaseSenseHashTable GetOneRecord(Session session, string tableName, System.Collections.IDictionary key,string fields);
        int RecordCount(Session session, string tableName, Condition condition);
        int RecordCount(string tableName, Condition condition);
        int Execute(string query_sql);
        int Execute(string query_sql, List<DbParameter> parameters);
        int Execute(Session session, string query_sql, List<DbParameter> parameters);
        List<UnCaseSenseHashTable> Query(string tableName, Condition condition, string fields, string orderby);
        List<UnCaseSenseHashTable> Query(Session session, string tableName, Condition condition, string fields, string orderby);
        List<UnCaseSenseHashTable> Query(string tableName, Condition condition, string fields, string orderby, string groupby, int offset, int maxResultCount);
        List<UnCaseSenseHashTable> Query(Session session, string tableName, Condition condition, string fields, string orderby, string groupby, int offset, int maxResultCount);
        List<UnCaseSenseHashTable> Query(Session session, string query_sql, int offset, int maxResultCount);
        List<UnCaseSenseHashTable> Query(Session session, string query_sql, List<DbParameter> parameters, int offset, int maxResultCount);
        List<UnCaseSenseHashTable> Query(string query_sql, int offset, int maxResultCount);
        List<UnCaseSenseHashTable> QueryOne2Many(Session session, List<UnCaseSenseHashTable> records, string primaryTabFK, string manyTab, string manyTabPK, string fields, bool isMerge, string newPropertyPrefix);
        List<UnCaseSenseHashTable> QueryOne2Many(List<UnCaseSenseHashTable> records, string primaryTabFK, string manyTab, string manyTabPK, string fields, bool isMerge, string newPropertyPrefix);
        int InsertRecord(String tableName, System.Collections.IDictionary record);
        int InsertRecord(Session session, String tableName, System.Collections.IDictionary record);
        int UpdateRecord(string tableName, System.Collections.IDictionary record, bool checkVer);
        int UpdateRecord(Session session, string tableName, System.Collections.IDictionary record, bool checkVer);
        int UpdateRecord(Session session, string tableName, System.Collections.IDictionary record, Condition where, bool checkVer);
        int DeleteRecord(string tableName, System.Collections.IDictionary record);
        int DeleteRecord(Session session, string tableName, System.Collections.IDictionary record);
        int DeleteRecord(Session session, string tableName, System.Collections.IDictionary record, Condition where);

        int SoftDeleteRecord(string tableName, System.Collections.IDictionary record);
        int SoftDeleteRecord(Session session, string tableName, System.Collections.IDictionary record);
        int SoftDeleteRecord(string tableName, Condition delCdt);
        int SoftDeleteRecord(Session session, string tableName, Condition delCdt);
    }
}
