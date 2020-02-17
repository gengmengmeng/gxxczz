using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.Common;
using MisFrameWork.core.xml;
using MisFrameWork.core.db.Support;



namespace MisFrameWork.core.db
{
    [Serializable]
    public class Condition
    {
        private List<Condition> subConditions = new List<Condition>();
        private string relate;
        private string src;
        private string op;

        public static Condition CreateCondition(string relation,string[] fields,string[] operators,object[] values)
        {
            if ((fields.Length!=operators.Length)||(operators.Length!=values.Length))
            {
                throw (new Exception("字段、操作和值的数据量不符："+fields.Length.ToString()+","+operators.Length.ToString()+","+values.Length.ToString()));
            }
            Condition cdt = new Condition();
            for (int i=0;i<fields.Length;i++)
                cdt.subConditions.Add(new Condition(relation,fields[i],operators[i],values[i]));
            return cdt;
        }

        private static Condition empty = new Condition();
        public static Condition Empty
        {
            get { return empty; }
        }

        public void AddSubCondition(string relate, string src, string op, Object tag)
        {
            SubConditions.Add(new Condition(relate, src, op, tag));
        }

        public void AddSubCondition(Condition subCdt)
        {
            SubConditions.Add(subCdt);
        }
        public List<Condition> SubConditions
        {
            get { return subConditions; }
            set { subConditions = value; }
        }

        public string Src 
        {
            get { return src; }
            set { src = value; }
        }
        public string SrcText { get; set; }
        public string Op 
        {
            get { return op; }
            set { op = value.ToUpper(); }
        }
        
        public string Relate 
        {
            get { return relate; }
            set { relate = value.ToUpper(); }
        }

        public Object Tag { get; set; }
        public bool Not { get; set; }
        public Condition() 
        {
            this.Not = false;
            this.Relate = "and";
            this.Op = "=";
        }

        
        public Condition(string relate, string src, string op, Object tag):this()
        {
            this.Relate = relate;
            this.Src = src;
            this.Op = op;
            this.Tag = tag;
        }


        /// <summary>
        /// <?xml version="1.0" encoding="utf-8"?>
        ///     <conditions relate="AND" not="False">
        ///        <conditions relate="AND" not="False">
        ///            <condition src="N1" op="=" tag="1" relate="OR" type="number" />
        ///            <condition src="D1" op="=" tag="2018-11-20 11:38" relate="OR" type="datetime" />
        ///            <condition src="T1" op="=" tag="文本" relate="OR" type="string" />
        ///        </conditions>
        ///        <condition src="ROOT_1" op="=" tag="ROOT_1" relate="AND" type="string" />
        ///     </conditions>   
        /// </summary>
        /// <returns></returns>
        public string ToXmlString()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                this.ToXmlNodeString(true);
        }

