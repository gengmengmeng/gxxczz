using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;

using log4net;
using MisFrameWork.core;
using MisFrameWork.core.db;
using MisFrameWork.core.db.Support;
using MisFrameWork.core.converter;


namespace MisFrameWork.core.WebTool
{
    /// <summary>
    /// 创建表的语句如下：&lt;br&gt;
    /// create table T_UPLOADED_FILE_2&lt;br&gt;
    ///    (&lt;br&gt;
    ///      ID               NUMBER not null,&lt;br&gt;
    ///      FILE_TYPE        VARCHAR2(5),--文件内容&lt;br&gt;
    ///      FILE_NAME        VARCHAR2(100),--上传后的文件名&lt;br&gt;
    ///      FILE_HINT        VARCHAR2(100),--上传后的文件名&lt;br&gt;
    ///      UPLOAD_TIME      DATE,--初次上传时间&lt;br&gt;
    ///      UPLOAD_COMPETED  NUMBER --上传完成标识。等于为全部份段上传完成。&lt;br&gt;
    ///    );&lt;br&gt;
    ///    --增加主键 ;&lt;br&gt;
    ///    alter table T_UPLOADED_FILE_2 add constraint PK_T_UPLOADED_FIELE_2 primary key (ID);
    ///    --建立序列 ;&lt;br&gt;
    ///    create sequence SEQ_T_UPLOADED_FILE_2;&lt;br&gt;
    ///    minvalue 1 ;&lt;br&gt;
    ///    start with 1 ;&lt;br&gt;
    ///    increment by 1; ;&lt;br&gt;
    ///    对于Oracle数据库，ID字段最好使用序列。对于其它数据库，可以使用自增值，但一定要配置临听器来获取保存后的值。;&lt;br&gt;
    ///    总之：在保存后这个字段应该马上能自动取到保存后的值作为文件名
    /// </summary>
    public class UploadFileTool
    {
        protected static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(UploadFileTool));
        private string uploadRootPath = "";

        /// <summary>
        /// 文件上传时的根目录
        /// </summary>
        public string UploadRootPath
        {
            get { return uploadRootPath; }
            set { 
                uploadRootPath = value;
                if (!uploadRootPath.EndsWith("\\") && !uploadRootPath.EndsWith("//"))
                    uploadRootPath += "\\";
            }
        }

        private IDataBaseUtility dataBaseUtility = DbUtilityManager.Instance.DefaultDbUtility;

        /// <summary>
        /// 数据库操作的工具类，保存数据等就使用这个类了。
        /// </summary>
        public IDataBaseUtility DataBaseUtility
        {
          get { return dataBaseUtility; }
          set { dataBaseUtility = value; }
        }

        private string uploadTableName = "T_UPLOADED_FILE_2";

        /// <summary>
        /// 保存上传文件信息的表名
        /// </summary>
        public string UploadTableName
        {
            get { return uploadTableName; }
            set { uploadTableName = value; }
        }

