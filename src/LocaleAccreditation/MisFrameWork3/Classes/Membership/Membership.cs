using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MisFrameWork3.Classes.Membership
{
    public class Membership
    {
        public class LoginResult
        {
            public Boolean Result = false;
            public string Message = "";
            public FWUserInfo UserInfo = null;
        }
        public static LoginResult Login(string uid, string pwd_md5)
        {
            //Log4NetHelp.WriteDebugLog("登陆日志");
            LoginResult result = new LoginResult();
            FWUserInfo user = new FWUserInfo();
            if (!user.LoadData(uid))
            {
                result.Result = false;
                result.Message = "用户信息不存在或者用户未启用";
                return result;
            }
            if (!user.Password.Equals(pwd_md5.ToUpper()))
            {
                result.Result = false;
                result.Message = "用户密码不正确";
                return result;
            }
            result.Result = true;
            result.UserInfo = user;

            //这里要注册到Session
            HttpContext.Current.Session["USER_INFO"] = user;
            return result;
        }

        public static void Logout()
        {
            HttpContext.Current.Session.Remove("USER_INFO");
        }

        public static FWUserInfo CurrentUser
        {
            get
            {
                Object obj = HttpContext.Current.Session["USER_INFO"];
                if (obj != null)
                    return (FWUserInfo)obj;
                return null;
            }
        }
    }
}