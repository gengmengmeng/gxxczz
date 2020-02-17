using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using MisFrameWork.core.converter;

namespace MisFrameWork.core.converter.Support
{
    public class NumberConverter : IObjectConverter
    {

        protected Type ReturnNumberType = typeof(decimal);
        public NumberConverter(Type returnNumberType)
        {
            ReturnNumberType = returnNumberType;
        }

        #region IObjectConverter 成员

        public object FromString(object str)
        {
            System.Reflection.MethodInfo methord = ReturnNumberType.GetMethod("Parse", new Type[] { typeof(string) });
            return methord.Invoke(null,new object[]{str.ToString()});
        }

        public string ToString(object obj)
        {
            return obj.ToString();
        }

        #endregion
    }
}
