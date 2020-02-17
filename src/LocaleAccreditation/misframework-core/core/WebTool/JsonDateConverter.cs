using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
namespace MisFrameWork.core.WebTool
{
    public class JsonDateConverter : JavaScriptConverter
    {
        string formatString = null;
        public JsonDateConverter(string formatString)
        {
            this.formatString=formatString;
        }
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new Type[] { typeof(System.Collections.Hashtable),typeof(UnCaseSenseHashTable)}; }
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            System.Collections.IDictionary d = (System.Collections.IDictionary)obj;
            foreach (string k in d.Keys)
            {
                object v = d[k];
                if (v == null)
                    result[k] = "";
                else if ( v is DateTime)
                {
                    DateTime dt = (DateTime)v;
                    result[k] = "new Date("+dt.Year+","+dt.Month+","+dt.Day+","+dt.Hour+","+dt.Minute+","+dt.Second+","+dt.Millisecond+","+")";
                }
                else 
                    result[k] = d[k];
            }
            return result;
        }

        public override object  Deserialize(IDictionary<string,object> dictionary, Type type, JavaScriptSerializer serializer)
        {
 	        System.Collections.IDictionary result = (System.Collections.IDictionary)type.GetConstructor(new Type[]{}).Invoke(null);
            foreach(string k in dictionary.Keys)
            {
                result[k]=dictionary[k];
            }
            return result;
        }
    }

    public class JsonDateStringConverter : JavaScriptConverter
    {
        string formatString = null;
        public JsonDateStringConverter(string formatString)
        {
            this.formatString = formatString;
        }
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new Type[] { typeof(System.Collections.Hashtable), typeof(UnCaseSenseHashTable) }; }
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            System.Collections.IDictionary d = (System.Collections.IDictionary)obj;
            foreach (string k in d.Keys)
            {
                object v = d[k];
                if (v == null)
                    result[k] = "";
                else if (v is DateTime)
                    result[k] = ((DateTime)v).ToString(formatString);
                else
                    result[k] = d[k];
            }
            return result;
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            System.Collections.IDictionary result = (System.Collections.IDictionary)type.GetConstructor(new Type[] { }).Invoke(null);
            foreach (string k in dictionary.Keys)
            {
                result[k] = dictionary[k];
            }
            return result;
        }
    }
}
