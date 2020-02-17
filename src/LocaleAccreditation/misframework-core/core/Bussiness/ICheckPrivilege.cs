using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MisFrameWork.core.Bussiness
{
    public interface ICheckPrivilege
    {
        /// <summary>
        /// 验证该用户是否具备某种权限
        /// </summary>
        /// <param name="privilege">权限标识</param>
        /// <returns></returns>
        bool HaveModulePrivilege(string privilege);
        bool HaveOperationPrivilege(string privilege);
     
    }
}
