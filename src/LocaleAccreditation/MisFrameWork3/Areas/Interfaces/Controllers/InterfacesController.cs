using MisFrameWork.core;
using MisFrameWork.core.db;
using MisFrameWork.core.db.Support;
using MisFrameWork3.Classes.Controller;
using MisFrameWork3.Classes.Membership;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MisFrameWork3.Areas.Interfaces.Controllers
{
    public class InterfacesController : FWExtServiceController
    {
        protected static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(AbstractDataBaseUtility));
        [FWExtServiceMethod(Descript = "登陆接口")]
        [FWExtServiceParameters(Name = "uid", Descript = "用户名", DefaultValue = "admin")]
        [FWExtServiceParameters(Name = "pwd", Descript = "设备密码密码", DefaultValue = "123456")]
        public FWExtServiceResult Login(UnCaseSenseHashTable data, string uid, string pwd)
        {
            FWExtServiceResult result = new FWExtServiceResult();
            Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
            try
            {              
                session.BeginTransaction();
                //string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["oraclecs"].ToString();
                //OracleConnection conn = new OracleConnection(connString);
                //string sql = "select userenv('sessionid') from dual";
                //conn.Open();
                //OracleCommand cmd = new OracleCommand(sql, conn);
                //cmd.CommandType = CommandType.Text;
                //DataSet ds = new DataSet();
                //OracleDataAdapter da = new OracleDataAdapter();
                //da.SelectCommand = cmd;
                //da.Fill(ds);
                //DataTable dt = new DataTable();
                //if (ds != null && ds.Tables.Count > 0)
                //    dt = ds.Tables[0];
                //conn.Close();
                //Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "登陆接口当前会话sessionID为：" + dt.Rows[0][0].ToString() + "已开启");
                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(pwd))
                {
                    Condition cdtIds = new Condition();
                    cdtIds.AddSubCondition("AND", "USER_ID", "=", uid);
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query(session, "FW_S_USERS", cdtIds, "*", null, null, -1, -1);
                    session.Close();
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "登陆接口当前会话sessionID为：已关闭");
                    if (records.Count == 1)
                    {
                        if (string.IsNullOrEmpty(records[0]["SB_PASSWD"].ToString())) {
                            result.RESULT = -1;
                            result.MSG = "未配置设备登录密码";
                            return result;
                        }
                        bool temp = pwd.Equals(records[0]["SB_PASSWD"].ToString());
                        if (temp)
                        {
                            result.RESULT = 1;
                            result.MSG = "登陆成功";
                            return result;
                        }
                        else
                        {
                            result.RESULT = -1;
                            result.MSG = "密码错误";
                            return result;
                        }
                    }
                    else
                    {
                        result.RESULT = -1;
                        result.MSG = "账户不存在";
                        return result;
                    }
                }
                else {
                    session.Close();
                    result.RESULT = -1;
                    result.MSG = "账户或密码不能为空";
                    return result;
                }
            }
            catch (Exception e) {
                session.Close();
                Log4NetHelp.WriteErrorLog("登陆设备接口发生异常：" + e.Message, e);
                result.RESULT = -1;
                result.MSG = e.Message;
                return result;
            }
        }


        [FWExtServiceMethod(Descript = "登陆账户查询接口")]       
        [FWExtServiceParameters(Name = "MACHINENO", Descript = "设备ID", DefaultValue = "123456")]
        public FWExtServiceResult Query(UnCaseSenseHashTable data, string MACHINENO)
        {
          
            FWExtServiceResult result = new FWExtServiceResult();
            string MACHINE_IP = "";
            try
            {
                Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
                session.BeginTransaction();
                if (!string.IsNullOrEmpty(MACHINENO)) {
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "IP地址更新已开启");
                    Condition cdtIds_IP = new Condition();
                    HttpRequest Request = System.Web.HttpContext.Current.Request;
                    if (string.IsNullOrEmpty(Request.ServerVariables.Get("Remote_Addr").ToString()))
                    {
                        session.Close();
                        result.RESULT = -1;
                        result.MSG = "获取设备IP地址为空";
                        return result;
                    }
                    else
                    {
                        MACHINE_IP = Request.ServerVariables.Get("Remote_Addr").ToString();
                    }
                    cdtIds_IP.AddSubCondition("AND", "MACHINENO", "=", MACHINENO);
                    List<UnCaseSenseHashTable> records_IP = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtIds_IP, "*", null, null, -1, -1);
                    if (records_IP.Count != 0)
                    {                        
                        UnCaseSenseHashTable data3 = new UnCaseSenseHashTable();
                        data3["ID"] = records_IP[0]["ID"];
                        data3["MACHINE_IP"] = MACHINE_IP;
                        int r = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord(session, "B_MACHINE", data3, false);
                        session.Commit();
                        session.Close();
                        Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "IP地址更新已关闭");
                    }
                }
            }
            catch (Exception ee) {
                Log4NetHelp.WriteErrorLog("IP地址更新发生异常：" + ee.Message, ee);
                result.RESULT = -1;
                result.MSG = ee.Message;
                return result;
            }
            try
            {                             
                if (!string.IsNullOrEmpty(MACHINENO))
                {                   
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备查询已开启");
                    Condition cdtIds = new Condition();
                    cdtIds.AddSubCondition("AND", "SB_ID_V_D_MACHINE__MC", "like", "%" + MACHINENO + "%");
                    cdtIds.AddSubCondition("AND", "DISABLED", "=", "0");
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query("FW_S_USERS", cdtIds, "*", null, null, -1, -1);
                    //session.Close();
                    //Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备查询当前会话sessionID为：" + dt.Rows[0][0].ToString() + "已关闭");
                    if (records.Count != 0)
                    {
                        string[] users = new string[records.Count];
                        int count = records.Count;
                        for (int i = 0; i < count; i++)
                        {
                            users[i] = records[i]["USER_ID"].ToString();
                        }
                        result.DATA = new UnCaseSenseHashTable();
                        result.DATA.Add("UserList", Newtonsoft.Json.JsonConvert.SerializeObject(users));
                        //Newtonsoft.Json.JsonConvert.SerializeObject(users);
                        result.RESULT = 1;
                        result.MSG = "查询成功";
                        Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备查询已结束");
                        return result;
                    }
                    else
                    {
                        result.RESULT = -1;
                        result.MSG = "设备未添加账户";
                        return result;
                    }
                }
                else
                {
                    //session.Close();
                    result.RESULT = -1;
                    result.MSG = "设备编号不能为空";
                    return result;
                }
            }
            catch (Exception e) {
                //session.Close();
                Log4NetHelp.WriteErrorLog("登陆账户查询接口发生异常：" + e.Message, e);
                result.RESULT = -1;
                result.MSG = e.Message;
                return result;
            }
        }


        [FWExtServiceMethod(Descript = "设备注册接口")]
        [FWExtServiceParameters(Name = "MACHINENO", Descript = "设备编号", DefaultValue = "AC66666")]
        [FWExtServiceParameters(Name = "SSDW", Descript = "所属单位编号", DefaultValue = "450300000000")]
        [FWExtServiceParameters(Name = "SBFZR", Descript = "设备负责人账户", DefaultValue = "admin")]
        [FWExtServiceParameters(Name = "Phone", Descript = "联系电话", DefaultValue = "18598762345")]
        [FWExtServiceParameters(Name = "Address", Descript = "联系地址", DefaultValue = "广东省黄浦区彩频路")]
        public FWExtServiceResult MachineRegister(UnCaseSenseHashTable data, string MACHINENO)
        {
            FWExtServiceResult result = new FWExtServiceResult();           
            string MACHINE_IP = "";
            if (!string.IsNullOrEmpty(MACHINENO))
            {
                Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
                try
                {                    
                    session.BeginTransaction();
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备注册接口已开启");
                    Condition cdtIds = new Condition();
                    HttpRequest Request = System.Web.HttpContext.Current.Request;
                    if (string.IsNullOrEmpty(Request.ServerVariables.Get("Remote_Addr").ToString()))
                    {
                        session.Close();
                        result.RESULT = -1;
                        result.MSG = "获取设备IP地址为空";
                        return result;
                    }
                    else {
                        MACHINE_IP = Request.ServerVariables.Get("Remote_Addr").ToString();
                    }                        
                    cdtIds.AddSubCondition("AND", "MACHINENO", "=", MACHINENO);
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query(session, "B_MACHINE", cdtIds, "*", null, null, -1, -1);
                    if (records.Count != 0)
                    {
                        UnCaseSenseHashTable data3 = new UnCaseSenseHashTable();
                        data3["ID"] = records[0]["ID"];                       
                        data3["SBYXZT"] = "1";
                        data3["MACHINE_IP"] = MACHINE_IP;  
                        data3["REGISTER"] = DateTime.Now;
                        int r = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord(session, "B_MACHINE", data3, false);
                        session.Commit();
                        session.Close();
                        Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备注册接口已关闭");
                        result.RESULT = 1;
                        result.MSG = "更新注册成功";
                        return result;
                    }
                    if (records.Count == 0)
                    {
                        UnCaseSenseHashTable data2 = new UnCaseSenseHashTable();
                        data2["MACHINENO"] = MACHINENO;                       
                        data2["SBYXZT"] = "1";
                        data2["MACHINE_IP"] = MACHINE_IP;
                        data2["REGISTER"] = DateTime.Now;
                        int re = DbUtilityManager.Instance.DefaultDbUtility.InsertRecord("B_MACHINE", data2);
                        string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["oracleyc"].ToString();
                        String sql = "insert into S_DICT(LX,DM,V5,EDT_TIME)values(:LX,:DM,:V5,:EDT_TIME)";
                        OracleConnection conn = new OracleConnection(connString);
                        conn.Open();
                        OracleCommand cmd = new OracleCommand(sql, conn);
                        string LX = "Z451";
                        cmd.Parameters.Add(":LX", OracleDbType.NVarchar2, 600).Value = LX;
                        cmd.Parameters.Add(":DM", OracleDbType.NVarchar2, 600).Value = MACHINENO;
                        cmd.Parameters.Add(":V5", OracleDbType.NVarchar2, 600).Value = "1";
                        cmd.Parameters.Add(":EDT_TIME", OracleDbType.Date, 10000000).Value = DateTime.Now.Date;
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            conn.Close();
                            session.Rollback();
                            session.Close();
                            result.RESULT = -1;
                            result.MSG = "保存信息出错";
                            return result;
                        }
                        session.Commit();
                        session.Close();
                        Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备注册接口已关闭");
                        conn.Close();
                        result.RESULT = 1;
                        result.MSG = "注册成功";
                        return result;
                    }
                }
                catch (Exception e)
                {
                    Log4NetHelp.WriteErrorLog("设备注册接口发生异常：" + e.Message, e);
                    session.Rollback();
                    session.Close();
                    result.RESULT = -1;
                    result.MSG = e.Message;
                    return result;
                }
            }
            else {
                result.RESULT = -1;
                result.MSG = "设备编号不能为空";
                return result;
            }
            return result;
        }


        [FWExtServiceMethod(Descript = "设备是否注册接口")]
        [FWExtServiceParameters(Name = "MACHINENO", Descript = "设备编号", DefaultValue = "AC66666")]      
        public FWExtServiceResult MachineJudge(UnCaseSenseHashTable data, string MACHINENO)
        {
            FWExtServiceResult result = new FWExtServiceResult();                   
            if (!string.IsNullOrEmpty(MACHINENO))
            {
                //Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
                try
                {
                    //session.BeginTransaction();
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备是否注册接口已开启");
                    Condition cdtIds = new Condition();
                    cdtIds.AddSubCondition("AND", "MACHINENO", "=", MACHINENO);
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtIds, "*", null, null, -1, -1);
                    //session.Close();
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "设备是否注册接口已关闭");
                    if (records.Count != 0)
                    {
                        if (records[0]["DISABLED"].ToString() == "1")
                        {
                            result.RESULT = -1;
                            result.MSG = "设备已注册但未启用";
                            return result;

                        }
                        else
                        {
                            result.RESULT = 1;
                            result.MSG = "设备已注册启用";
                            return result;
                        }
                    }
                    else
                    {
                        result.RESULT = -1;
                        result.MSG = "设备未注册";
                        return result;
                    }                  
                }
                catch (Exception e)
                {                   
                    //session.Close();
                    Log4NetHelp.WriteErrorLog("设备是否注册接口发生异常：" + e.Message, e);
                    result.RESULT = -1;
                    result.MSG = e.Message;
                    return result;
                }
            }
            else
            {
                result.RESULT = -1;
                result.MSG = "设备编号不能为空";
                return result;
            }
        }



        [FWExtServiceMethod(Descript = "设备状态记录接口")]
        [FWExtServiceParameters(Name = "MACHINENO", Descript = "设备编号", DefaultValue = "AC66666")]
        [FWExtServiceParameters(Name = "SBYXZT", Descript = "设备状态代码,1在线，2离线")]
        public FWExtServiceResult MachineState(UnCaseSenseHashTable data, string MACHINENO, string SBYXZT)
        {
            FWExtServiceResult result = new FWExtServiceResult();
            if (!string.IsNullOrEmpty(MACHINENO))
            {
                Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
                try
                {
                    session.BeginTransaction();
                    Condition cdtIds = new Condition();
                    cdtIds.AddSubCondition("AND", "MACHINENO", "=", MACHINENO);
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query(session, "B_MACHINE", cdtIds, "*", null, null, -1, -1);
                    if (records.Count != 0)
                    {
                        if (records[0]["DISABLED"].ToString() == "1")
                        {
                            result.RESULT = -1;
                            result.MSG = "设备存在未启用";
                            session.Close();
                            return result;
                        }
                        else
                        {
                            UnCaseSenseHashTable data3 = new UnCaseSenseHashTable();
                            data3["ID"] = records[0]["ID"];
                            data3["SBYXZT"] = SBYXZT;
                            data3["UPDATE_ON"] = DateTime.Now;
                            int r = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord(session, "B_MACHINE", data3, false);
                            session.Commit();
                            session.Close();
                            result.RESULT = 1;
                            result.MSG = "状态更改成功";
                        }
                    }
                    else
                    {
                        result.RESULT = -1;
                        result.MSG = "设备不存在";
                        session.Close();
                        return result;
                    }
                }
                catch (Exception e)
                {
                    Log4NetHelp.WriteErrorLog("设备状态记录接口发生异常：" + e.Message, e);
                    session.Rollback();
                    session.Close();
                    result.RESULT = -1;
                    result.MSG = e.Message;
                    return result;
                }
            }
            else {
                result.RESULT = -1;
                result.MSG = "设备编号不能为空";
            }          
            return result;
        }
        static string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 

                pwd = pwd + s[i].ToString("X2");

            }
            return pwd;
        }

        [FWExtServiceMethod(Descript = "制证记录接口")]
        [FWExtServiceParameters(Name = "SYSTEMID", Descript = "Guid")]
        [FWExtServiceParameters(Name = "CSRQ", Descript = "出生日期")]
        [FWExtServiceParameters(Name = "FFDW", Descript = "发放单位代码 ")]
        [FWExtServiceParameters(Name = "FFDWMC", Descript = "发放单位名称")]
        [FWExtServiceParameters(Name = "FWCS", Descript = "服务处所（工作单位）")]
        [FWExtServiceParameters(Name = "GMSFHM", Descript = "身份证号码")]
        [FWExtServiceParameters(Name = "HJDXZ", Descript = "户籍地详址")]
        [FWExtServiceParameters(Name = "HJDZSSXQ", Descript = "")]
        [FWExtServiceParameters(Name = "HJDZSSXQMC", Descript = "")]
        [FWExtServiceParameters(Name = "JZZYXQJZRQ", Descript = "居住证有效期截止日期")]
        [FWExtServiceParameters(Name = "JZZYXQQSRQ", Descript = "居住证有效期起始日期")]
        [FWExtServiceParameters(Name = "MZ", Descript = "民族代码")]
        [FWExtServiceParameters(Name = "MZMC", Descript = "民族名称")]
        [FWExtServiceParameters(Name = "RESERVATION01", Descript = "打印用的户籍地址")]
        [FWExtServiceParameters(Name = "RESERVATION02", Descript = "打印用的居住地址")]
        [FWExtServiceParameters(Name = "RESERVATION36", Descript = "签发日期")]
        [FWExtServiceParameters(Name = "SLBH", Descript = "受理编号")]
        [FWExtServiceParameters(Name = "XB", Descript = "性别")]
        [FWExtServiceParameters(Name = "XJZDZQZ", Descript = "现居住地址")]
        [FWExtServiceParameters(Name = "XM", Descript = "姓名")]
        [FWExtServiceParameters(Name = "ZP", Descript = "")]
        [FWExtServiceParameters(Name = "ZPID", Descript = "")]
        [FWExtServiceParameters(Name = "ZZSBID", Descript = "制证设备ID")]
        [FWExtServiceParameters(Name = "ZZSY", Descript = "暂|居住事由")]
        [FWExtServiceParameters(Name = "ZZXXXRSJ", Descript = "信息写入时间")]
        [FWExtServiceParameters(Name = "ZZLX", Descript = "")]
        [FWExtServiceParameters(Name = "ZZFSSJ", Descript = "")]
        [FWExtServiceParameters(Name = "ZZXXZZSFCG", Descript = "制证是否成功 （0失败，1成功）")]
        public FWExtServiceResult MakecardRecord(UnCaseSenseHashTable data, string SYSTEMID, string CSRQ, string FFDW, string FFDWMC, string FWCS, string GMSFHM, string HJDXZ, string HJDZSSXQ, string HJDZSSXQMC, string JZZYXQJZRQ, string JZZYXQQSRQ, string MZ, string MZMC, string RESERVATION01, string RESERVATION02, string RESERVATION36, string SLBH, string XB, string XJZDZQZ, string XM, string ZP, string ZPID, string ZZSBID, string ZZSY, string ZZXXXRSJ, string ZZLX, string ZZFSSJ, string ZZXXZZSFCG, string ZZXXCWLX, string ZZXXCWLXMC, string ZZZXPH)
        {

            FWExtServiceResult result = new FWExtServiceResult();
            //Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
            try
            {
                if (!((!string.IsNullOrEmpty(ZZZXPH)&& ZZXXZZSFCG == "1"&& ZZZXPH!="") || ZZXXZZSFCG == "0")) {
                    result.RESULT = -1;
                    result.MSG = "制证芯片号不能为空";
                    return result;
                }

                    //string SYSTEMID = data["SYSTEMID"].ToString();
                    //DateTime CSRQ_Date = DateTime.ParseExact(CSRQ, "yyyy/MM/dd", null);string.IsNullOrEmpty(Request["cdt_combination"])
                    if (CSRQ == "" || JZZYXQJZRQ == "" || JZZYXQQSRQ == "" || RESERVATION36 == "" || ZZXXXRSJ == "" || ZP == "" || ZPID == "" || string.IsNullOrEmpty(CSRQ) || string.IsNullOrEmpty(JZZYXQJZRQ) || string.IsNullOrEmpty(JZZYXQQSRQ) || string.IsNullOrEmpty(RESERVATION36) || string.IsNullOrEmpty(ZZXXXRSJ) || string.IsNullOrEmpty(ZP) || string.IsNullOrEmpty(ZPID) || string.IsNullOrEmpty(ZZXXZZSFCG))
                    {
                        if (ZP == "" || ZPID == "" || string.IsNullOrEmpty(ZP) || string.IsNullOrEmpty(ZPID))
                        {
                            result.RESULT = -1;
                            result.MSG = "图像不能为空";
                        }
                        if (string.IsNullOrEmpty(ZZXXZZSFCG) || ZZXXZZSFCG == "")
                        {
                            result.RESULT = -1;
                            result.MSG = "制证结果不能为空";
                        }
                        else
                        {
                            result.RESULT = -1;
                            result.MSG = "时间不能为空";
                        }
                        return result;
                    }
                    else
                    {
                        byte[] ZP_byte = Convert.FromBase64String(ZP.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+"));
                        byte[] ZP_1K_byte = Convert.FromBase64String(ZPID.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+"));
                        DateTime CSRQ_Date = DateTime.ParseExact(CSRQ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        DateTime JZZYXQJZRQ_Date = DateTime.ParseExact(JZZYXQJZRQ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        DateTime JZZYXQQSRQ_Date = DateTime.ParseExact(JZZYXQQSRQ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        DateTime RESERVATION36_Date = DateTime.ParseExact(RESERVATION36, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        DateTime ZZXXXRSJ_Date = DateTime.ParseExact(ZZXXXRSJ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        //string FFDW = data["FFDW"].ToString();
                        //string FFDWMC = data["FFDWMC"].ToString();
                        //string FWCS = data["FWCS"].ToString();
                        //string GMSFHM = data["GMSFHM"].ToString();
                        //string HJDXZ = data["HJDXZ"].ToString();
                        //string HJDZSSXQ = data["HJDZSSXQ"].ToString();
                        //string HJDZSSXQMC = data["HJDZSSXQMC"].ToString();
                        //string JZZYXQJZRQ = data["JZZYXQJZRQ"].ToString();
                        //string JZZYXQQSRQ = data["JZZYXQQSRQ"].ToString();
                        //string MZ = data["MZ"].ToString();
                        //string MZMC = data["MZMC"].ToString();
                        //string RESERVATION01 = data["RESERVATION01"].ToString();
                        //string RESERVATION02 = data["RESERVATION02"].ToString();
                        //string RESERVATION36 = data["RESERVATION36"].ToString();
                        //string SLBH = data["SLBH"].ToString();
                        //string XB = data["XB"].ToString();
                        //string XJZDZQZ = data["XJZDZQZ"].ToString();
                        //string XM = data["XM"].ToString();
                        //string ZP = data["ZP"].ToString();
                        //string ZP_1K = data["ZP_1K"].ToString();
                        //string ZZSBID = data["ZZSBID"].ToString(); 
                        //string ZZSY = data["ZZSY"].ToString();
                        //string ZZXXXRSJ = data["ZZXXXRSJ"].ToString();
                        //string ZZLX = data["ZZLX"].ToString();
                        //string ZZFSSJ = data["ZZFSSJ"].ToString();
                        //string ZZXXZZSFCG = data["ZZXXZZSFCG"].ToString();
                        string sql_jl = "insert into C_JZZ_TMP_TC(SYSTEMID,CSRQ,FFDW,FFDWMC,FWCS,GMSFHM,HJDXZ,HJDZSSXQ,HJDZSSXQMC,JZZYXQJZRQ,JZZYXQQSRQ,MZ,MZMC,RESERVATION01,RESERVATION02,RESERVATION36,SLBH,XB,XJZDZQZ,XM,ZZSBID,ZZSY,ZZXXXRSJ,ZZXXZZSFCG,ZZXXCWLX,ZZXXCWLXMC,ZZZXPH)values(:SYSTEMID,:CSRQ,:FFDW,:FFDWMC,:FWCS,:GMSFHM,:HJDXZ,:HJDZSSXQ,:HJDZSSXQMC,:JZZYXQJZRQ,:JZZYXQQSRQ,:MZ,:MZMC,:RESERVATION01,:RESERVATION02,:RESERVATION36,:SLBH,:XB,:XJZDZQZ,:XM,:ZZSBID,:ZZSY,:ZZXXXRSJ,:ZZXXZZSFCG,:ZZXXCWLX,:ZZXXCWLXMC,:ZZZXPH)";
                    string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["oracleyc"].ToString();
                    OracleConnection conn = new OracleConnection(connString);
                        OracleCommand cmd = new OracleCommand(sql_jl, conn);
                        cmd.Parameters.Add(":SYSTEMID", OracleDbType.NVarchar2, 600).Value = SYSTEMID;
                        cmd.Parameters.Add(":CSRQ", OracleDbType.Date, 600).Value = CSRQ_Date;
                        cmd.Parameters.Add(":FFDW", OracleDbType.NVarchar2, 600).Value = FFDW;
                        cmd.Parameters.Add(":FFDWMC", OracleDbType.NVarchar2, 600).Value = FFDWMC;
                        cmd.Parameters.Add(":FWCS", OracleDbType.NVarchar2, 600).Value = FWCS;
                        cmd.Parameters.Add(":GMSFHM", OracleDbType.NVarchar2, 600).Value = GMSFHM;
                        cmd.Parameters.Add(":HJDXZ", OracleDbType.NVarchar2, 600).Value = HJDXZ;
                        cmd.Parameters.Add(":HJDZSSXQ", OracleDbType.NVarchar2, 600).Value = HJDZSSXQ;
                        cmd.Parameters.Add(":HJDZSSXQMC", OracleDbType.NVarchar2, 600).Value = HJDZSSXQMC;
                        cmd.Parameters.Add(":JZZYXQJZRQ", OracleDbType.Date, 600).Value = JZZYXQJZRQ_Date;
                        cmd.Parameters.Add(":JZZYXQQSRQ", OracleDbType.Date, 600).Value = JZZYXQQSRQ_Date;
                        cmd.Parameters.Add(":MZ", OracleDbType.NVarchar2, 600).Value = MZ;
                        cmd.Parameters.Add(":MZMC", OracleDbType.NVarchar2, 600).Value = MZMC;
                        cmd.Parameters.Add(":RESERVATION01", OracleDbType.NVarchar2, 600).Value = RESERVATION01;
                        cmd.Parameters.Add(":RESERVATION02", OracleDbType.NVarchar2, 600).Value = RESERVATION02;
                        cmd.Parameters.Add(":RESERVATION36", OracleDbType.Date, 600).Value = RESERVATION36_Date;
                        cmd.Parameters.Add(":SLBH", OracleDbType.NVarchar2, 600).Value = SLBH;
                        cmd.Parameters.Add(":XB", OracleDbType.NVarchar2, 600).Value = XB;
                        cmd.Parameters.Add(":XJZDZQZ", OracleDbType.NVarchar2, 600).Value = XJZDZQZ;
                        cmd.Parameters.Add(":XM", OracleDbType.NVarchar2, 600).Value = XM;
                        cmd.Parameters.Add(":ZZSBID", OracleDbType.NVarchar2, 600).Value = ZZSBID;
                        cmd.Parameters.Add(":ZZSY", OracleDbType.NVarchar2, 600).Value = ZZSY;
                        cmd.Parameters.Add(":ZZXXXRSJ", OracleDbType.Date, 600).Value = ZZXXXRSJ_Date;
                        cmd.Parameters.Add(":ZZXXZZSFCG", OracleDbType.NVarchar2, 600).Value = ZZXXZZSFCG;
                        cmd.Parameters.Add(":ZZXXCWLX", OracleDbType.NVarchar2, 600).Value = ZZXXCWLX;
                        cmd.Parameters.Add(":ZZXXCWLXMC", OracleDbType.NVarchar2, 600).Value = ZZXXCWLXMC;
                        cmd.Parameters.Add(":ZZZXPH", OracleDbType.NVarchar2, 600).Value = ZZZXPH;
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            result.RESULT = -1;
                            result.MSG = "存储制证数据失败";
                            conn.Close();
                            return result;
                        //session.Rollback();
                        //session.Close();
                        }
                        else
                        {
                            //byte[] ZP_byte = Convert.FromBase64String(ZP.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+"));
                            //byte[] ZP_1K_byte = Convert.FromBase64String(ZP_1K.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+")); ;                        
                            string querysql = "insert into C_JZZ_ZP_TC(SLBH,ZP,ZP_1K,SYSTEMID)values(:SLBH,:ZP,:ZP_1K,:SYSTEMID)";
                            OracleConnection connection = new OracleConnection(connString);
                            OracleCommand command = new OracleCommand(querysql, connection);
                            command.Parameters.Add(":SLBH", OracleDbType.NVarchar2, 600).Value = SLBH;
                            command.Parameters.Add(":ZP", OracleDbType.Blob, 2147483647).Value = ZP_byte;
                            command.Parameters.Add(":ZP_1K", OracleDbType.Blob, 2147483647).Value = ZP_1K_byte;
                            command.Parameters.Add(":SYSTEMID", OracleDbType.NVarchar2, 600).Value = SYSTEMID;
                            connection.Open();
                            int rowsAffected_ZP = command.ExecuteNonQuery();
                            conn.Close();
                            connection.Close();
                            connection.Dispose();
                            if (rowsAffected_ZP == 0)
                            {
                                //session.Rollback();
                                //session.Close();
                                result.RESULT = -1;
                                result.MSG = "存储图片数据失败";
                              return result;
                            }
                            else
                            {
                                result.RESULT = 1;
                                result.MSG = "存储成功";
                                return result;
                                  //session.Commit();
                            //session.Close();
                        }
                        }
                    }               
            }
            catch (Exception e)
            {
                //session.Rollback();
                //session.Close();
                result.RESULT = -1;
                result.MSG = e.Message;
                return result;
            }                   
            return result;
        }




        [FWExtServiceMethod(Descript = "白卡制证接口")]
        [FWExtServiceParameters(Name = "XPHM", Descript = "芯片号", DefaultValue = "AC66666123213")]
        [FWExtServiceParameters(Name = "DATE", Descript = "初始化时间", DefaultValue = "20190719000000")]
        [FWExtServiceParameters(Name = "MACHINE", Descript = "机器编号", DefaultValue = "AC66666")]
        [FWExtServiceParameters(Name = "OPERATE", Descript = "操作人", DefaultValue = "admin")]
        [FWExtServiceParameters(Name = "CARD_STATION", Descript = "卡片状态", DefaultValue = "1")]
        [FWExtServiceParameters(Name = "CARD_COMPANY", Descript = "卡片单位编号", DefaultValue = "450000000000")]
        public FWExtServiceResult CardInit(UnCaseSenseHashTable data, string XPHM, string DATE, string MACHINE, string OPERATE, string CARD_STATION, string CARD_COMPANY)
        {
            FWExtServiceResult result = new FWExtServiceResult();
            if (!string.IsNullOrEmpty(XPHM))
            {
                //Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();               
                try
                {                    
                    //session.BeginTransaction();
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡制证已开启");
                    Condition cdtIds = new Condition();                                    
                    cdtIds.AddSubCondition("AND", "XPHM", "=", XPHM);                   
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query("B_CARD_MAKE", cdtIds, "*", null, null, -1, -1);
                    if (records.Count >= 1) {
                        UnCaseSenseHashTable data1 = new UnCaseSenseHashTable();
                        data1["ID"] = records[0]["ID"];
                        data1["XPHM"] = XPHM;
                        data1["CARD_STATION"] = CARD_STATION;
                        data1["CARD_MACHINE"] = MACHINE;
                        data1["OPERATE"] = OPERATE;
                        if (CARD_COMPANY != null)
                        {
                            data1["CARD_COMPANY"] = CARD_COMPANY;
                        }
                        DateTime CREATE_TIME1 = DateTime.ParseExact(DATE, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        data1["CREATE_TIME"] = CREATE_TIME1;                      
                        int a = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord("B_CARD_MAKE", data1, false);
                        result.RESULT = 1;
                        result.MSG = "芯片状态已更新";
                        //session.Commit();
                        //session.Close();
                        return result;
                    }
                    UnCaseSenseHashTable data3 = new UnCaseSenseHashTable();
                    if (string.IsNullOrEmpty(DATE)|| DATE == "") {
                        result.RESULT = -1;
                        result.MSG = "初始化时间不能为空";
                        //session.Close();
                        //session.Rollback();
                        //session.Close();
                        return result;
                    }
                    logger.Debug("输入前的时间为"+DATE);
                    DateTime CREATE_TIME = DateTime.ParseExact(DATE, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                    logger.Debug("转换后的时间为"+CREATE_TIME);
                    data3["XPHM"] = XPHM;
                    data3["CARD_MACHINE"] = MACHINE;
                    data3["CARD_STATION"] = CARD_STATION;
                    data3["OPERATE"] = OPERATE;
                    data3["CREATE_TIME"] = CREATE_TIME;
                    if (CARD_COMPANY != null)
                    {
                        data3["CARD_COMPANY"] = CARD_COMPANY;                        
                    }
                    int r = DbUtilityManager.Instance.DefaultDbUtility.InsertRecord("B_CARD_MAKE", data3);
                    //int r = DbUtilityManager.Instance.DefaultDbUtility.InsertRecord(session, "B_CARD_MAKE", data3);
                    //session.Commit();
                    //session.Close();
                    if (0 == r)
                    {
                        result.RESULT = -1;
                        result.MSG ="插入失败";
                        //session.Rollback();
                        //session.Close();
                        return result;
                    }
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡制证已关闭");
                    //Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡制证当前会话sessionID为：" + dt.Rows[0][0].ToString() + "已关闭");
                    result.RESULT = 1;
                    result.MSG = "保存成功";
                    //session.Commit();
                    //session.Close();
                }
                catch (Exception e)
                {
                    //conn.Close();
                    //session.Rollback();
                    //session.Close();
                    Log4NetHelp.WriteErrorLog("白卡初始化发生异常：" + e.Message, e);
                    result.RESULT = -1;
                    result.MSG = e.Message;
                    //session.Rollback();
                    //session.Close();
                    return result;
                }
            }
            else
            {
                result.RESULT = -1;
                result.MSG = "芯片号不能为空";
            }
            return result;
        }





        [FWExtServiceMethod(Descript = "卡片单位编号查询接口")]
        [FWExtServiceParameters(Name = "XPHM", Descript = "设备编号", DefaultValue = "1534689940")]
        [FWExtServiceParameters(Name = "CARD_COMPANY", Descript = "单位编号", DefaultValue = "450300000000")]
        public FWExtServiceResult CardQuery(UnCaseSenseHashTable data, string XPHM, string CARD_COMPANY)
        {
            FWExtServiceResult result = new FWExtServiceResult();
            if (!string.IsNullOrEmpty(XPHM))
            {
                try
                {
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡单位查询已开启");
                    Condition cdtIds = new Condition();
                    cdtIds.AddSubCondition("AND", "XPHM", "=", XPHM);
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query("B_CARD_MAKE", cdtIds, "*", null, null, -1, -1);
                    if (records.Count == 0)
                    {
                        UnCaseSenseHashTable data3 = new UnCaseSenseHashTable();
                        data3["XPHM"] = XPHM;
                        data3["CARD_STATION"] = 4;
                        data3["CARD_STATION_FF"] = 4;
                        data3["CARD_COMPANY"] = CARD_COMPANY;
                        data3["CREATE_TIME"] = "20190101000000";
                        int r = DbUtilityManager.Instance.DefaultDbUtility.InsertRecord("B_CARD_MAKE", data3);
                        if (0 == r)
                        {
                            result.RESULT = -1;
                            result.MSG = "插入失败";
                            Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡单位查询已关闭");
                            return result;
                        }
                        result.RESULT = 1;
                        result.MSG = "可制证";
                        Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡单位查询已关闭");
                        return result;
                    }
                    else
                    {
                        if (records[0]["CARD_COMPANY"] != null && CARD_COMPANY.Length >= 4)
                        {
                            if (CARD_COMPANY.Substring(0, 4) == records[0]["CARD_COMPANY"].ToString().Substring(0, 4))
                            {
                                result.RESULT = 1;
                                result.MSG = "可制证";
                                Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡单位查询已关闭");
                                return result;
                            }
                            else
                            {
                                result.RESULT = -1;
                                result.MSG = "非同市白卡";
                                Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡单位查询已关闭");
                                return result;
                            }
                        }
                        else
                        {
                            UnCaseSenseHashTable data1 = new UnCaseSenseHashTable();
                            data1["XPHM"] = XPHM;
                            data1["ID"] = records[0]["ID"];
                            data1["CARD_COMPANY"] = CARD_COMPANY.Substring(0, 4)+"00000000";
                            int r = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord("B_CARD_MAKE", data1, false);
                            if (0 == r)
                            {
                                result.RESULT = -1;
                                result.MSG = "更新失败";
                                Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡单位查询已关闭");
                                return result;
                            }
                            result.RESULT = 1;
                            result.MSG = "可制证";
                            Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡单位查询已关闭");
                            return result;
                        }
                    }                 
                }
                catch (Exception e)
                {
                    Log4NetHelp.WriteErrorLog("白卡单位查询接口发生异常：" + e.Message, e);                    
                    result.RESULT = -1;
                    result.MSG = e.Message;
                    return result;
                }
            }
            else
            {
                result.RESULT = -1;
                result.MSG = "芯片号不能为空";
                return result;
            }            
        }



        [FWExtServiceMethod(Descript = "设备监控接口")]
        [FWExtServiceParameters(Name = "MACHINE_CODE", Descript = "设备编号", DefaultValue = "Z4510001")]
        [FWExtServiceParameters(Name = "COMPANY_CODE", Descript = "单位代码", DefaultValue = "45021123213")]
        [FWExtServiceParameters(Name = "GZCODE", Descript = "故障代码", DefaultValue = "2")]
        [FWExtServiceParameters(Name = "GZMS", Descript = "故障详情", DefaultValue = "提示错误代码404")]
        public FWExtServiceResult MachineMonitor(UnCaseSenseHashTable data, string MACHINE_CODE, string COMPANY_CODE, string GZCODE, string GZMS)
        {
            FWExtServiceResult result = new FWExtServiceResult();
            Session session = DbUtilityManager.Instance.DefaultDbUtility.CreateAndOpenSession();
            if (!string.IsNullOrEmpty(MACHINE_CODE) && MACHINE_CODE != "")
            {
                try
                {
                    UnCaseSenseHashTable data1 = new UnCaseSenseHashTable();
                    session.BeginTransaction();
                    Condition cdtIds = new Condition();
                    cdtIds.AddSubCondition("AND", "MACHINENO", "=", MACHINE_CODE);
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtIds, "*", null, null, -1, -1);
                    if (records.Count == 0)
                    {
                        session.Close();
                        result.RESULT = -1;
                        result.MSG = "设备表未添加此设备";
                        return result;
                    }
                    if (records[0]["SBYXZT"].ToString() == "3")
                    {
                        List<UnCaseSenseHashTable> records1 = DbUtilityManager.Instance.DefaultDbUtility.Query("select * from B_FAULT t where machine_code='" + MACHINE_CODE + "'and GZCODE='" + GZCODE + "' order by create_on desc", -1, -1);
                        if (records1.Count == 0)
                        {
                            data1["MACHINE_CODE"] = MACHINE_CODE;
                            data1["COMPANY_CODE"] = COMPANY_CODE;
                            data1["GZCODE"] = GZCODE;
                            data1["GZMS"] = GZMS;
                            data1["CREATE_ON"] = DateTime.Now;
                            int s = DbUtilityManager.Instance.DefaultDbUtility.InsertRecord(session, "B_FAULT", data1);
                            if (0 == s)
                            {
                                session.Rollback();
                                session.Close();
                                result.RESULT = -1;
                                result.MSG = "插入故障表失败";
                                return result;
                            }
                        }
                        else
                        {
                            data1["ID"] = records1[0]["ID"];
                            data1["GZCODE"] = GZCODE;
                            data1["GZMS"] = GZMS;
                            data1["CREATE_ON"] = DateTime.Now;
                            int a = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord(session, "B_FAULT", data1, false);
                            if (0 == a)
                            {
                                session.Rollback();
                                session.Close();
                                result.RESULT = -1;
                                result.MSG = "更新故障表失败";
                                return result;
                            }
                        }
                    }
                    else
                    {
                        data1["ID"] = records[0]["ID"];
                        data1["SBYXZT"] = "3";
                        int a = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord(session, "B_MACHINE", data1, false);
                        if (0 == a)
                        {
                            session.Rollback();
                            session.Close();
                            result.RESULT = -1;
                            result.MSG = "更新设备表设备状态失败";
                            return result;
                        }
                        UnCaseSenseHashTable data2 = new UnCaseSenseHashTable();
                        data2["MACHINE_CODE"] = MACHINE_CODE;
                        data2["COMPANY_CODE"] = COMPANY_CODE;
                        data2["GZCODE"] = GZCODE;
                        data2["GZMS"] = GZMS;
                        data2["CREATE_ON"] = DateTime.Now;
                        int s = DbUtilityManager.Instance.DefaultDbUtility.InsertRecord(session, "B_FAULT", data2);
                        if (0 == s)
                        {
                            session.Rollback();
                            session.Close();
                            result.RESULT = -1;
                            result.MSG = "插入故障表失败";
                            return result;
                        }
                    }
                    result.RESULT = 1;
                    result.MSG = "保存成功";
                    session.Commit();
                    session.Close();
                }
                catch (Exception e)
                {
                    session.Rollback();
                    session.Close();
                    result.RESULT = -1;
                    result.MSG = e.Message;
                    return result;
                }
            }
            else
            {
                session.Close();
                result.RESULT = -1;
                result.MSG = "设备编号不能为空";
            }
            return result;
        }



        [FWExtServiceMethod(Descript = "制证成功白卡状态更新接口")]
        [FWExtServiceParameters(Name = "XPHM", Descript = "卡芯片号", DefaultValue = "1534689940")]
        [FWExtServiceParameters(Name = "CARD_COMPANY", Descript = "单位编号", DefaultValue = "450100000000")]
        [FWExtServiceParameters(Name = "CARD_STATION", Descript = "制证状态", DefaultValue = "1")]
        public FWExtServiceResult CardUpdate(UnCaseSenseHashTable data, string XPHM, string CARD_COMPANY, string CARD_STATION)
        {
            FWExtServiceResult result = new FWExtServiceResult();
            if (!string.IsNullOrEmpty(XPHM))
            {
                try
                {
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡状态更新已开启");
                    Condition cdtIds = new Condition();
                    cdtIds.AddSubCondition("AND", "XPHM", "=", XPHM);
                    List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query("B_CARD_MAKE", cdtIds, "*", null, null, -1, -1);
                    if (records.Count == 0)
                    {
                        result.RESULT = -1;
                        result.MSG = "无该芯片号记录";
                        Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡状态更新已关闭");
                        return result;
                    }
                    UnCaseSenseHashTable data3 = new UnCaseSenseHashTable();
                    data3["XPHM"] = XPHM;
                    data3["ID"] = records[0]["ID"];
                    data3["CARD_COMPANY"] = CARD_COMPANY;
                    data3["CARD_STATION"] = CARD_STATION;
                    int r = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord("B_CARD_MAKE", data3, false);
                    if (r==0) {
                        result.RESULT = -1;
                        result.MSG = "更新失败";
                        Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡状态更新已关闭");
                        return result;
                    }
                    Condition cdtIds_Stock = new Condition();
                    cdtIds_Stock.AddSubCondition("AND", "COMPANY_ID", "=", CARD_COMPANY);
                    List<UnCaseSenseHashTable> records_Stock = DbUtilityManager.Instance.DefaultDbUtility.Query("B_CARD_STOCK", cdtIds_Stock, "*", null, null, -1, -1);
                    if (records_Stock.Count == 0)
                    {
                        result.RESULT = -1;
                        result.MSG = "无该库存记录";
                        return result;
                    }
                    else
                    {
                        UnCaseSenseHashTable data4 = new UnCaseSenseHashTable();
                        Int32 number = Convert.ToInt32(records_Stock[0]["STOCK_OVERPLUS"].ToString());
                        if (number - 1<0)
                        {
                            result.RESULT = -1;
                            result.MSG = "剩余库存不够发放";
                            Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "库存更新已关闭");
                            return result;

                        }
                        data4["STOCK_OVERPLUS"] = number-1;                       
                        data4["ID"] = records_Stock[0]["ID"];
                        int s = DbUtilityManager.Instance.DefaultDbUtility.UpdateRecord("B_CARD_STOCK", data4, false);
                        if (s == 0)
                        {
                            result.RESULT = -1;
                            result.MSG = "库存更新失败";
                            Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "库存更新已关闭");
                            return result;
                        }
                    }
                    result.RESULT = 1;
                    result.MSG = "更新成功";
                    Log4NetHelp.WriteDebugLog(DateTime.Now.ToString() + "白卡状态更新已关闭");
                    return result;
                }
                catch (Exception e)
                {
                    Log4NetHelp.WriteErrorLog("白卡状态更新发生异常：" + e.Message, e);
                    result.RESULT = -1;
                    result.MSG = e.Message;
                    return result;
                }
            }
            else
            {
                result.RESULT = -1;
                result.MSG = "芯片号不能为空";
            }
            return result;
        }
    }
}