        protected string ToXmlNodeString(bool isRoot)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (this.subConditions.Count > 0)
            {
                sb.Append("<conditions relate=\"" + this.Relate + "\" not=\"" + this.Not.ToString() + "\">\n");
                for (int i = 0; i < this.SubConditions.Count; i++)
                {
                    Condition c  = this.SubConditions[i];
                    sb.Append(c.ToXmlNodeString(false));
                }
                sb.Append("</conditions>\n");
            }
            else
            {
                string type = "string";
                string v = "";
                if (this.Tag == null)
                {
                    type = "null";
                }
                else if (this.Tag is DateTime)
                {
                    type = "datetime";
                    v = ((DateTime)Tag).ToString("yyyy-MM-dd HH:mm");
                }
                else if (this.Tag is int || this.Tag is Int64 || this.Tag is byte || this.Tag is float || this.Tag is decimal)
                {
                    type = "number";
                    v = this.Tag.ToString();
                }
                else
                    v = XmlTool.Instance.ToXmlString(this.Tag.ToString());
                if (isRoot)
                    sb.Append("<conditions relate=\"" + this.Relate + "\" not=\"" + this.Not.ToString() + "\">\n");
                sb.Append("<condition src=\"" + this.Src + "\" op=\"" + XmlTool.Instance.ToXmlString(this.Op) + "\" tag=\"" + v + "\" relate=\"" + this.Relate + "\" type=\"" + type + "\" />\n");
                if (isRoot)
                    sb.Append("</conditions>\n");
            }
            return sb.ToString();
        }

        public string ToJsonString()
        {
            return this.ToJsonNodeString();
        }

        protected string ToJsonNodeString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (this.subConditions.Count > 0)
            {
                sb.Append("{");
                sb.Append("\"relate\":\"" + this.Relate + "\",\"not\":\"" + this.Not.ToString() + "\",");
                sb.Append("\"conditions\":[");
                for (int i = 0; i < this.SubConditions.Count; i++)
                {
                    if (i>0)
                        sb.Append(",");
                    Condition c  = this.SubConditions[i];
                    sb.Append(c.ToJsonNodeString());                   
                }
                sb.Append("]");
                sb.Append("}");
            }
            else
            {
                string type = "string";
                string v = "";
                if (this.Tag == null)
                {
                    type = "null";
                }
                else if (this.Tag is DateTime)
                {
                    type = "datetime";
                    v = ((DateTime)Tag).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (this.Tag is int || this.Tag is Int64 || this.Tag is byte || this.Tag is float || this.Tag is decimal)
                {
                    type = "number";
                    v = this.Tag.ToString();
                }
                else
                    v = XmlTool.Instance.ToXmlString(this.Tag.ToString());
                sb.Append("{\"src\":\"" + this.Src + "\",\"op\":\"" + this.Op + "\",\"tag\":\"" + v + "\",\"relate\":\"" + this.Relate + "\",\"type\":\"" + type + "\"}");
            }
            return sb.ToString();
        }

        public static Condition LoadFromNameValueCollection(System.Collections.Specialized.NameValueCollection obj)
        {
            Condition queryCondition = new Condition();
            String combine_condition_cnt = obj["combine_condition_cnt"];
            Condition outCdt = new Condition();
            if (combine_condition_cnt != null && !"".Equals(combine_condition_cnt))
            {
                int cur_condition_index = 0;
                PrepareCombineConditionFromParams(obj, queryCondition, int.Parse(combine_condition_cnt), ref cur_condition_index, 0);
            }
            else
            {
                queryCondition = PrepareConditionFromParams(obj);
            }
            return queryCondition;
        }
        /// <summary>
        /// 处理Request里的参数生成查询条件
        /// 
        /// QRY_OP_数值_字段名
        /// QRY_FV_数值_字段名
        /// 如果字段名以"__"开始和结束的，作为特殊条件，不做处理。
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        protected static Condition PrepareConditionFromParams(System.Collections.Specialized.NameValueCollection obj)
        {
            Condition result = new Condition();
            foreach (string k in obj.AllKeys)
            {
                if (k != null && k.StartsWith("QRY_FV_"))
                {
                    String fv = obj[k];
                    if (fv == null || "".Equals(fv))
                        continue;
                    int fieldNameStartIndex = k.IndexOf('_', 7);
                    String fn = k.Substring(fieldNameStartIndex + 1, k.Length - fieldNameStartIndex - 1);
                    String fn_xh = k.Substring(7, fieldNameStartIndex - 7);
                    if (fn.StartsWith("__") && fn.EndsWith("__"))//属于特殊处理的条件不做处理
                    {
                        continue;
                    }

                    String op = obj["QRY_OP_" + fn_xh + "_" + fn];
                    if (op == null || "".Equals(op))
                    {
                        op = "=";
                    }
                    if ("LIKE".Equals(op.ToUpper()))
                    {
                        if (fn == "UNIT")
                        {
                            for (int i = 11; i >= 0; i--)
                            {
                                if (fv[i].ToString().Trim() != "0")
                                {
                                    fv = fv.Substring(0, i + 1);
                                    fv = "%" + fv + "%";
                                    break;
                                }
                            }
                        }
                        else
                            fv = "%" + fv + "%";
                    }

                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\d{4}\/\d{2}\/\d{2}");
                    if (r.IsMatch(fv))
                    {
                        result.SubConditions.Add(new Condition("and", fn, op, DateTime.Parse(fv)));
                    }
                    else
                    {
                        int intFv = 0;
                        if ("ID".Equals(fn) && int.TryParse(fv, out intFv))
                            result.SubConditions.Add(new Condition("and", fn, op, intFv));
                        else
                            result.SubConditions.Add(new Condition("and", fn, op, fv));
                    }
                }
            }
            return result;
        }



        protected static void PrepareCombineConditionFromParams(System.Collections.Specialized.NameValueCollection obj, Condition parentC, int condition_cnt, ref int cur_condition_index, int depth)
        {
            while (cur_condition_index < condition_cnt)
            {
                String index = cur_condition_index.ToString();
                String qry_relate = obj["QRY_RELATE_" + index];
                int qry_depth = int.Parse(obj["QRY_DEPTH_" + index]);
                String qry_op = obj["QRY_OP_" + index];
                String qry_fn = obj["QRY_FN_" + index];
                String qry_fv = obj["QRY_FV_" + index];
                if (qry_depth == depth)
                {
                    parentC.SubConditions.Add(new Condition(qry_relate, qry_fn, qry_op, qry_fv));
                    cur_condition_index++;
                }
                else
                    if (qry_depth > depth)
                    {
                        Condition new_sub_condition = new Condition();
                        new_sub_condition.Relate = qry_relate;
                        new_sub_condition.SubConditions.Add(new Condition("and", qry_fn, qry_op, qry_fv));
                        parentC.SubConditions.Add(new_sub_condition);
                        cur_condition_index++;
                        PrepareCombineConditionFromParams(obj, new_sub_condition, condition_cnt, ref cur_condition_index, qry_depth);
                    }
                    else
                        return;
            }
        }

        public static Condition LoadFromXml(String xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            return LoadFromXml(doc);
        }
        public static Condition LoadFromXml(System.Xml.XmlDocument doc)
        {
            return LoadFromXmlNode(doc.DocumentElement);
        }


        public static Condition LoadFromXmlNode(System.Xml.XmlNode node)
        {
            Condition result = new Condition();
            result.Relate = node.Attributes["relate"].Value;
            result.Not = (node.Attributes["not"] != null && ("true".Equals(node.Attributes["not"].Value.ToLower()) || "1".Equals(node.Attributes["not"].Value)));
            foreach (XmlNode subnode in node.ChildNodes)
            {
                if (subnode.Name.Equals("conditions"))
                {
                    result.SubConditions.Add(LoadFromXmlNode(subnode));
                }
                else if (subnode.Name.Equals("condition"))
                {
                    string src = subnode.Attributes["src"].Value.Replace("\'","").Replace("\"","");
                    string op = XmlTool.Instance.ToXmlString(subnode.Attributes["op"].Value.Replace("\'","").Replace("\"",""));
                    object tag = null;
                    string type = subnode.Attributes["type"].Value;
                    string relate = subnode.Attributes["relate"].Value.Replace("\'","").Replace("\"","");
                    if ("null".Equals(type))
                        tag = null;
                    else if ("number".Equals(type))
                        tag = Decimal.Parse(subnode.Attributes["tag"].Value);
                    else if ("datetime".Equals(type))
                        tag = DateTime.Parse(subnode.Attributes["tag"].Value);
                    else
                        tag = XmlTool.Instance.ToXmlString(subnode.Attributes["tag"].Value);
                    result.SubConditions.Add(new Condition(relate,src,op,tag));
                }
            }
            return result;
        }

        public static Condition LoadFromJson(String jsonString)
        {
            UnCaseSenseHashTable root = new UnCaseSenseHashTable();
            root.LoadFromJson(jsonString);
            return LoadFromJsonNode(root);
        }

        public static Condition LoadFromJsonNode(UnCaseSenseHashTable node)
        {
            Condition result = new Condition();
            result.Relate = node["relate"].ToString();
            result.Not = (node["not"] != null && ("true".Equals(node["not"].ToString().ToLower()) || "1".Equals(node["not"])));
            if (node["conditions"]!=null)
            {
                List<UnCaseSenseHashTable> subCdtList = (List<UnCaseSenseHashTable>)node["conditions"];
                foreach (UnCaseSenseHashTable subnode in subCdtList)
                {
                    result.AddSubCondition(LoadFromJsonNode(subnode));
                }
            }
            else
            {
                result.src=(string)node["SRC"];
                result.op=(string)node["OP"];
                result.Tag=(string)node["TAG"];
            }
            
            return result;
        }

        public SqlStatementObject GetSqlStatementObject(IDataBaseUtility dbu,string tableName, List<DbParameter> ps)
        {
            StringBuilder sql = new StringBuilder();
            if (ps == null)
                ps = new List<DbParameter>();
            GetSqlStatementObject(dbu,tableName, sql, ps, false);

            return new SqlStatementObject(sql.ToString(), ps);
	    }       

        private void GetSqlStatementObject(IDataBaseUtility dbu,string tableName,StringBuilder sql,List<DbParameter> ps,bool fieldNameUseDescript){
		    bool getparams = ps!=null;
		    if (!sql.Length.Equals(0))
			    sql.Append(" "+this.Relate+" ");
		    if ((this.subConditions == null) || (this.subConditions.Count==0)){
			    String f = this.Src;
			    //如果要求使用字段描述的，就使用字段描述
                if (fieldNameUseDescript && this.SrcText != null && !"".Equals(this.SrcText))
                {
				    f = this.SrcText;
			    }
			    if (f==null||"".Equals(f)){
				    sql.Append("1=1");
                    if ((tableName != null) && !"".Equals(tableName))
                    {
                        ITableInfo ti = dbu.CreateTableInfo(tableName);
                        if ((ti != null) && ti.FieldsByName.ContainsKey("DELETED_MARK"))
                        {
                            sql.Insert(0, "(DELETED_MARK=0 AND (");
                            sql.Append("))");
                        }
                    }
                    return;
			    }
			    bool doNotUseValue = OperationDoNotUseValue(this.Op);
			    String opAndValue = " "+this.Op+" ";
			    if (!doNotUseValue){//如果是不需要值的操作。
				    if (getparams){//如果是要获取参数的还要做以下处理
                        string expression = TagExpression();
                        if (expression!=null)
                        {
                            opAndValue += expression;
					    }
					    else
                        {
                            DbParameter param = dbu.DbProviderFactory.CreateParameter();
                            param.Value = this.Tag;
                            if (this.Tag is DateTime)
                                param.DbType = System.Data.DbType.DateTime;
                            else if (this.Tag is int || this.Tag is Int64 || this.Tag is Int16 || this.Tag is decimal || this.Tag is double || this.Tag is float)
                                param.DbType = System.Data.DbType.Decimal;
                            else
                                param.DbType = System.Data.DbType.String;
                            param.ParameterName = "p" + ps.Count.ToString();
                            ps.Add(param);
                            //opAndValue += "?";
                            opAndValue += dbu.Dialect.ParameterPrefix + param.ParameterName;
						    
					    }
				    }
				    else{
					    String v = this.Tag.ToString();
					    if (this.Tag is DateTime){	
						    v = ((DateTime)Tag).ToString("yyyy/MM/dd HH:mm");
					    }    						
					    opAndValue += v.ToString();
				    }					
			    }
			    sql.Append(f+opAndValue);
		    }
		    else{
                //有个子项
                StringBuilder s = new StringBuilder();
			    for (int i=0;i<this.subConditions.Count;i++){							
				    this.subConditions[i].GetSqlStatementObject(dbu,null, s, ps, fieldNameUseDescript);				
			    }
			    sql.Append("("+s+")");
		    }
            if ((tableName != null) && !"".Equals(tableName))
            {
                ITableInfo ti = dbu.CreateTableInfo(tableName);
                if ((ti != null) && ti.FieldsByName.ContainsKey("DELETED_MARK"))
                {
                    sql.Insert(0, "(DELETED_MARK=0 AND (");
                    sql.Append("))");
                }
            }
	    }

        private bool OperationDoNotUseValue(String op)
        {
            //TODO 没有判断
            //DefaultDialect dd = DbHelper.getDbHelper().getConfiguration().getDialect();
            //boolean result = dd.isnull().equals(op) || dd.notnull().equals(op);
            //if (!result && this.fieldValue == null)
            //{
            //    this.operation = dd.isnull();
            //    result = true;
            //}
            return false;
        }

        private string TagExpression() //如果tag的值是以“EXPR:”开头的，就把它作为表达式来查询
        {
            if (this.Tag is string && this.Tag != null && this.Tag.ToString().StartsWith("EXPR:"))
                return this.Tag.ToString().Substring(5);
            return null;
        }

        public void Query(IDataBaseUtility dbu,string tableName,string field,string orderBy,int offset,int maxResults)
        {
            
            ITableInfo tableInfo = dbu.CreateTableInfo(tableName);            
        }
    }
}
