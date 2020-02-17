using System;
using System.Collections;
using System.Linq;
using System.Text;

using MisFrameWork.core.db;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.Bussiness.imple
{
    [Serializable]
    public class DefaultUserInfo : IUserInfo
    {
        public static String USERS_TABLE_NAME = "S_USERS";
        public static String USER_ID_FIELD_NAME = "USER_ID";
        public static String USER_NAME_FIELD_NAME = "USER_NAME";
        public static String USER_PASSWD_FIELD_NAME = "USER_PASSWD";

        public static DefaultUserInfo LoadFromDb(string userid)
        {
            Condition c = new Condition("and",USER_ID_FIELD_NAME,"=",userid);
            System.Collections.Generic.IList<UnCaseSenseHashTable> datas = DbUtilityManager.Instance.DefaultDbUtility.Query(USERS_TABLE_NAME,c,"*","");
            if (datas.Count != 0)
            {
                DefaultUserInfo result = new DefaultUserInfo(datas[0]);
                return result;
            }
            else
                return null;
        }

        public DefaultUserInfo(IDictionary userMap)
            : base()
        {
            this.userRecord = userMap;
        }

        public IDictionary UserRecord
        {
            get {
                return this.userRecord;
            }
        }

        IDictionary userRecord = new Hashtable();
        System.Collections.Generic.List<IRoleInfo> roleInfos = null;

        #region IUserInfo 成员

        public string UserId
        {
            get { return userRecord[USER_ID_FIELD_NAME].ToString(); }
        }

        public string UserName
        {
            get { return userRecord[USER_NAME_FIELD_NAME].ToString(); }
        }

        public string PassWord
        {
            get { return userRecord[USER_PASSWD_FIELD_NAME].ToString(); }
        }

        public System.Collections.Generic.List<IRoleInfo> GetRoleInfos()
        {            
            if (roleInfos == null)
            {
                roleInfos = new System.Collections.Generic.List<IRoleInfo>();
                if (string.IsNullOrEmpty((string)userRecord["ROLES_INFOR"]))
                    return roleInfos;
                Session s = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
                try
                {
                    string[] roles = ((string)userRecord["ROLES_INFOR"]).Split(',');
                    foreach (string r in roles)
                    {
                        UnCaseSenseHashTable role = DbUtilityManager.Instance.DefaultDbUtility.GetOneRecord(DefaultRoleInfo.ROLES_TABLE_NAME, r);
                        if (role != null)
                            roleInfos.Add(new DefaultRoleInfo(role));
                    }
                }
                finally
                {
                    s.Close();
                }
            }
            return roleInfos;
        }

        public bool HaveModulePrivilege(string privilege)
        {
            if ("admin".Equals(this.UserId.ToLower()) || "administrator".Equals(this.UserId.ToLower()))
                return true;
            for (int i = 0; i < this.GetRoleInfos().Count; i++)
            {
                if (this.GetRoleInfos()[i].HaveModulePrivilege(privilege))
                    return true;
            }
            return false;
        }

        public bool HaveOperationPrivilege(string privilege)
        {
            if ("admin".Equals(this.UserId.ToLower()) || "administrator".Equals(this.UserId.ToLower()))
                return true;
            for (int i = 0; i < this.GetRoleInfos().Count; i++)
            {
                if (this.GetRoleInfos()[i].HaveOperationPrivilege(privilege))
                    return true;
            }
            return false;
        }

        
        public bool IsRole(string roleId)
        {
            if ("admin".Equals(this.UserId.ToLower()) || "administrator".Equals(this.UserId.ToLower()))
                return true;
            for (int i = 0; i < this.GetRoleInfos().Count; i++)
            {
                if (roleId.Equals(this.GetRoleInfos()[i].RoleId))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
