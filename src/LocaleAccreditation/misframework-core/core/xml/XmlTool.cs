using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
namespace MisFrameWork.core.xml
{
    public class XmlTool
    {
        private XmlTool() { }
        private static XmlTool instance = null;
        public static XmlTool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new XmlTool();
                }
                return instance;
            }
        }

        public string GetAttritudeValue(XmlNode node, string attName, string defaultValue)
        {
            if (node.Attributes == null)
                throw new Exception(node.InnerText);
            if ((node.Attributes[attName] == null) || (node.Attributes[attName].Value == ""))
                return defaultValue;
            return node.Attributes[attName].Value;
        }

        public string GetAttritudeValue(XmlNode node, string attName)
        {
            return GetAttritudeValue(node, attName, "");
        }

        public string ConvertObject(object obj)
        {
            StringBuilder result = new StringBuilder();
            Type t = obj.GetType();
            result.Append("<" + t.Name + ">");
            PropertyInfo[] pis = t.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                if (pi.CanRead)
                {
                    object pi_value = pi.GetValue(obj,null);
                    result.Append("<" + pi.Name + ">");
                    result.Append(ToXmlString(pi_value));
                    result.Append("</" + pi.Name + ">");
                }
            }
            result.Append("</" + t.Name + ">");
            return result.ToString();
        }

        public string ToXmlString(object str)
        {
            if (str == null)
                return "";
            return str.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
