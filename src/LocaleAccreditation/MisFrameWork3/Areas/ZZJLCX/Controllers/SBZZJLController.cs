using iTextSharp.text;
using iTextSharp.text.pdf;
using MisFrameWork.core;
using MisFrameWork.core.db;
using MisFrameWork3.Classes.Controller;
using MisFrameWork3.Classes.Membership;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MisFrameWork3.Areas.ZZJLCX.Controllers
{
    public class SBZZJLController : FWBaseController    
    {
        // GET: ZZJLCX/SBZZJL
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult JsonConditionCombinationInfo()
        {
            return View();
        }

        #region __TIPS__:设备制证数量统计
        public ActionResult JsonDataList()
        {
            int RoleLevel = Membership.CurrentUser.RoleLevel;
            int rows = 30;
            int page = 1;
            try
            {
                rows = int.Parse(Request["rows"]);
                page = int.Parse(Request["page"]);
            }
            catch (Exception ee)
            {
                rows = 30; 
                page = 1;
                return Json(new { success = false, message = ee.Message }, JsonRequestBehavior.AllowGet);
            }
            string bignumber = (rows * page).ToString();
            string smallnumber = ((page - 1) * rows + 1).ToString();
            string sqlnumber = "";
            string sql = "";
            if (Request["cdt_combination"] != null)
            {
                if (!Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_ALL_ZZJL"))
                {
                    sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo();
                    sqlnumber = "select count(*) from (select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  "+ GetMachineNo();
                }
                else
                {
                    sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 ";
                    sqlnumber = "select count(*) from (select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1";
                }
                string jsoncdtCombination = System.Text.ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(Request["cdt_combination"]));
                Condition cdtCombination = Condition.LoadFromJson(jsoncdtCombination);
                cdtCombination.Relate = "AND";
                ReplaceCdtCombinationOpreate(cdtCombination);
                int count = cdtCombination.SubConditions.Count;
                if (count != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Condition c = cdtCombination.SubConditions[i];
                        if (c.Src == "ZZXXZZRQ")
                        {
                            sql += " " + c.Relate + " " + c.Src + " " + c.Op + "  TO_DATE('" + c.Tag + "', 'YYYY-MM-DD HH24:MI:SS') ";
                            sqlnumber += " " + c.Relate + " " + c.Src + " " + c.Op + "  TO_DATE('" + c.Tag + "', 'YYYY-MM-DD HH24:MI:SS') ";
                        }
                        if (c.Src == "COMPANY_ID_1")
                        {
                            string comId2 = "";
                            char[] a = c.Tag.ToString().ToCharArray();
                            string comId = "";
                            bool temp = false;
                            for (int j = a.Length - 2; j >= 0; j--)
                            {
                                string cc = a[j].ToString();
                                if (cc != "0" && !temp)
                                {
                                    temp = true;
                                }
                                if (temp)
                                {
                                    comId += a[j];
                                }
                            }
                            char[] charArray = comId.ToCharArray();
                            Array.Reverse(charArray);
                            comId2 = new String(charArray);
                            comId2 += "%";
                            sql += " " + c.Relate + " ZZXXZZDW like '" + comId2+"'";
                            sqlnumber += " " + c.Relate + " ZZXXZZDW like '" + comId2 + "'";
                        }
                        else
                        {
                            sql += " " + c.Relate + " " + c.Src + " " + c.Op + " '" + c.Tag + "'";
                            sqlnumber += " " + c.Relate + " " + c.Src + " " + c.Op + " '" + c.Tag + "'";
                        }
                    }
                }
                if (Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_OWN_ZZJL") && RoleLevel != 0)
                {
                    string userId = Membership.CurrentUser.UserId;
                    Condition cdtId = new Condition();
                    List<UnCaseSenseHashTable> record;
                    cdtId.AddSubCondition("AND", "SBFZR", "=", userId);
                    record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "MACHINENO", null, null, -1, -1);
                    string zzjbh = "(";
                    for (int i = 0; i < record.Count; i++)
                    {
                        zzjbh += "'" + record[i]["MACHINENO"] + "',";
                    }
                    zzjbh += "'0')";
                    sql += " AND ZZSBID in "+ zzjbh;
                    sqlnumber += " AND ZZSBID in " + zzjbh;
                }

                sql += " group by (ZZSBID,ZZXXZZDWMC)";
                sqlnumber += " group by (ZZSBID,ZZXXZZDWMC)";
            }
            if (!string.IsNullOrEmpty(sql))
            {
                try
                {
                    sqlnumber += ")";
                    sql = "select * from(SELECT rownum as rn, a.* FROM(" + sql + ") a) where rn> " + smallnumber + " and rn<= " + bignumber;
                    string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["oracleyc"].ToString();
                    OracleConnection conn = new OracleConnection(connString);
                    conn.Open();
                    OracleCommand cmd = new OracleCommand(sql, conn);
                    cmd.CommandType = CommandType.Text;
                    DataSet ds = new DataSet();
                    OracleDataAdapter da = new OracleDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    DataTable dt = new DataTable();
                    if (ds != null && ds.Tables.Count > 0)
                        dt = ds.Tables[0];
                    dt.Columns.Add("ZZXXZZDWMC");
                    int count = dt.Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Condition cdtId = new Condition();
                        string item = dt.Rows[i][1].ToString();
                        cdtId.AddSubCondition("AND", "MACHINENO", "=", item);
                        List<UnCaseSenseHashTable> record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "SSDW_V_D_FW_COMP__MC", null, null, -1, -1);
                        if (record.Count != 0)
                        {
                            dt.Rows[i]["ZZXXZZDWMC"] = record[0]["SSDW_V_D_FW_COMP__MC"];
                        }
                        else
                        {
                            dt.Rows[i]["ZZXXZZDWMC"] = "";
                        }
                    }
                    conn.Close();
                    OracleConnection conn_number = new OracleConnection(connString);
                    conn_number.Open();
                    OracleCommand cmd_number = new OracleCommand(sqlnumber, conn_number);
                    cmd_number.CommandType = CommandType.Text;
                    DataSet ds_number = new DataSet();
                    OracleDataAdapter da_number = new OracleDataAdapter();
                    da_number.SelectCommand = cmd_number;
                    da_number.Fill(ds_number);
                    DataTable dt_number = new DataTable();
                    if (ds_number != null && ds_number.Tables.Count > 0)
                        dt_number = ds_number.Tables[0];
                    int count1 = dt_number.Rows.Count;
                    int item_number = 0;
                    if (count1 == 1)
                    {
                        item_number = Convert.ToInt32(dt_number.Rows[0][0].ToString());
                    }
                    conn_number.Close();
                    return JsonDateObject(new { total = item_number, rows = dt });
                    //return JsonDateObject(dt);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                string query_sql;
                string query_sqlnumber;
                string search = Request["search"];
                if (string.IsNullOrEmpty(search))
                {
                    if (!Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_ALL_ZZJL"))
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo();
                        query_sqlnumber = "select count(*) from (select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo();
                    }
                    else
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 ";
                        query_sqlnumber = "select count(*) from (select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 ";
                    }
                }
                else
                {
                    if (!Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_ALL_ZZJL"))
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo() + " and ( ZZSBID like  '%" + search + "%' OR ZZXXZZDW like  '%" + search + "%' OR ZZXXZZDWMC like  '%" + search + "%') ";
                        query_sqlnumber = "select count(*) from (select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo() + " and ( ZZSBID like  '%" + search + "%' OR ZZXXZZDW like  '%" + search + "%' OR ZZXXZZDWMC like  '%" + search + "%') ";
                    }
                    else
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 and ( ZZSBID like  '%" + search + "%' OR ZZXXZZDW like  '%" + search + "%' OR ZZXXZZDWMC like  '%" + search + "%') ";
                        query_sqlnumber = "select count(*) from (select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 and ( ZZSBID like  '%" + search + "%' OR ZZXXZZDW like  '%" + search + "%' OR ZZXXZZDWMC like  '%" + search + "%') ";
                    }
                }
                if (Request["date_range_type"] != null && (Request["start_date"] != null || Request["end_date"] != null))
                {
                    string fieldName = null;
                    int dataRangeTypeIndex = 0;
                    try
                    {
                        dataRangeTypeIndex = int.Parse(Request["date_range_type"]);
                    }
                    catch (Exception e)
                    {
                        dataRangeTypeIndex = 0;
                    }
                    string[] arrDataRangeFields = new string[] { "制证时间" };
                    if (dataRangeTypeIndex != 0)
                    {
                        fieldName = arrDataRangeFields[dataRangeTypeIndex - 1];
                        if (!String.IsNullOrEmpty(Request["start_date"]))
                        {
                            query_sql += " AND ZZXXZZRQ >=  TO_DATE('" + Request["start_date"].ToString() + "', 'YYYY-MM-DD HH24:MI:SS') ";
                            query_sqlnumber += " AND ZZXXZZRQ >=  TO_DATE('" + Request["start_date"].ToString() + "', 'YYYY-MM-DD HH24:MI:SS') ";
                        }
                        if (!String.IsNullOrEmpty(Request["end_date"]))
                        {
                            DateTime dtEndDate = DateTime.Parse(Request ["end_date"]);
                            dtEndDate = dtEndDate.AddDays(1);//加多一天

                            query_sql += " AND ZZXXZZRQ <=  TO_DATE('" + dtEndDate.ToString() + "', 'YYYY-MM-DD HH24:MI:SS') ";
                            query_sqlnumber += " AND ZZXXZZRQ <=  TO_DATE('" + dtEndDate.ToString() + "', 'YYYY-MM-DD HH24:MI:SS') ";
                        }
                    }
                }

                if (Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_OWN_ZZJL") && RoleLevel != 0)
                {
                    string userId = Membership.CurrentUser.UserId;
                    Condition cdtId = new Condition();
                    List<UnCaseSenseHashTable> record;
                    cdtId.AddSubCondition("AND", "SBFZR", "=", userId);
                    record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "MACHINENO", null, null, -1, -1);
                    string zzjbh = "(";
                    for (int i = 0; i < record.Count; i++)
                    {
                        zzjbh += "'" + record[i]["MACHINENO"] + "',";
                    }
                    zzjbh += "'0')";
                    query_sql += " AND ZZSBID in "+ zzjbh;
                    query_sqlnumber += " AND ZZSBID in " + zzjbh;
                }
                query_sql += " group by (ZZSBID) ";
                query_sqlnumber += " group by (ZZSBID) ";

                try
                {
                    query_sqlnumber += ")";
                    string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["oracleyc"].ToString();
                    query_sql = "select * from(SELECT rownum as rn, a.* FROM("+query_sql+ ") a) where rn>= "+ smallnumber + " and rn<= "+ bignumber;
                    OracleConnection conn = new OracleConnection(connString);
                    conn.Open();
                    OracleCommand cmd = new OracleCommand(query_sql, conn);
                    cmd.CommandType = CommandType.Text;
                    DataSet ds = new DataSet();
                    OracleDataAdapter da = new OracleDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    DataTable dt = new DataTable();
                    if (ds != null && ds.Tables.Count > 0)
                        dt = ds.Tables[0];
                    dt.Columns.Add("ZZXXZZDWMC");
                    int count = dt.Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Condition cdtId = new Condition();
                        string item = dt.Rows[i][1].ToString();
                        cdtId.AddSubCondition("AND", "MACHINENO", "=", item);
                        List<UnCaseSenseHashTable> record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "SSDW_V_D_FW_COMP__MC", null, null, -1, -1);
                        if (record.Count != 0)
                        {
                            dt.Rows[i]["ZZXXZZDWMC"] = record[0]["SSDW_V_D_FW_COMP__MC"];
                        }
                        else {
                            dt.Rows[i]["ZZXXZZDWMC"] = "";
                        }                        
                    }
                    conn.Close();
                    OracleConnection conn_number = new OracleConnection(connString);
                    conn_number.Open();
                    OracleCommand cmd_number = new OracleCommand(query_sqlnumber, conn_number);
                    cmd_number.CommandType = CommandType.Text;
                    DataSet ds_number = new DataSet();
                    OracleDataAdapter da_number = new OracleDataAdapter();
                    da_number.SelectCommand = cmd_number;
                    da_number.Fill(ds_number);
                    DataTable dt_number = new DataTable();
                    if (ds_number != null && ds_number.Tables.Count > 0)
                        dt_number = ds_number.Tables[0];
                    int count1 = dt_number.Rows.Count;
                    int itemnumber = 0;
                    if (count1 == 1)
                    {
                        itemnumber = Convert.ToInt32(dt_number.Rows[0][0].ToString());
                    }
                    conn_number.Close();
                    return JsonDateObject(new { total = itemnumber, rows = dt });
                    //return JsonDateObject(dt);
                }
                catch (Exception ee)
                {
                    return Json(new { success = false, message = ee.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #region __TIPS__:框架通用函数 ( 字典控件相关 )
        public ActionResult JsonDicShort()
        {
            //__TIPS__:这里可以先过滤一下业务允许使用什么字典
            List<UnCaseSenseHashTable> records = GetDicData(Request["dic"], null);
            return Json(records, JsonRequestBehavior.AllowGet);
        }

        public ActionResult JsonDicLarge()
        {
            //__TIPS__:这里可以先过滤一下业务允许使用什么字典
            return QueryDataFromEasyUIDataGrid(Request["dic"], null, "DM,MC", null, "*");
        }

        public ActionResult ViewDicLargeUI()
        {
            /*
             * __TIPS__:有些特殊的字典可能需要显示更多的东西所以这里可以根据Request的值返回不同的视图
             *          以下演示根据字典内容，返回不同的视图。
             * */
            if ("V_D_FW_COMP".Equals(Request["dic"]))
            {
                return View("ViewSbzzjlType");
            }
            else
            {
                return View("~/Views/Shared/ViewCommonDicUI.cshtml");
            }
        }
        #endregion


        #endregion

        #region 打印数据
        public FileResult ActionPrint(string name, string oject_id)
        {
            //获取数据
            int RoleLevel = Membership.CurrentUser.RoleLevel;
            string sql = "";
            DataTable dt = new DataTable();
            if (Request["cdt_combination"] != null)
            {
                if (!Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_ALL_ZZJL"))
                {
                    sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo();
                }
                else
                {
                    sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 ";
                }
                string jsoncdtCombination = System.Text.ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(Request["cdt_combination"]));
                Condition cdtCombination = Condition.LoadFromJson(jsoncdtCombination);
                cdtCombination.Relate = "AND";
                ReplaceCdtCombinationOpreate(cdtCombination);
                int count = cdtCombination.SubConditions.Count;
                if (count != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Condition c = cdtCombination.SubConditions[i];
                        if (c.Src == "ZZXXZZRQ")
                        {
                            sql += " " + c.Relate + " " + c.Src + " " + c.Op + "  TO_DATE('" + c.Tag + "', 'YYYY-MM-DD HH24:MI:SS') ";
                        }
                        if (c.Src == "COMPANY_ID_1")
                        {
                            string comId2 = "";
                            char[] a = c.Tag.ToString().ToCharArray();
                            string comId = "";
                            bool temp = false;
                            for (int j = a.Length - 2; j >= 0; j--)
                            {
                                string cc = a[j].ToString();
                                if (cc != "0" && !temp)
                                {
                                    temp = true;
                                }
                                if (temp)
                                {
                                    comId += a[j];
                                }
                            }
                            char[] charArray = comId.ToCharArray();
                            Array.Reverse(charArray);
                            comId2 = new String(charArray);
                            comId2 += "%";
                            sql += " " + c.Relate + " ZZXXZZDW like '" + comId2 + "'";
                        }
                        else
                        {
                            sql += " " + c.Relate + " " + c.Src + " " + c.Op + " '" + c.Tag + "'";
                        }
                    }
                }
                if (Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_OWN_ZZJL") && RoleLevel != 0)
                {
                    string userId = Membership.CurrentUser.UserId;
                    Condition cdtId = new Condition();
                    List<UnCaseSenseHashTable> record;
                    cdtId.AddSubCondition("AND", "SBFZR", "=", userId);
                    record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "MACHINENO", null, null, -1, -1);
                    string zzjbh = "(";
                    for (int i = 0; i < record.Count; i++)
                    {
                        zzjbh += "'" + record[i]["MACHINENO"] + "',";
                    }
                    zzjbh += "'0')";
                    sql += " AND ZZSBID in " + zzjbh;
                }

                sql += " group by (ZZSBID,ZZXXZZDWMC)";
            }
            if (!string.IsNullOrEmpty(sql))
            {

                    string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["oracleyc"].ToString();
                    OracleConnection conn = new OracleConnection(connString);
                    conn.Open();
                    OracleCommand cmd = new OracleCommand(sql, conn);
                    cmd.CommandType = CommandType.Text;
                    DataSet ds = new DataSet();
                    OracleDataAdapter da = new OracleDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds != null && ds.Tables.Count > 0)
                        dt = ds.Tables[0];
                    dt.Columns.Add("ZZXXZZDWMC");
                    int count = dt.Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Condition cdtId = new Condition();
                        string item = dt.Rows[i][0].ToString();
                        cdtId.AddSubCondition("AND", "MACHINENO", "=", item);
                        List<UnCaseSenseHashTable> record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "SSDW_V_D_FW_COMP__MC", null, null, -1, -1);
                        if (record.Count != 0)
                        {
                            dt.Rows[i]["ZZXXZZDWMC"] = record[0]["SSDW_V_D_FW_COMP__MC"];
                        }
                        else
                        {
                            dt.Rows[i]["ZZXXZZDWMC"] = "";
                        }
                    }
                    conn.Close();
            }
            else
            {
                string query_sql;
                string search = Request["search"];
                if (string.IsNullOrEmpty(search))
                {
                    if (!Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_ALL_ZZJL"))
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo();
                    }
                    else
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 ";
                    }
                }
                else
                {
                    if (!Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_ALL_ZZJL"))
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where ZZSBID in  " + GetMachineNo() + " and ( ZZSBID like  '%" + search + "%' OR ZZXXZZDW like  '%" + search + "%' OR ZZXXZZDWMC like  '%" + search + "%') ";

                    }
                    else
                    {
                        query_sql = "select ZZSBID,count(*) as count from C_JZZ_TMP where 1=1 and ( ZZSBID like  '%" + search + "%' OR ZZXXZZDW like  '%" + search + "%' OR ZZXXZZDWMC like  '%" + search + "%') ";
                    }
                }
                if (Request["date_range_type"] != null && (Request["start_date"] != null || Request["end_date"] != null))
                {
                    string fieldName = null;
                    int dataRangeTypeIndex = 0;
                    try
                    {
                        dataRangeTypeIndex = int.Parse(Request["date_range_type"]);
                    }
                    catch (Exception e)
                    {
                        dataRangeTypeIndex = 0;
                    }
                    string[] arrDataRangeFields = new string[] { "制证时间" };
                    if (dataRangeTypeIndex != 0)
                    {
                        fieldName = arrDataRangeFields[dataRangeTypeIndex - 1];
                        if (!String.IsNullOrEmpty(Request["start_date"]))
                        {
                            query_sql += " AND ZZXXZZRQ >=  TO_DATE('" + Request["start_date"].ToString() + "', 'YYYY-MM-DD HH24:MI:SS') ";
                        }
                        if (!String.IsNullOrEmpty(Request["end_date"]))
                        {
                            DateTime dtEndDate = DateTime.Parse(Request["end_date"]);
                            dtEndDate = dtEndDate.AddDays(1);//加多一天

                            query_sql += " AND ZZXXZZRQ <=  TO_DATE('" + dtEndDate.ToString() + "', 'YYYY-MM-DD HH24:MI:SS') ";
                        }
                    }
                }

                if (Membership.CurrentUser.HaveAuthority("ZZJL.MACHINEZZTJ.QUERY_OWN_ZZJL") && RoleLevel != 0)
                {
                    string userId = Membership.CurrentUser.UserId;
                    Condition cdtId = new Condition();
                    List<UnCaseSenseHashTable> record;
                    cdtId.AddSubCondition("AND", "SBFZR", "=", userId);
                    record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "MACHINENO", null, null, -1, -1);
                    string zzjbh = "(";
                    for (int i = 0; i < record.Count; i++)
                    {
                        zzjbh += "'" + record[i]["MACHINENO"] + "',";
                    }
                    zzjbh += "'0')";
                    query_sql += " AND ZZSBID in " + zzjbh;
                }
                query_sql += " group by (ZZSBID) ";

                    string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["oracleyc"].ToString();
                    OracleConnection conn = new OracleConnection(connString);
                    conn.Open();
                    OracleCommand cmd = new OracleCommand(query_sql, conn);
                    cmd.CommandType = CommandType.Text;
                    DataSet ds = new DataSet();
                    OracleDataAdapter da = new OracleDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds != null && ds.Tables.Count > 0)
                        dt = ds.Tables[0];
                    dt.Columns.Add("ZZXXZZDWMC");
                    int count = dt.Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Condition cdtId = new Condition();
                        string item = dt.Rows[i][0].ToString();
                        cdtId.AddSubCondition("AND", "MACHINENO", "=", item);
                        List<UnCaseSenseHashTable> record = DbUtilityManager.Instance.DefaultDbUtility.Query("B_MACHINE", cdtId, "SSDW_V_D_FW_COMP__MC", null, null, -1, -1);
                        if (record.Count != 0)
                        {
                            dt.Rows[i]["ZZXXZZDWMC"] = record[0]["SSDW_V_D_FW_COMP__MC"];
                        }
                        else
                        {
                            dt.Rows[i]["ZZXXZZDWMC"] = "";
                        }
                    }
                    conn.Close();              
            }
            int count_print = dt.Rows.Count;
            //List<UnCaseSenseHashTable> records = DbUtilityManager.Instance.DefaultDbUtility.Query(QuerySql, -1, -1);

            //设置打印图纸大小
            Document document = new Document(PageSize.A4);
            //设置页边距
            document.SetMargins(36, 36, 36, 60);
            //中文字体
            string chinese = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "SIMSUN.TTC,1");
            BaseFont baseFont = BaseFont.CreateFont(chinese, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            //文字大小12，文字样式
            Font cn = new Font(baseFont, 14, Font.NORMAL);

            //PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(@"D:\temp.pdf", FileMode.Create));

            //这样写：是生成文件到内存中去
            var memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);//生成到内存中
            //writer.PageEvent = new PdfPageHelper();//页脚
            document.Open();//打开文件


            //Paragraph title = new Paragraph("国家工作人员登记备案表", new Font(baseFont, 23, Font.BOLD, BaseColor.BLACK));
            Paragraph title = new Paragraph("", new Font(baseFont, 23, Font.BOLD, BaseColor.BLACK));
            title.Alignment = Element.ALIGN_CENTER; //居中
            title.SpacingAfter = 20;
            document.Add(title);

            //数据表格
            PdfPTable table = new PdfPTable(4);
            table.SetWidths(new float[] { 2.5F, 8, 8,16});
            table.WidthPercentage = 100;
            AddBodyContentCell(table, "序号", cn);
            AddBodyContentCell(table, "设备编号", cn);
            AddBodyContentCell(table, "数量", cn);
            AddBodyContentCell(table, "制证单位", cn);

            for (int i = 0; i < count_print; i++)
            {
                //UnCaseSenseHashTable record = records[i];
                AddBodyContentCell(table, Convert.ToString(i + 1), cn);
                if (!string.IsNullOrEmpty(dt.Rows[i][0].ToString()))
                {
                    AddBodyContentCell(table, dt.Rows[i][0].ToString(), cn);
                }
                else
                {
                    AddBodyContentCell(table, "", cn);
                }
                
                if (dt.Rows[i][1].ToString()!= null)
                {
                    if (!string.IsNullOrEmpty(dt.Rows[i][1].ToString()))
                    {
                        AddBodyContentCell(table, dt.Rows[i][1].ToString(), cn);
                    }
                    else
                    {
                        AddBodyContentCell(table, "", cn);
                    }
                }
                else
                {
                    AddBodyContentCell(table, "", cn);
                }
                if (dt.Rows[i][2].ToString() != null)
                {
                    if (!string.IsNullOrEmpty(dt.Rows[i][2].ToString()))
                    {
                        AddBodyContentCell(table, dt.Rows[i][2].ToString(), cn);
                    }
                    else
                    {
                        AddBodyContentCell(table, "", cn);
                    }
                }
                else
                {
                    AddBodyContentCell(table, "", cn);
                }
            }
            document.Add(table);

            document.Close();

            var bytes = memoryStream.ToArray();
            //result = Convert.ToBase64String(bytes);

            return File(bytes, "application/pdf");
        }

        private void AddBodyContentCell(PdfPTable bodyTable, String text, iTextSharp.text.Font font, int rowspan = 2, bool needRightBorder = false)
        {
            PdfPCell cell = new PdfPCell();
            //float defaultBorder = 0.5f;
            //cell.BorderWidthLeft = defaultBorder;
            //cell.BorderWidthTop = 0;
            //cell.BorderWidthRight = needRightBorder ? defaultBorder : 0;
            //cell.BorderWidthBottom = defaultBorder;
            cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_BASELINE;
            //cell.Rowspan = rowspan;
            cell.PaddingBottom = 3;
            cell.Phrase = new Phrase(text, font);
            bodyTable.AddCell(cell);
        }
        #endregion
    }
}