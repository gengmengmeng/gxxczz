using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db
{
    public class DbUtilityManager
    {
        public static string DefaultDbUtilityName = "default";
        Dictionary<string, IDataBaseUtility> dbUtilitys = new Dictionary<string, IDataBaseUtility>();

        private static DbUtilityManager instance = new DbUtilityManager();
        public static DbUtilityManager Instance
        {
            get { return instance; }
            set { instance = value; }
        }
        public void RegistDbUitility(string name, IDataBaseUtility dbutility)
        {
            dbUtilitys[name] = dbutility;
        }

        public IDataBaseUtility GetDbUtility(string name)
        {
            return dbUtilitys[name];
        }

        /// <summary>
        /// 本系统内部使用的
        /// </summary>
        public IDataBaseUtility DefaultDbUtility
        {
            get
            {
                return dbUtilitys[DefaultDbUtilityName];
            }
        }
    }

    public interface IRecordCallBack
    {
        void Execute(IDataBaseUtility dbu, UnCaseSenseHashTable record);
    }
}
