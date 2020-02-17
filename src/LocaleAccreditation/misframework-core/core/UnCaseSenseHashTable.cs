using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;

using MisFrameWork.core.converter;
using MisFrameWork.core.db.Support;
using System.Runtime.Serialization;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MisFrameWork.core
{
    [Serializable]
    public class UnCaseSenseHashTable : System.Collections.Hashtable, ISerializable
    {
        protected log4net.ILog logger = log4net.LogManager.GetLogger(typeof(UnCaseSenseHashTable));
        private System.Collections.ArrayList listKey = new System.Collections.ArrayList();
        public override void Remove(object key)
        {
            base.Remove(key.ToString().ToUpper());
            listKey.Remove(key.ToString().ToUpper());
        }

        public override void Add(object key, object value)
        {
            base.Add(key.ToString().ToUpper(), value);
            listKey.Add(key.ToString().ToUpper());
        }

        public override void Clear()
        {
            base.Clear();
            listKey.Clear();
        }

        public override ICollection Keys
        {
            get
            {
                return listKey;
            }
        }

        public override bool ContainsKey(object key)
        {
            if (base.ContainsKey(key))
                return true;
            return base.ContainsKey(key.ToString().ToUpper());
        }

        public override object this[object key]
        {
            get
            {
                if (base.ContainsKey(key))
                    return base[key];
                else
                    return base[key.ToString().ToUpper()];
            }
            set
            {
                if (!this.ContainsKey(key))
                    listKey.Add(key.ToString().ToUpper());
                base[key.ToString().ToUpper()] = value;
            }
        }

        public void SetCaseSenseValue(object key, object value)
        {
            base[key] = value;
            listKey.Add(key.ToString());
        }

        public bool HasValue(object key)
        {
            return this.ContainsKey(key) && this[key] != null && !"".Equals(this[key]);
        }

        public bool HasKeyValue(object key)
        {
            return this.ContainsKey(key);
        }

        public DateTime GetDateValue(object key)
        {
            object v = this[key];
            if (v == null)
                return new DateTime(1753, 1, 1);
            if (v is DateTime)
                return (DateTime)v;
            else
            {
                try
                {
                    if (v.ToString().Length == 23)
                    {
                        if (v.ToString().IndexOf('-') > 0)
                            return DateTime.ParseExact(v.ToString(), "yyyy-MM-dd HH:mm:ss:fff", null);
                        else
                            return DateTime.ParseExact(v.ToString(), "yyyy/MM/dd HH:mm:ss:fff", null);
                    }
                    else if (v.ToString().Length == 19)
                    {
                        if (v.ToString().IndexOf('-') > 0)
                            return DateTime.ParseExact(v.ToString(), "yyyy-MM-dd HH:mm:ss", null);
                        else
                            return DateTime.ParseExact(v.ToString(), "yyyy/MM/dd HH:mm:ss", null);
                    }
                    else if (v.ToString().Length == 14)
                    {
                        return DateTime.ParseExact(v.ToString(), "yyyyMMddHHmmss", null);
                    }
                    else if (v.ToString().Length == 12)
                    {
                        return DateTime.ParseExact(v.ToString(), "yyyyMMddHHmm", null);
                    }
                    else //8
                    {
                        return DateTime.ParseExact(v.ToString(), "yyyyMMdd", null);
                    }
                }
                catch (Exception ee)
                {
                    try
                    {
                        return DateTime.Parse(v.ToString());
                    }
                    catch (Exception e)
                    {
                        logger.Error("转换字段:" + v.ToString() + "为日期时出错。");
                        throw e;
                    }
                }
            }
        }

        public object GetDefaultValueByDbType(DbType type, string keyOfRecord)
        {
            switch (type)
            {
                case DbType.Decimal:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                case DbType.SByte:
                    return GetDecimalValue(keyOfRecord);
                case DbType.Double:
                    return GetFloatValue(keyOfRecord);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    return GetDateValue(keyOfRecord);
                case DbType.String:
                    if ((this[keyOfRecord] != null) && (this[keyOfRecord] is DateTime))
                        return this.GetDateString(keyOfRecord, "yyyy-MM-dd HH:mm:ss:fff");
                    else
                        return this[keyOfRecord];
                default:
                    return this[keyOfRecord];
            }
        }

        public string GetDateString(object key)
        {
            return GetDateString(key, "yyyy\\/MM\\/dd HH:mm");
        }

        public string GetDateString(object key, string format)
        {
            if (this.HasValue(key))
            {
                return this.GetDateValue(key).ToString(format);
            }
            else
            {
                return "";
            }
        }

        public long GetLongValue(object key)
        {
            object v = this[key];
            if (v is long)
                return (long)v;
            else
            {
                try
                {
                    if (v == null)
                        return 0;
                    if ("on".Equals(v))
                        return 1;
                    if ("off".Equals(v))
                        return 0;
                    return long.Parse(v.ToString());
                }
                catch (Exception e)
                {
                    logger.Error("转换字段:" + v.ToString() + "为数值时出错。");
                    throw e;
                }
            }
        }

        public int GetIntValue(object key)
        {
            return GetIntValue(key, 0);
        }
        public int GetIntValue(object key,int defaultValue)
        {
            object v = this[key];
            if (v is int)
                return (int)v;
            else if (v is decimal)
                return int.Parse(v.ToString());
            else
            {
                try
                {
                    if (v == null)
                        return defaultValue;
                    if ("on".Equals(v))
                        return 1;
                    if ("off".Equals(v))
                        return 0;
                    return int.Parse(v.ToString());
                }
                catch (Exception e)
                {
                    logger.Error("转换字段 GetIntValue:" + v.ToString() + "为数值时出错。");
                    throw e;
                }
            }
        }



        public decimal GetDecimalValue(object key)
        {
            return GetDecimalValue(key, 0);
        }
        public decimal GetDecimalValue(object key,decimal defaultValue)
        {
            object v = this[key];
            if (v is decimal)
                return (decimal)v;
            else if (v is int)
                return decimal.Parse(v.ToString());
            else
            {
                try
                {
                    if (v == null)
                        return defaultValue;
                    if ("on".Equals(v))
                        return 1;
                    if ("off".Equals(v))
                        return 0;
                    return decimal.Parse(v.ToString());
                }
                catch (Exception e)
                {
                    logger.Error("转换字段 GetDecimalValue:" + v.ToString() + "为数值时出错。");
                    throw e;
                }
            }
        }
        public float GetFloatValue(object key)
        {
            return GetFloatValue(key, 0);
        }
        public float GetFloatValue(object key,float defaultValue)
        {
            object v = this[key];
            if (v is float)
                return (float)v;
            else
            {
                try
                {
                    if (v == null)
                        return defaultValue;
                    if ("on".Equals(v))
                        return 1;
                    if ("off".Equals(v))
                        return 0;
                    return float.Parse(v.ToString());
                }
                catch (Exception e)
                {
                    logger.Error("转换字段 GetFloatValue:" + v.ToString() + "为数值时出错。");
                    throw e;
                }
            }
        }

        /// <summary>
        /// 返回可用于javascript双引号中的字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetJsString(object key)
        {
            if (this.HasValue(key))
            {
                string result = this[key].ToString().Replace("'", "\\'").Replace("\"", "\\\"").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r\n", "\\n").Replace("\n", "\\n");
                return result;
            }
            else
                return "";
        }
        /// <summary>
        /// 返回满意或不满意方面的radiobutton状态
        /// </summary>
        /// <param name="xmlstring"></param>
        /// <returns></returns>
        public string GetRadioStatus(object key, object flag)
        {
            string retrunvalue = "false";

            if (this.HasValue(key))
            {
                string result = this[key].ToString().Replace("'", "\\'").Replace("\"", "\\\"").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r\n", "\\n").Replace("\n", "\\n");
                if (result.Length > 0)
                {
                    if (result == flag.ToString())
                        return "true";
                    else
                    {
                        string[] sgroup = result.Split(new char[] { ',' });
                        foreach (string i in sgroup)
                        {
                            if (i == flag.ToString())
                                retrunvalue = "true";
                        }
                        return retrunvalue;
                    }
                }
                else
                    return "false";
            }
            else
                return "false";
        }

        public bool LoadFromXml(string xmlstring)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlstring);
            return LoadFromXml(doc);
        }
        public bool LoadFromXml(XmlDocument doc)
        {
            try
            {
                if (doc != null)
                {
                    return LoadFromXml(doc.DocumentElement);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool LoadFromXmlFile(string path)
        {
            if (!System.IO.File.Exists(path))
                return false;
            string xmlData = System.IO.File.ReadAllText(path);
            return LoadFromXml(xmlData);
        }

        public void SaveToXmlFile(string path)
        {
            string xmlData = this.ToXmlString();
            System.IO.File.WriteAllText(path, xmlData);
        }

        public void SaveToJsonFile(string path)
        {
            string xmlData = this.ToJsonString(true);
            System.IO.File.WriteAllText(path, xmlData);
        }
        public void SaveToJsonFile(string path, Encoding code)
        {
            string xmlData = this.ToJsonString(true);
            System.IO.File.WriteAllText(path, xmlData, code);
        }

        public bool LoadFromXml(XmlNode root)
        {
            try
            {
                if (root != null)
                {
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if ("list".Equals(node.Name.ToLower()))
                        {
                            string n = node.Attributes["key"].Value;
                            List<UnCaseSenseHashTable> list = new List<UnCaseSenseHashTable>();
                            foreach (XmlNode listNode in node.ChildNodes)
                            {
                                UnCaseSenseHashTable subNode = new UnCaseSenseHashTable();
                                subNode.LoadFromXml(listNode);
                                list.Add(subNode);
                            }
                            this[n] = list;
                        }
                        else if ("subnode".Equals(node.Name.ToLower()))
                        {
                            string n = node.Attributes["key"].Value;
                            UnCaseSenseHashTable list = new UnCaseSenseHashTable();
                            UnCaseSenseHashTable subNode = new UnCaseSenseHashTable();
                            subNode.LoadFromXml(node.ChildNodes[0]);
                            this[n] = subNode;
                        }
                        else
                        {
                            string n = node.Attributes["key"].Value;
                            string type = "System.String";
                            if (node.Attributes["type"] != null)
                                type = node.Attributes["type"].Value;
                            Type t = Type.GetType(type);
                            string v = node.InnerText;
                            this[n] = ConvertManager.DefaultConvertManager.FromString(t, v);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void LoadFromNameValueCollection(System.Collections.Specialized.NameValueCollection obj, bool overrideOldValue)
        {
            LoadFromNameValueCollection(obj, "", overrideOldValue);
        }

        public void LoadFromNameValueCollection(System.Collections.Specialized.NameValueCollection obj, ITableInfo tableInfor, bool overrideOldValue)
        {
            String regP = "";
            bool addOr = false;
            foreach (IFieldInfo f in tableInfor.Fields)
            {
                if (addOr)
                    regP = regP + "|";
                regP += f.CloumnName;
                addOr = true;
            }
            LoadFromNameValueCollection(obj,regP, overrideOldValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="regex">正则表达式，如果匹配结果有分组，使用第一个分组，如果没有，使用匹配结果</param>
        /// <param name="overrideOldValue"></param>
        public void LoadFromNameValueCollection(System.Collections.Specialized.NameValueCollection obj, String regex, bool overrideOldValue)
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(".*");
            if (regex != null && !"".Equals(regex))
                r = new System.Text.RegularExpressions.Regex(regex);

            if (obj == null)
                return;
            foreach (string k in obj.Keys)
            {
                string realKey = k;
                if (k == null || "".Equals(k))
                    continue;
                System.Text.RegularExpressions.Match m = r.Match(k);
                if ((r != null) && (!m.Success))
                    continue;
                else
                {
                    if (m.Groups.Count > 1)
                        realKey = m.Groups[1].Value;
                }
                if (!overrideOldValue && this.Contains(realKey))
                    continue;
                this[realKey] = obj[k];
            }
        }

        public void LoadFormDictionary(System.Collections.IDictionary obj, bool overrideOldValue)
        {
            LoadFormDictionary(obj, null, overrideOldValue);
        }

        public void LoadFormDictionary(System.Collections.IDictionary obj, string regex, bool overrideOldValue)
        {
            LoadFormDictionary(obj, regex, "", overrideOldValue);
        }

        public void LoadFormDictionary(System.Collections.IDictionary obj, string regex, string prefix, bool overrideOldValue)
        {
            if (prefix == null)
                prefix = "";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(".*");
            if (regex != null && !"".Equals(regex))
                r = new System.Text.RegularExpressions.Regex(regex);

            if (obj == null)
                return;
            foreach (string k in obj.Keys)
            {
                string realKey = k;
                if (k == null)
                    continue;
                System.Text.RegularExpressions.Match m = r.Match(k);
                if ((r != null) && (!m.Success))
                    continue;
                else
                {
                    if (m.Groups.Count > 1)
                        realKey = m.Groups[1].Value;
                }
                if (!overrideOldValue && this.Contains(k))
                    continue;
                this[prefix + realKey] = obj[k];
            }
        }

        public string ToXmlString()
        {
            return ToXmlString(true, 1);
        }

        public bool LoadFromJson(string strJson)
        {
            JObject obj = (JObject)JsonConvert.DeserializeObject(strJson);
            if (obj.Type != JTokenType.Object)
                return false;
            LoadFromJobject(this, obj);
            return true;
        }

        public bool LoadFromJsonFile(string path)
        {
            if (!System.IO.File.Exists(path))
                return false;
            string jsonData = System.IO.File.ReadAllText(path);
            return LoadFromJson(jsonData);
        }

        public bool LoadFromJsonFile(string path, Encoding code)
        {
            if (!System.IO.File.Exists(path))
                return false;
            string jsonData = System.IO.File.ReadAllText(path, code);
            return LoadFromJson(jsonData);
        }


        private void LoadFromJobject(UnCaseSenseHashTable t, JObject jobj)
        {
            for (JToken jt = jobj.First; jt != null; jt = jt.Next)
            {
                if (jt.Type == JTokenType.Property)
                {
                    JProperty jp = (JProperty)jt;
                    if (jp.Value is JValue)
                        t[jp.Name] = ((JValue)jp.Value).Value;
                    else if (jp.Value.Type == JTokenType.Array)
                    {
                        JArray jarr = ((JArray)jp.Value);
                        List<UnCaseSenseHashTable> arr = new List<UnCaseSenseHashTable>();
                        t[jp.Name] = arr;
                        for (JToken subJT = jarr.First; subJT != null; subJT = subJT.Next)
                        {
                            UnCaseSenseHashTable r = new UnCaseSenseHashTable();
                            arr.Add(r);
                            LoadFromJobject(r, (JObject)subJT);
                        }
                    }
                    else if (jp.Value.Type == JTokenType.Object)
                    {
                        UnCaseSenseHashTable r = new UnCaseSenseHashTable();
                        t[jp.Name] = r;
                        LoadFromJobject(r, (JObject)jp.Value);
                    }
                }
            }
        }
        
        public string ToJsonString()
        {
            return ToJsonString(false);
        }
        public string ToJsonString(bool isFormat)
        {
            //return JsonConvert.SerializeObject(this);
            return ToJsonString(isFormat, "", true);
        }
        public string ToJsonStringLowerKey()
        {
            return ToJsonString(false, "", true, true);
        }

        private string ToJsonString(bool isFormat,string preChar, Boolean notHead, bool IsLowerKey = false)
        {
            string NEW_LINE = "\r\n";
            string TABLE = "\t";
            if (!isFormat)
            {
                NEW_LINE = "";
                TABLE = "";
            }
            List<UnCaseSenseHashTable> listType = new List<UnCaseSenseHashTable>();
            UnCaseSenseHashTable hashType = new UnCaseSenseHashTable();
            StringBuilder result = new StringBuilder();
            if (notHead)
                result.Append("{" + NEW_LINE);
            else
                result.Append(preChar + "{" + NEW_LINE);
            string c = " ";
            foreach (string k in this.Keys)
            {
                object v = this[k];
                string strK = "\"" + k + "\"";
                if(IsLowerKey == true)
                {
                    strK = strK.ToLower();
                }
                Type t = typeof(string);
                if (v != null)
                {
                    t = v.GetType();
                }
                if (t.Equals(listType.GetType()))
                {
                    result.Append(preChar + TABLE + c + strK + ":[" + NEW_LINE);
                    List<UnCaseSenseHashTable> rs = (List<UnCaseSenseHashTable>)v;
                    string cc = " ";
                    foreach (UnCaseSenseHashTable r in rs)
                    {
                        result.Append(preChar + TABLE+TABLE  + cc + r.ToJsonString(isFormat,preChar + TABLE+TABLE, true));
                        cc = ",";
                    }
                    result.Append(preChar + TABLE+"]" + NEW_LINE);
                }
                else if (t.Equals(hashType.GetType()))
                {
                    result.Append(preChar + TABLE + c + strK + ":" + NEW_LINE);
                    UnCaseSenseHashTable rs = (UnCaseSenseHashTable)v;
                    result.Append(rs.ToJsonString(isFormat,preChar + TABLE,false));                    
                }
                else
                {
                    if (v == null)
                        result.Append(preChar + TABLE + c + strK + ":null" + NEW_LINE);
                    else if (ConvertManager.IsBooleanType(t) || ConvertManager.IsDigitType(t))
                        result.Append(preChar + TABLE + c + strK + ":" + ConvertManager.DefaultConvertManager.ToString(v).ToLower() + NEW_LINE);
                    else
                        result.Append(preChar + TABLE + c + strK + ":\"" + ConvertManager.DefaultConvertManager.ToString(v).Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"" + NEW_LINE);
                }
                c = ",";
            }
            result.Append(preChar + "}" + NEW_LINE);
            return result.ToString();
        }
        public string ToXmlString(bool containXmlHeader)
        {
            return ToXmlString(containXmlHeader, 1);
        }

        public string ToXmlString(bool containXmlHeader, int version)
        {
            return ToXmlString(containXmlHeader, version, "");
        }
        public string ToXmlString(bool containXmlHeader, int version, string preChar)
        {
            List<UnCaseSenseHashTable> listType = new List<UnCaseSenseHashTable>();
            UnCaseSenseHashTable hashType = new UnCaseSenseHashTable();
            StringBuilder result = new StringBuilder();
            if (containXmlHeader)
                result.Append("<?xml version=\"1.0\" encoding=\"GB2312\"?>\r\n");
            result.Append(preChar + "<node version=\"" + version.ToString() + "\">\r\n");
            if (version == 1)
            {
                foreach (string k in this.Keys)
                {
                    object v = this[k];
                    Type t = typeof(string);
                    if (v != null)
                    {
                        t = v.GetType();
                    }
                    if (t.Equals(listType.GetType()))
                    {
                        result.Append(preChar + "\t<list key=\"" + k + "\">\r\n");
                        List<UnCaseSenseHashTable> rs = (List<UnCaseSenseHashTable>)v;
                        foreach (UnCaseSenseHashTable r in rs)
                            result.Append(r.ToXmlString(false, version, preChar + "\t\t"));
                        result.Append(preChar + "\t</list>\r\n");
                    }
                    else if (t.Equals(hashType.GetType()))
                    {
                        result.Append("\t<subnode key=\"" + k + "\">\r\n");
                        UnCaseSenseHashTable rs = (UnCaseSenseHashTable)v;
                        result.Append(rs.ToXmlString(false, version, preChar + "\t\t"));
                        result.Append("\t</subnode>\r\n");
                    }
                    else
                    {
                        if (ConvertManager.IsBooleanType(t) || ConvertManager.IsDigitType(t))
                            result.Append(preChar + "\t<entity key=\"" + k + "\" type=\"" + t.FullName + "\">" + ConvertManager.DefaultConvertManager.ToString(v) + "</entity>\r\n");
                        else
                            result.Append(preChar + "\t<entity key=\"" + k + "\" type=\"" + t.FullName + "\"><![CDATA[" + ConvertManager.DefaultConvertManager.ToString(v) + "]]></entity>\r\n");
                    }
                }
            }
            else if (version == 2)
            {
                foreach (string k in this.Keys)
                {
                    object v = this[k];
                    Type t = typeof(string);
                    if (v != null)
                    {
                        t = v.GetType();
                    }
                    result.Append(preChar + "\t<" + k + " key=\"" + k + "\" type=\"" + t.FullName + "\"><![CDATA[" + ConvertManager.DefaultConvertManager.ToString(v) + "]]></" + k + ">\r\n");
                }
            }
            result.Append(preChar + "</node>\r\n");
            return result.ToString();
        }

        #region ISerializable 成员
        public UnCaseSenseHashTable()
            : base()
        {
        }
        public UnCaseSenseHashTable(SerializationInfo si, StreamingContext context)
        {
            System.Runtime.Serialization.SerializationInfoEnumerator sie = si.GetEnumerator();
            while (sie.MoveNext())
            {
                this[sie.Name] = sie.Value;
            }
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (string k in this.Keys)
            {
                info.AddValue(k, this[k]);
            }
        }

        #endregion
    }
}