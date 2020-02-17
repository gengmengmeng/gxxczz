using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MisFrameWork.core.WebTool
{
    public static class CryptHelper
    {
        #region RSA加密
        /// <summary>
        /// 用指定公钥加密文本
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="input">文本</param>
        /// <returns></returns>
        public static string RsaEncrypt(string publicKey, string input)
        {
            const string sFormat =
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            var provider = new RSACryptoServiceProvider();
            var arrPubKey = Hex2Bytes(publicKey);
            var pkB64 = Convert.ToBase64String(arrPubKey);
            var xmlStr = string.Format(sFormat, pkB64);
            var bytesInput = Encoding.UTF8.GetBytes(input);
            provider.FromXmlString(xmlStr);
            var encryptArray = provider.Encrypt(bytesInput, false);
            var hexStr = BitConverter.ToString(encryptArray).Replace("-", "");
            return hexStr.ToLower();
        }
        #endregion
        #region 公用函数
        /// <summary>
        /// 将字节转换成16进制数字
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static string Bytes2Hex(byte[] bytes)
        {
            var buffer = new StringBuilder();
            foreach (var t in bytes)
            {
                buffer.AppendFormat("{0:x2}", t);
            }
            return buffer.ToString();
        }
        /// <summary>
        /// 将16进制数组转换成字节
        /// </summary>
        /// <param name="input">16进制数组</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static byte[] Hex2Bytes(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var offset = input.Length % 2;
            if (offset == 1) input = "0" + input;
            int i;
            var list = new List<byte>();
            for (i = 0; i < input.Length; i += 2)
            {
                var temp = input.Substring(i, 2);
                byte bv;
                var success = byte.TryParse(temp, NumberStyles.HexNumber, null, out bv);
                if (!success) throw new ArgumentOutOfRangeException();
                list.Add(bv);
            }
            return list.ToArray();
        }
        #endregion
        #region SHA1加密
        /// <summary>
        /// SHA1加密 使用缺省密钥给字符串加密
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        public static string Sha1Encrypt(string sourceString)
        {
            var data = Encoding.Default.GetBytes(sourceString);
            HashAlgorithm sha = new SHA1CryptoServiceProvider();
            var bytes = sha.ComputeHash(data);
            return BitConverter.ToString(bytes).Replace("-", "");
        }
        #endregion
        #region DES加解密
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="pubKey"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DesEncrypt(string pubKey, string input)
        {
            var des = new DESCryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(input);
            des.Key = Encoding.ASCII.GetBytes(pubKey);
            des.IV = Encoding.ASCII.GetBytes(pubKey);
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                    cryptoStream.FlushFinalBlock();
                }
                var data = ms.ToArray();
                return BitConverter.ToString(data).Replace("-", "");
            }
        }
        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="pubKey"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DesDecrypt(string pubKey, string input)
        {
            var des = new DESCryptoServiceProvider();
            var bytes = Hex2Bytes(input);
            des.Key = Encoding.ASCII.GetBytes(pubKey);
            des.IV = Encoding.ASCII.GetBytes(pubKey);
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                    cryptoStream.FlushFinalBlock();
                }
                var data = ms.ToArray();
                return Encoding.UTF8.GetString(data);
            }
        }
        #endregion
        #region MD5加密
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">输入</param>
        /// <returns></returns>
        public static string Md5Encrypt(string input)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(input);
            var encs = md5.ComputeHash(data);
            return BitConverter.ToString(encs).Replace("-", "").ToUpper();
        }
        #endregion
    }
}
