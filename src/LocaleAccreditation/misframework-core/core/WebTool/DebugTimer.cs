using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MisFrameWork.core.WebTool
{
    public class DebugTimer
    {
        List<DateTime> dateTimes = new List<DateTime>();
        List<string> messages = new List<string>();

        public void Mark(string msg)
        {
            dateTimes.Add(DateTime.Now);
            messages.Add(msg);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < messages.Count; i++)
            {
                sb.Append(messages[i]+"：");
                sb.Append(dateTimes[i].ToString("yyyy/MM/dd HH:mm:ss"));
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
