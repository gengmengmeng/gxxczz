using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MisFrameWork.core
{
    public class StaticFunction
    {
        public static string ParseVarString(string strInput, System.Collections.IDictionary ps)
        {
            string v = strInput;
            if (ps != null)
            {
                MatchCollection mc = Regex.Matches(strInput, "\\$([^\\$]*)\\$");
                foreach (Match m in mc)
                {
                    string strOld = m.Groups[0].Value;
                    string strParam = m.Groups[1].Value;
                    string strParamName, newV;
                    int z = strParam.IndexOf(":");
                    if (z > 0)
                    {
                        strParamName = strParam.Substring(0, z);
                        newV = strParam.Substring(z + 1);
                    }
                    else
                    {
                        strParamName = strParam;
                        newV = "";
                    }

                    if (!"".Equals(strParamName) && ps.Contains(strParamName))
                    {
                        if (ps[strParamName] == null)
                            newV = "";
                        else
                            newV = ps[strParamName].ToString();
                    }

                    v = v.Replace(strOld, newV);
                }
            }
            return v;
        }
    }
}
