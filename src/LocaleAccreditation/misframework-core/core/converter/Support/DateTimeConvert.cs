using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using MisFrameWork.core.converter;

namespace MisFrameWork.core.converter.Support
{
    /// <summary>
    /// 支持8位，10位，
    /// </summary>
    public class DateTimeConvert : IObjectConverter
    {
        public string DateTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";

        public DateTimeConvert() { }
        public DateTimeConvert(string dateTimeFormat)
        {
            this.DateTimeFormat = dateTimeFormat;
        }
        #region IObjectConverter 成员

        public object FromString(object str)
        {
            if (str is DateTime)
                return str;
            else
            {
                string s = str.ToString();
                //string dateformat = "yyyyMMdd";
                //string dateSeperater = "-";
                //if (s.IndexOf("/"))
                //    dateSeperater = "/";
                //if (s.Length == 10)
                //    dateformat = "yyyy" + dateSeperater + "MM" + dateSeperater + "dd";
                //else if (s.Length == 16)
                //    dateformat = "yyyy" + dateSeperater + "MM" + dateSeperater + "dd HH:mm";
                //else if (s.Length == 19)
                //    dateformat = "yyyy" + dateSeperater + "MM" + dateSeperater + "dd HH:mm:ss";
                //else
                //    throw new Exception(s + "是无效的日期格式");
                return DateTime.ParseExact(s,DateTimeFormat, null);

            }
        }

        public string ToString(object obj)
        {
            if (obj is DateTime)
                return ((DateTime)obj).ToString(this.DateTimeFormat);
            else
                throw new Exception("对象不是有效的日期型");
        }

        #endregion
    }
}
