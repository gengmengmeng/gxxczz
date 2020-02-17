using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MisFrameWork.core.Bussiness
{
    public interface IRoleInfo : ICheckPrivilege
    {
        /// <summary>
        /// 角色标识
        /// </summary>
        string RoleId { get; }
        /// <summary>
        /// 角色名称
        /// </summary>
        string RoleName { get; }

        IDictionary RoleRecord { get; }
        /// <summary>
        /// 角色所具有的权限
        /// </summary>
        string[] ModulePrivileges { get; }
        string[] OperationPrivileges { get; }
    }
}
