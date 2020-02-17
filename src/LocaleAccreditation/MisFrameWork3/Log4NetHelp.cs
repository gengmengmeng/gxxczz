
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MisFrameWork3
{
    public static class Log4NetHelp
    {
        static log4net.ILog debugLog = log4net.LogManager.GetLogger("debugFile");
        static log4net.ILog errorLog = log4net.LogManager.GetLogger("errorFile");

        public static void WriteDebugLog(string log)
        {
            if (debugLog.IsDebugEnabled)
            {
                debugLog.Debug(log);
            }
        }


        public static void WriteErrorLog(string log,Exception ex)
        {
            if (errorLog.IsErrorEnabled)
            {
                errorLog.Error(log + ex.StackTrace);
            }
        }
    }
}