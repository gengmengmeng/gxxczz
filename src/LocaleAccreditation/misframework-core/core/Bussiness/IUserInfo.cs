using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace MisFrameWork.core.Bussiness
{
    public interface IUserInfo : ICheckPrivilege 
    {
        string UserId { get; }
        string UserName { get; }
        string PassWord { get; }
        IDictionary UserRecord { get; }
        System.Collections.Generic.List<IRoleInfo> GetRoleInfos();
        bool IsRole(string roleId);

    }
}
