using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace MisFrameWork.core.db.Support
{
    public abstract class AbstractTableInfo : ITableInfo
    {
        protected static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(AbstractTableInfo));
        protected string tableName = null;
        private IDataBaseUtility dataBaseUtility = null;

        public IDataBaseUtility DataBaseUtility
        {
            get { return dataBaseUtility; }
            set { dataBaseUtility = value; }
        }
        private List<IFieldInfo> fields = new List<IFieldInfo>();

        public List<IFieldInfo> Fields
        {
            get { return fields; }
            set { fields = value; }
        }
        private SortedDictionary<String, IFieldInfo> fieldsByName = new SortedDictionary<string, IFieldInfo>();

        public SortedDictionary<String, IFieldInfo> FieldsByName
        {
            get { return fieldsByName; }
            set { fieldsByName = value; }
        }
        private SortedDictionary<String, IFieldInfo> primaryFields = new SortedDictionary<string, IFieldInfo>();

        public SortedDictionary<String, IFieldInfo> PrimaryFields
        {
            get { return primaryFields; }
            set { primaryFields = value; }
        }

        #region ITableInfo 成员

        public abstract void LoadInformation(IDataBaseUtility dbu, string tableName);

        public string TableName
        {
            get { return tableName; }
        }

        public bool HaveVerField
        {
            get { return this.FieldsByName.ContainsKey("VER"); }
        }

        public string GetDicMC_Field(string fieldName)
        {
            if (fieldName.EndsWith("__MC"))
                return null;
            foreach (string s in this.FieldsByName.Keys)
            {
                if ((s.Length>fieldName.Length+4)&&(s.StartsWith(fieldName) && s.EndsWith("__MC")))
                    return s;
            }
            return null;
        }

        public SqlStatementObject GetInsertSqlStatement(System.Collections.IDictionary record)
        {
            List<DbParameter> ps = new List<DbParameter>();
            StringBuilder sql = new StringBuilder();
            StringBuilder f_list = new StringBuilder();
            StringBuilder v_list = new StringBuilder();
            String comma = null;
            if (this.HaveVerField && !record.Contains("VER"))
            {
                record["VER"] = 1;
            }
            sql.Append("insert into " + this.tableName);

            comma = "";
            foreach (string keyOfRecord in record.Keys)
            {
                if (this.FieldsByName.ContainsKey(keyOfRecord))
                {
                    IFieldInfo fieldInfo = this.FieldsByName[keyOfRecord];
                    object v = record[keyOfRecord];
                    f_list.Append(comma + fieldInfo.CloumnName);
                    if (v != null && v is string && v.ToString().StartsWith("EXPR:"))
                    {
                        v_list.Append(comma + v.ToString().Substring(5));
                    }
                    else if (v == null)
                    {
                        v_list.Append(comma + " null ");
                    }
                    else
                    {
                        if ("".Equals(v) && fieldInfo.DbType != DbType.AnsiString && fieldInfo.DbType != DbType.String)
                            v_list.Append(comma + " null ");
                        else
                        {
                            DbParameter p = fieldInfo.CreateParameter();
                            p.ParameterName = "p" + ps.Count.ToString();
                            p.DbType = fieldInfo.DbType;
                            if (record is UnCaseSenseHashTable)
                            {
                                UnCaseSenseHashTable uht = (UnCaseSenseHashTable)record;
                                p.Value = uht.GetDefaultValueByDbType(p.DbType, keyOfRecord);
                            }
                            else
                                p.Value = v;
                            ps.Add(p);
                            v_list.Append(comma + this.DataBaseUtility.Dialect.ParameterPrefix + p.ParameterName);
                        }
                    }
                    comma = String.Intern(",");
                }
            }
            sql.Append(" (" + f_list.ToString() + ") values(" + v_list.ToString() + ")");
            return new SqlStatementObject(sql.ToString(), ps);
        }

        public SqlStatementObject GetUpdateSqlStatement(System.Collections.IDictionary record,Condition where,bool checkVer)
        {
            List<DbParameter> ps = new List<DbParameter>();
            StringBuilder sql = new StringBuilder();
            String comma = null;
            comma = "";
            foreach (string keyOfRecord in record.Keys)
            {
                if ("VER".Equals(keyOfRecord))//版本信息在最后保存
                    continue;
                if (this.PrimaryFields.ContainsKey(keyOfRecord))//主键值不更新
                    continue;
                if (this.FieldsByName.ContainsKey(keyOfRecord))
                {
                    IFieldInfo fieldInfo = this.FieldsByName[keyOfRecord];
                    object v = record[keyOfRecord];

                    if (v != null && v is string && v.ToString().StartsWith("EXPR:"))
                    {
                        sql.Append(comma + fieldInfo.CloumnName + " = " + v.ToString().Substring(5));
                    }
                    else if (v==null)
                    {
                        sql.Append(comma + fieldInfo.CloumnName + " = null ");
                    }
                    else
                    {
                        
                        if ("".Equals(v) && fieldInfo.DbType!=DbType.AnsiString && fieldInfo.DbType!=DbType.String)
                        {
                            sql.Append(comma + fieldInfo.CloumnName + " = null ");
                        }
                        else
                        {
                            DbParameter p = fieldInfo.CreateParameter();
                            p.ParameterName = "p" + ps.Count.ToString();
                            p.DbType = fieldInfo.DbType;
                            if (record is UnCaseSenseHashTable)
                            {
                                UnCaseSenseHashTable uht = (UnCaseSenseHashTable)record;
                                p.Value = uht.GetDefaultValueByDbType(p.DbType, keyOfRecord);
                            }
                            else
                                p.Value = v;
                            ps.Add(p);
                            sql.Append(comma + fieldInfo.CloumnName + " = " + this.DataBaseUtility.Dialect.ParameterPrefix + p.ParameterName);
                        }
                    }
                    comma = String.Intern(",");
                }
            }            
            if (this.HaveVerField)
            {
                sql.Append(",VER=VER+1");
                if (checkVer && record.Contains("VER") && record["VER"]!=null)
                {
                    DbParameter p = dataBaseUtility.DbProviderFactory.CreateParameter();
                }
            }
            SqlStatementObject conditionSSO = where.GetSqlStatementObject(this.dataBaseUtility,this.TableName,ps);
           
            string finalSql = "update " + this.TableName + " set " + sql.ToString() + " where " + conditionSSO.Sql;
            if (this.HaveVerField)
            {
                if (checkVer && record.Contains("VER") && record["VER"]!=null)
                {
                    finalSql = "update " + this.TableName + " set " + sql.ToString() + " where (VER <= "+int.Parse(record["VER"].ToString())+" AND (" + conditionSSO.Sql+"))";
                }
            }
            return new SqlStatementObject(finalSql, ps);
        }

        public SqlStatementObject GetDeleteSqlStatement(Condition where)
        {
            SqlStatementObject conditionSSO = where.GetSqlStatementObject(this.dataBaseUtility,this.TableName,null);
            String finalSql = "delete from " + this.TableName + " where " + conditionSSO.Sql;
            return new SqlStatementObject(finalSql, conditionSSO.Parameters);
        }

        public Condition GetPrimaryCondition(IDictionary record)
        {
            Condition result = new Condition();
            if (this.PrimaryFields.Keys.Count <= 0)
            {
                Exception e = new Exception(this.TableName + "表未设置主键！");
                logger.Error(e);
                throw e;
            }
            foreach (string k in this.PrimaryFields.Keys)
            {
                if (record.Contains(k))
                {
                    result.SubConditions.Add(new Condition("and",k,"=",record[k]));
                }
                else
                {
                    Exception e = new Exception("更新"+this.TableName + "时，记录的主键信息不足！");
                    logger.Error(e);
                    throw e;
                }
            }
            return result;
        }
        public string GetXmlString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<table name='" + this.tableName + "'>");
            result.Append("<primaryKey>");
            foreach (string k in this.primaryFields.Keys)
            {
                result.Append(MisFrameWork.core.xml.XmlTool.Instance.ConvertObject(this.primaryFields[k]));
            }
            result.Append("</primaryKey>");
            result.Append("<cloumns>");
            foreach (IFieldInfo fi in this.fields)
            {
                result.Append(MisFrameWork.core.xml.XmlTool.Instance.ConvertObject(fi));
            }
            result.Append("</cloumns>");
            result.Append("</table>");
            return result.ToString();
        }

        #endregion
    }
}