        /// <summary>
        /// 开始上传一个文件时使用
        /// </summary>
        /// <param name="uploadType">上传的类型。上传后会在根目录中创建这个名称的文件夹</param>
        /// <param name="sourceFileName">上传前文件在客户端的文件名</param>
        /// <param name="base64StringFileContent">文件的内容，使用BASE64加密算法转换为字符串</param>
        /// <param name="xmlFieldContent">一些附加的参数，可以写入数据库</param>
        /// <returns>上传成功返回文件的ID。</returns>
        public int BeginUpload(string uploadType, string sourceFileName, string base64StringFileContent, string xmlFieldContent, string xmlWriteByClientFieldContent)
        {
            string filePath = uploadType + "\\" + DateTime.Now.ToString("yyyyMM")+"\\";
            string fileFullPath = this.UploadRootPath + filePath;
            
            byte[] fileContent = Convert.FromBase64String(base64StringFileContent);

            System.IO.Directory.CreateDirectory(fileFullPath);
            UnCaseSenseHashTable uploadRecord = new UnCaseSenseHashTable();
            if (xmlFieldContent != null && !"".Equals(xmlFieldContent))
                uploadRecord.LoadFromXml(xmlFieldContent);
            uploadRecord["FILE_TYPE"] = uploadType;
            uploadRecord["UPLOAD_TIME"] = DateTime.Now;
            uploadRecord["UPLOAD_COMPETED"] = 0;
            if (xmlWriteByClientFieldContent != null && !"".Equals(xmlWriteByClientFieldContent))
                uploadRecord.LoadFromXml(xmlWriteByClientFieldContent);

            Session session = this.DataBaseUtility.CreateAndOpenSession();
            session.BeginTransaction();
            try
            {
                this.DataBaseUtility.InsertRecord(session, this.UploadTableName, uploadRecord);
                string fileName = uploadRecord["ID"].ToString().PadLeft(10, '0') + sourceFileName.Substring(sourceFileName.LastIndexOf('.'));
                //开始创建文件
                FileStream fs = new FileStream(fileFullPath + fileName, FileMode.CreateNew, FileAccess.Write);
                uploadRecord["FILE_NAME"] = filePath + fileName;
                DataBaseUtility.UpdateRecord(session, this.UploadTableName, uploadRecord, true);
                fs.Write(fileContent, 0, fileContent.Length);
                fs.Close();
                session.Commit();
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(uploadRecord.ToXmlString());
                }
                return int.Parse(uploadRecord["ID"].ToString());
            }
            catch (Exception e)
            {
                session.CurrentTransaction.Rollback();
                logger.Error("上传文件时出错！\n" + uploadRecord.ToXmlString(), e);
                throw e;
            }
            finally{
                session.Close();
            }

            return -1;
        }

        public int Upload(int fileid, string base64StringFileContent)
        {
            System.Collections.Hashtable key = new System.Collections.Hashtable();
            key["ID"]=fileid;
            UnCaseSenseHashTable record = this.DataBaseUtility.GetOneRecord(this.UploadTableName, key);
            if (record == null)
                return -1;
            string fileFullName = this.UploadRootPath + record["FILE_NAME"].ToString();

            if (base64StringFileContent == null || "".Equals(base64StringFileContent))//如果传空内容上来就返回总长度。
            {
                FileStream fs = new FileStream(fileFullName, FileMode.Open, FileAccess.Read);
                return (int)fs.Length;
            }
            else
            {
                byte[] fileContent = Convert.FromBase64String(base64StringFileContent);
                FileStream fs = new FileStream(fileFullName, FileMode.Append, FileAccess.Write);
                fs.Write(fileContent, 0, fileContent.Length);
                int result = (int)fs.Length;
                fs.Close();
                return result;
            }
        }

        public int EndUpload(int fileid)
        {
            System.Collections.Hashtable key = new System.Collections.Hashtable();
            key["ID"] = fileid;
            UnCaseSenseHashTable record = this.DataBaseUtility.GetOneRecord(this.UploadTableName, key); 
            if (record == null)
                return -1;
            else
            {
                record["UPLOAD_COMPETED"] = 1;
                Session session = DataBaseUtility.CreateAndOpenSession();
                try{
                    DataBaseUtility.UpdateRecord(session, this.UploadTableName, record, true);
                }
                finally{
                    session.Close();
                }
                return 1;
            }
        }

        public string GetFileContentBase64(int fileid)
        {
            UnCaseSenseHashTable record = DbUtilityManager.Instance.DefaultDbUtility.GetOneRecord(this.UploadTableName, fileid);
            string fileFullPath = this.UploadRootPath + record["FILE_NAME"];
            System.IO.FileStream fs = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs,0,(int)fs.Length);
            fs.Close();

            return Convert.ToBase64String(bs);
        }
    }
}
