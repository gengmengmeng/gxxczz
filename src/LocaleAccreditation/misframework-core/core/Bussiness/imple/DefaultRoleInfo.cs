using System;
using System.Collections;
using System.Linq;
using System.Text;

using MisFrameWork.core.db;
using MisFrameWork.core.db.Support;

namespace MisFrameWork.core.Bussiness.imple
{
    [Serializable]
    public class DefaultRoleInfo : IRoleInfo
    {
        public static String ROLES_TABLE_NAME = "S_ROLES";
        public static String ROLE_ID_FIELD_NAME = "ROLE_ID";
        public static String ROLE_NAME_FIELD_NAME = "ROLE_NAME";
        public static String ROLE_MODULE_PRIVILEGE_FIELD_NAME = "MODULE_PRIVILEGES";
        public static String ROLE_OPERATION_PRIVILEGE_FIELD_NAME = "OPERATION_PRIVILEGES";

        public static DefaultRoleInfo LoadFromDb(string roleid)
        {
            UnCaseSenseHashTable csht = DbUtilityManager.Instance.DefaultDbUtility.GetOneRecord(ROLES_TABLE_NAME, roleid);
            if (csht != null)
            {
                DefaultRoleInfo result = new DefaultRoleInfo(csht);
                return result;
            }
            else
                return null;
        }

        protected IDictionary roleRecord = new Hashtable();

        public IDictionary RoleRecord
        {
            get { return roleRecord; }
        }
        protected string[] modulePrivileges = new string[]{};
        public string[] ModulePrivileges
        {
            get { return modulePrivileges; }
        }

        protected string[] operationPrivileges = new string[] { };
        public string[] OperationPrivileges
        {
            get { return operationPrivileges; }
        }

        public DefaultRoleInfo(IDictionary roleMap)
            : base()
        {
            this.roleRecord = roleMap;
            if (roleRecord.Contains(ROLE_MODULE_PRIVILEGE_FIELD_NAME) && roleRecord[ROLE_MODULE_PRIVILEGE_FIELD_NAME] != null)
                modulePrivileges = roleRecord[ROLE_MODULE_PRIVILEGE_FIELD_NAME].ToString().Split(',');
            if (roleRecord.Contains(ROLE_OPERATION_PRIVILEGE_FIELD_NAME) && roleRecord[ROLE_OPERATION_PRIVILEGE_FIELD_NAME] != null)
                operationPrivileges = roleRecord[ROLE_OPERATION_PRIVILEGE_FIELD_NAME].ToString().Split(',');
        }
        
        #region IRoleInfo 成员

        public string RoleId
        {
            get { return roleRecord[ROLE_ID_FIELD_NAME].ToString(); }
        }

        public string RoleName
        {
            get { return roleRecord[ROLE_NAME_FIELD_NAME].ToString(); }
        }

        

        public bool HaveModulePrivilege(string privilege)
        {
            if ("000001".Equals(this.RoleId) || "ADMIN".Equals(this.RoleId))
                return true;
            String[] ps = this.ModulePrivileges;
            for (int i = 0; i < ps.Length; i++)
            {
                if (privilege.Equals(ps[i]))
                    return true;
            }
            return false;
        }

        public bool HaveOperationPrivilege(string privilege)
        {
            if ("000001".Equals(this.RoleId) || "ADMIN".Equals(this.RoleId))
                return true;
            String[] ps = this.OperationPrivileges;
            for (int i = 0; i < ps.Length; i++)
            {
                if (privilege.Equals(ps[i]))
                    return true;
            }
            return false;
        }
        #endregion
    }
}
