using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.Common;

using Spring.Context;
using Spring.Context.Support;

using MisFrameWork.core.xml;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.db.Listener
{
    public class DbOperateListenerManager
    {
        protected static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(AbstractDataBaseUtility));

        SortedDictionary<string, List<IDbOperateListener>> beforeInsertListener = new SortedDictionary<string, List<IDbOperateListener>>();
        SortedDictionary<string, List<IDbOperateListener>> afterInsertListener = new SortedDictionary<string, List<IDbOperateListener>>();

        SortedDictionary<string, List<IDbOperateListener>> beforeUpdateListener = new SortedDictionary<string, List<IDbOperateListener>>();
        SortedDictionary<string, List<IDbOperateListener>> afterUpdateListener = new SortedDictionary<string, List<IDbOperateListener>>();

        SortedDictionary<string, List<IDbOperateListener>> beforeDeleteListener = new SortedDictionary<string, List<IDbOperateListener>>();
        SortedDictionary<string, List<IDbOperateListener>> afterDeleteListener = new SortedDictionary<string, List<IDbOperateListener>>();

        public const int LISTENSER_TYPE_BEFORE_INSERT = 0;
        public const int LISTENSER_TYPE_BEFORE_UPDATE = 1;
        public const int LISTENSER_TYPE_BEFORE_DELETE = 2;

        public const int LISTENSER_TYPE_AFTER_INSERT = 3;
        public const int LISTENSER_TYPE_AFTER_UPDATE = 4;
        public const int LISTENSER_TYPE_AFTER_DELETE = 5;

        public bool RegistListener(string tableName, IDbOperateListener listener, int eventType)
        {
            string tn = tableName.ToUpper();
            List<IDbOperateListener> list = this.GetListenerList(tn, eventType);
            if (list == null)
                return false;
            list.Add(listener);
            return true;
        }

        public List<IDbOperateListener> GetListenerList(string tableName, int eventType)
        {
            string tn = tableName.ToUpper();
            List<IDbOperateListener> result = null;
            switch (eventType)
            {
                case LISTENSER_TYPE_BEFORE_INSERT:
                    {
                        result = GetListenerListFromEventList(tn, this.beforeInsertListener);
                        break;
                    }
                case LISTENSER_TYPE_BEFORE_UPDATE:
                    {
                        result = GetListenerListFromEventList(tn, this.beforeUpdateListener);
                        break;
                    }
                case LISTENSER_TYPE_BEFORE_DELETE:
                    {
                        result = GetListenerListFromEventList(tn, this.beforeDeleteListener);
                        break;
                    }
                case LISTENSER_TYPE_AFTER_INSERT:
                    {
                        result = GetListenerListFromEventList(tn, this.afterInsertListener);
                        break;
                    }
                case LISTENSER_TYPE_AFTER_UPDATE:
                    {
                        result = GetListenerListFromEventList(tn, this.afterUpdateListener);
                        break;
                    }
                case LISTENSER_TYPE_AFTER_DELETE:
                    {
                        result = GetListenerListFromEventList(tn, this.beforeDeleteListener);
                        break;
                    }
            }
            return result;
        }

        private List<IDbOperateListener> GetListenerListFromEventList(string tableName, SortedDictionary<string, List<IDbOperateListener>> eventList)
        {
            List<IDbOperateListener> result = null;
            if (eventList.ContainsKey(tableName))
                result = eventList[tableName];
            else
            {
                result = new List<IDbOperateListener>();
                eventList[tableName] = result;
            }
            return result;
        }

        public bool LoadListenerFromSpringIocFile(string fileName)
        {
            IApplicationContext context = new XmlApplicationContext(fileName);
            bool result = true;
            result = result && LoadListenersFromContext(context,"before_insert",LISTENSER_TYPE_BEFORE_INSERT);
            result = result && LoadListenersFromContext(context,"before_update",LISTENSER_TYPE_BEFORE_UPDATE);
            result = result && LoadListenersFromContext(context,"before_delete",LISTENSER_TYPE_BEFORE_DELETE);

            result = result && LoadListenersFromContext(context,"after_insert",LISTENSER_TYPE_AFTER_INSERT);
            result = result && LoadListenersFromContext(context,"after_update",LISTENSER_TYPE_AFTER_UPDATE);
            result = result && LoadListenersFromContext(context,"after_delete",LISTENSER_TYPE_AFTER_DELETE);
            return result;
        }

        public void LoadDictionaryTableMaper(string fileName)
        {
            IApplicationContext context = new XmlApplicationContext(fileName);
            if (context.ContainsObject("db_table_dic_map_config"))
            {
                UnCaseSenseHashTable map = (UnCaseSenseHashTable)context.GetObject("db_table_dic_map_config");
                BeforeInsertOrUpdateConvertDic listener_insert = new BeforeInsertOrUpdateConvertDic(map);
                BeforeInsertOrUpdateConvertDic listener_update = new BeforeInsertOrUpdateConvertDic(map);
                this.RegistListener("*",listener_insert,LISTENSER_TYPE_BEFORE_INSERT);
                this.RegistListener("*",listener_insert,LISTENSER_TYPE_BEFORE_UPDATE);                
            }
        }

        protected bool LoadListenersFromContext(IApplicationContext context, string rootName, int eventType)
        {
            try
            {
                System.Collections.Hashtable map = null;
                if (context.ContainsObject(rootName))
                {
                    map = (System.Collections.Hashtable)context.GetObject(rootName);
                    foreach (string k in map.Keys)
                    {
                        System.Collections.IList listeners = (System.Collections.IList)map[k];
                        foreach (Support.IDbOperateListener listener in listeners)
                        {
                            this.RegistListener(k, listener, eventType);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("加载 表 监听器时出错！", e);
                return false;
            }
            return true;
        }
    }
}
