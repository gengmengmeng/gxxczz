using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using MisFrameWork.core.converter;

namespace MisFrameWork.core.converter.Support
{
    public class DefaultConverter : IObjectConverter
    {
        #region IObjectConverter 成员

        public object FromString(object str)
        {
            return str;
        }

        public string ToString(object obj)
        {
            if (obj != null)
                return obj.ToString();
            else
                return null;
        }

        #endregion
    }
}
