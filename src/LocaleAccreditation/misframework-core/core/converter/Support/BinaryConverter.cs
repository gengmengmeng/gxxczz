using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MisFrameWork.core.converter;

namespace MisFrameWork.core.converter.Support
{
    public class BinaryConverter : IObjectConverter
    {
        #region IObjectConverter 成员

        public object FromString(object str)
        {
            if (str == null || "".Equals(str))
                return null;
            else
            {
                return System.Convert.FromBase64String(str.ToString());
            }
        }

        public string ToString(object obj)
        {
            if (obj is byte[])
                return System.Convert.ToBase64String((byte[])obj);
            else
                return "";
        }

        #endregion
    }
}
