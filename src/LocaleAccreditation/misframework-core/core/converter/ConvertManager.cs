using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MisFrameWork.core.converter.Support;
namespace MisFrameWork.core.converter
{
    public class ConvertManager
    {
        protected IObjectConverter defaultConvert = new DefaultConverter();
        protected Dictionary<Type, IObjectConverter> converters = new Dictionary<Type, IObjectConverter>();
        public ConvertManager()
        {
            RegistConvert(typeof(DateTime), new DateTimeConvert());
            RegistConvert(typeof(int), new NumberConverter(typeof(int)));
            RegistConvert(typeof(Int16), new NumberConverter(typeof(Int16)));
            RegistConvert(typeof(Int32), new NumberConverter(typeof(Int32)));
            RegistConvert(typeof(Int64), new NumberConverter(typeof(Int64)));
            RegistConvert(typeof(decimal), new NumberConverter(typeof(decimal)));
            RegistConvert(typeof(float), new NumberConverter(typeof(float)));
            RegistConvert(typeof(double), new NumberConverter(typeof(double)));
            RegistConvert(typeof(byte[]), new BinaryConverter());
        }

        public static bool IsDigitType(Type t)
        {
            if (
                t.Equals(typeof(Int32)) || t.Equals(typeof(Int16)) || t.Equals(typeof(byte)) || t.Equals(typeof(long))
                || t.Equals(typeof(float)) || t.Equals(typeof(double)) || t.Equals(typeof(decimal)))
                return true;
            return false;
        }

        public static bool IsBooleanType(Type t)
        {
            if (t.Equals(typeof(bool)))
                return true;
            return false;
        }

        public static ConvertManager DefaultConvertManager = new ConvertManager();

        public void RegistConvert(Type type, IObjectConverter converter)
        {
            converters[type] = converter;
        }

        #region IObjectConverter 成员

        public object FromString(Type t, object str)
        {
            if (converters.Keys.Contains(t))
                return converters[t].FromString(str);
            else
                return defaultConvert.FromString(str);
        }

        public string ToString(object obj)
        {
            if (obj == null)
                return null;
            Type t = obj.GetType();
            if (converters.Keys.Contains(t))
                return converters[t].ToString(obj);
            else
                return defaultConvert.ToString(obj);
        }

        #endregion
    }
}
