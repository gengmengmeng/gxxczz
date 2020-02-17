using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace MisFrameWork.core.Utils
{

    public sealed class EncryptUtils
    {
        public static string ConvertLongToHexString(long value, string spanChar)
        {
            byte[] bs = new byte[8];
            long tmpL = value;
            for (int i = 0; i < 8; i++)
            {
                bs[i] = (byte)(tmpL & 0xFF);
                tmpL = tmpL >> 8;
            }
            return ConvertBytesToHexString(bs, bs.Length, spanChar);
        }

        public static string ConvertStringToHexString(string strInput, string spanChar)
        {
            byte[] bs = System.Text.Encoding.GetEncoding("utf-8").GetBytes(strInput);
            return ConvertBytesToHexString(bs, bs.Length, spanChar);
        }

        /// <summary>
        /// 把16进制的字符串转换为相应的数组
        /// </summary>
        /// <param name="hexStr"></param>
        /// <returns></returns>
        public static byte[] ConvertHexStringToBytes(string hexStr)
        {
            string realHexStr = hexStr.Replace(" ", "").Replace("0x", "");
            if (realHexStr.Length < 2)
                return new byte[0];
            byte[] result = new byte[realHexStr.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(realHexStr.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public static string ConvertHexStringToString(string hexStr)
        {
            byte[] bs = ConvertHexStringToBytes(hexStr);
            return System.Text.Encoding.Default.GetString(bs);
        }

        public static string ConvertBytesToHexString(byte[] bytes, int len, string spanChar)
        {
            StringBuilder sb = new StringBuilder();
            if (len > bytes.Length)
                len = bytes.Length;
            string format = "{0:X002}" + spanChar;
            for (int i = 0; i < len; i++)
            {
                sb.AppendFormat(format, bytes[i]);
            }
            return sb.ToString();
        }

        #region Base64加密解密
        public static string Base64Encrypt(string hexStr)
        {
            return Base64Encrypt(ConvertHexStringToBytes(hexStr));

        }



        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <param name="encode">字符编码</param>
        /// <returns></returns>

        public static string Base64Encrypt(byte[] input)
        {
            return Convert.ToBase64String(input);
        }



        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字符串</param>
        /// <returns></returns>

        public static byte[] Base64Decrypt(string base64Str)
        {
            return Convert.FromBase64String(base64Str);
        }



        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字符串</param>
        /// <param name="encode">字符的编码</param>
        /// <returns></returns>
        public static string Base64DecryptToHexStr(string base64Str)
        {
            byte[] bs = Convert.FromBase64String(base64Str);
            return ConvertBytesToHexString(bs, bs.Length, "");
        }

        #endregion



        #region DES加密解密
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="data">加密数据</param>
        /// <param name="key">8位字符的密钥字符串(hexStr</param>
        /// <param name="iv">8位字符的初始化向量字符串</param>
        /// <returns></returns>
        public static string DESEncrypt(string dataHexStr, string keyHexStr)
        {
            byte[] byData = ConvertHexStringToBytes(dataHexStr);
            byte[] byKey = ConvertHexStringToBytes(keyHexStr);
            byte[] byIV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            cryptoProvider.Padding = PaddingMode.None;
            cryptoProvider.Mode = CipherMode.ECB;

            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
            cst.Write(byData, 0, byData.Length);
            cst.FlushFinalBlock();
            return ConvertBytesToHexString(ms.GetBuffer(), (int)ms.Length, "");
        }



        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="data">解密数据</param>
        /// <param name="key">8位字符的密钥字符串(需要和加密时相同)</param>
        /// <param name="iv">8位字符的初始化向量字符串(需要和加密时相同)</param>
        /// <returns></returns>
        public static string DESDecrypt(string dataHexStr, string keyHexStr)
        {

            byte[] byEnc = ConvertHexStringToBytes(dataHexStr);
            byte[] byKey = ConvertHexStringToBytes(keyHexStr);
            byte[] byIV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            cryptoProvider.Padding = PaddingMode.None;
            cryptoProvider.Mode = CipherMode.ECB;
            MemoryStream ms = new MemoryStream(byEnc);

            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            byte[] output = new byte[byEnc.Length];
            cst.Read(output, 0, output.Length);
            return ConvertBytesToHexString(output, output.Length, "");
        }

        #endregion

        #region 3DES 加密解密


        public static string DES3Encrypt(string dataHexStr, string keyHexStr)
        {
            return DES3Encrypt(dataHexStr, keyHexStr, false);
        }

        public static string DES3Encrypt(string dataHexStr, string keyHexStr, bool isCBC_Mode)
        {
            byte[] byData = ConvertHexStringToBytes(dataHexStr);
            byte[] byKey = ConvertHexStringToBytes(keyHexStr);
            byte[] byIV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            if (byKey.Length == 8)
                return DESEncrypt(dataHexStr, keyHexStr);
            TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
            DES.IV = byIV;
            DES.Key = byKey;
            if (isCBC_Mode)
                DES.Mode = CipherMode.CBC;
            else
                DES.Mode = CipherMode.ECB;
            DES.Padding = PaddingMode.None;
            ICryptoTransform encrypt = DES.CreateEncryptor();
            return ConvertBytesToHexString(encrypt.TransformFinalBlock(byData, 0, byData.Length), (int)byData.Length, "");
        }


        public static string DES3Decrypt(string dataHexStr, string keyHexStr)
        {
            return DES3Decrypt(dataHexStr, keyHexStr, false);
        }

        public static string DES3Decrypt(string dataHexStr, string keyHexStr, bool isCBC_Mode)
        {
            byte[] byData = ConvertHexStringToBytes(dataHexStr);
            byte[] byKey = ConvertHexStringToBytes(keyHexStr);
            byte[] byIV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            if (byKey.Length == 8)
                return DESDecrypt(dataHexStr, keyHexStr);
            TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
            DES.IV = byIV;
            DES.Key = byKey;
            if (isCBC_Mode)
                DES.Mode = CipherMode.CBC;
            else
                DES.Mode = CipherMode.ECB;
            DES.Padding = PaddingMode.None;
            ICryptoTransform decrypt = DES.CreateDecryptor();
            string result = "";
            try
            {
                result = ConvertBytesToHexString(decrypt.TransformFinalBlock(byData, 0, byData.Length), (int)byData.Length, "");
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public static string DES3Subpassword(string keyHexStr, string initValue)
        {
            string newInitValue = initValue.Replace(" ", "");
            string result = "";
            if (newInitValue.Length != 16)
                return "";
            result = DES3Encrypt(newInitValue, keyHexStr);
            byte[] bsInitValue = ConvertHexStringToBytes(newInitValue);
            for (int i = 0; i < bsInitValue.Length; i++)
                bsInitValue[i] = (byte)(bsInitValue[i] ^ 0xFF);
            newInitValue = ConvertBytesToHexString(bsInitValue, 8, "");
            result += DES3Encrypt(newInitValue, keyHexStr);
            return result;
        }
        #endregion

        #region MD5加密

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <returns></returns>
        public static string MD5EncryptString(string input)
        {
            return MD5EncryptString(input, new UTF8Encoding());
        }



        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <param name="encode">字符的编码</param>
        /// <returns></returns>
        public static string MD5EncryptString(string input, Encoding encode)
        {

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(encode.GetBytes(input));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
                sb.Append(t[i].ToString("X").PadLeft(2, '0'));
            return sb.ToString();
        }



        /// <summary>
        /// MD5对文件流加密
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static string MD5Encrypt(Stream stream)
        {
            MD5 md5serv = MD5CryptoServiceProvider.Create();
            byte[] buffer = md5serv.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            foreach (byte var in buffer)
                sb.Append(var.ToString("X2"));
            return sb.ToString();
        }



        /// <summary>
        /// MD5加密(返回16位加密串)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string MD5Encrypt16(string input, Encoding encode)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string result = BitConverter.ToString(md5.ComputeHash(encode.GetBytes(input)), 4, 8);
            result = result.Replace("-", "");
            return result.ToUpper();
        }

        #endregion

        #region RSA 非对称算法
        public static string[] RSAGenerateKeys()
        {
            string[] result = new string[2];
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            result[0] = rsa.ToXmlString(true);
            result[1] = rsa.ToXmlString(false);
            return result;
        }

        public static string RSAEncrypt(string dataHexStr, string sPublicKey)
        {
            byte[] byData = ConvertHexStringToBytes(dataHexStr);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(sPublicKey);
            byte[] cipherbytes;
            cipherbytes = rsa.Encrypt(byData, false);
            return ConvertBytesToHexString(cipherbytes, cipherbytes.Length, "");
        }

        public static string RSADecrypt(string dataHexStr, string sPrivateKey)
        {
            byte[] byData = ConvertHexStringToBytes(dataHexStr);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(sPrivateKey);
            byte[] cipherbytes;
            cipherbytes = rsa.Decrypt(byData, false);
            return ConvertBytesToHexString(cipherbytes, cipherbytes.Length, "");
        }

        #endregion
    }
}
