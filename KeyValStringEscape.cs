using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;

namespace ZTCPHelper
{
    /// <summary>
    /// Key-Value数据转义及反转义：[k1-v1,k2-v2,k3-v3]转为k1=v1&k2=v2k3=v3格式的字符串
    /// </summary>
    public class KeyValStringEscape
    {

        public KeyValStringEscape()
        {
            EqualChar = '=';  //[
            EqualValue = '1'; //1
            EscapeChar = '%'; //\
            EscapeValue = '2'; //2
            JoinChar = '&'; //]
            JoinValue = '3'; //3
        }

        public KeyValStringEscape(char equalChar, char equalValue, char escape, char escapeValue, char joinChar, char joinValue)
        {
            EqualChar = equalChar;
            EqualValue = equalValue;
            EscapeChar = escape;
            EscapeValue = escapeValue;
            JoinChar = joinChar;
            JoinValue = joinValue;
        }

        /// <summary>
        /// 等于字符
        /// </summary>
        public readonly char   EqualChar;
        
        /// <summary>
        /// 等于字符转义值
        /// </summary>
        public readonly char EqualValue;

        /// <summary>
        /// 转义符'%'
        /// </summary>
        public readonly char EscapeChar;

        /// <summary>
        /// 转义符转义值
        /// </summary>
        public readonly char EscapeValue;

        /// <summary>
        ///  连接字符
        /// </summary>
        public readonly char JoinChar;

        /// <summary>
        /// 连接字符转义值
        /// </summary>
        public readonly char JoinValue;

        /// <summary>
        /// 字符串转义
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Escape(string input)
        {
            StringBuilder sb = new StringBuilder(input.Length + 10);

            foreach (char c in input)
            {
                if (c == EqualChar)
                {
                    sb.Append(EscapeChar);
                    sb.Append(EqualValue);
                }
                else if (c == EscapeChar)
                {
                    sb.Append(EscapeChar);
                    sb.Append(EscapeValue);
                }
                else if (c == JoinChar)
                {
                    sb.Append(EscapeChar);
                    sb.Append(JoinValue);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 数据流反转义
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string UnEscape(string input)
        {
            int l = input.Length;
            StringBuilder sb = new StringBuilder(l);
            int i = 0;
            while (i < l)
            {
                char c = input[i];
                if (c == EscapeChar)
                {
                    i++;
                    if (i < l)
                    {
                        char v = input[i];
                        if (v == EqualValue)
                        {
                            sb.Append(EqualChar);
                        }
                        else if (v == EscapeValue)
                        {
                            sb.Append(EscapeChar);
                        }
                        else if (v == JoinValue)
                        {
                            sb.Append(JoinChar);
                        }
                    }

                }
                else
                {
                    sb.Append(c);
                }
                i++;
            }

            return sb.ToString();
        }


        /// <summary>
        /// StrToDiction
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Dictionary<string, string> StrToDiction(string input)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] req = input.Split(EqualChar, JoinChar);

            int l = req.Length;
            int i = 0;
            while (i < l)
            {
                string k = UnEscape(req[i]);
                i++;
                string v = "";
                if (i < l)
                    v = UnEscape(req[i]);
                result[k] = v;
                i++;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public NameValueCollection StrToCollection(string input)
        {
            NameValueCollection result = new NameValueCollection();
            string[] req = input.Split(EqualChar, JoinChar);

            int l = req.Length;
            int i = 0;
            while (i < l)
            {
                string k = UnEscape(req[i]);
                i++;
                string v = "";
                if (i < l)
                    v = UnEscape(req[i]);
                result[k] = v;
                i++;
            }
            return result;
        }

        /// <summary>
        /// DictionToStr
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string DictionToStr(Dictionary<string, string> input)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> item in input)
            {
                sb.Append(Escape(item.Key));
                sb.Append(EqualChar);
                sb.Append(Escape(item.Value));
                sb.Append(JoinChar);
            }
            string str = sb.ToString();
            if (str.Length > 0)
            {
                str = str.Remove(str.Length - 1);
            }
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string CollectionToStr(NameValueCollection input)
        {
            StringBuilder sb = new StringBuilder();

            for (int i=0;i<input.Count;i++)
            {
                sb.Append(Escape(input.GetKey(i)));
                sb.Append(EqualChar);
                sb.Append(Escape(input.Get(i)));
                sb.Append(JoinChar);
            }
            string str = sb.ToString();
            if (str.Length > 0)
            {
                str = str.Remove(str.Length - 1);
            }
            return str;
        }

    }



}
