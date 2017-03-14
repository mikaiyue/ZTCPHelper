using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZTCPHelper
{


    public class ByteEscape
    {

        public ByteEscape()
        {
            StartChar = 0x5B;  //[
            StartValue = 0x31; //1
            EscapeChar = 0x5C; //\
            EscapeValue = 0x32; //2
            EndChar = 0x5D; //]
            EndValue = 0x33; //3
        }

        public ByteEscape(byte start, byte startescape, byte escape, byte escapeesc, byte end, byte endescape)
        {
            StartChar = start;
            StartValue = startescape;
            EscapeChar = escape;
            EscapeValue = escapeesc;
            EndChar = end;
            EndValue = endescape;
        }

        /// <summary>
        /// 开始符'['
        /// </summary>
        public readonly byte StartChar;

        /// <summary>
        /// 开始符转义值
        /// </summary>
        public readonly byte StartValue;

        /// <summary>
        /// 转义符'\'
        /// </summary>
        public readonly byte EscapeChar;

        /// <summary>
        /// 转义符转义值
        /// </summary>
        public readonly byte EscapeValue;

        /// <summary>
        /// 结束符']'
        /// </summary>
        public readonly byte EndChar;

        /// <summary>
        /// 结束符转义值
        /// </summary>
        public readonly byte EndValue;

        /// <summary>
        /// 数据流转义
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Escape(byte[] input)
        {
            MemoryStream ms = new MemoryStream(input.Length + 32);
            ms.WriteByte(StartChar);
            foreach (byte b in input)
            {
                if (b == StartChar)
                {
                    ms.WriteByte(EscapeChar);
                    ms.WriteByte(StartValue);
                }
                else if (b == EscapeChar)
                {
                    ms.WriteByte(EscapeChar);
                    ms.WriteByte(EscapeValue);
                }
                else if (b == EndChar)
                {
                    ms.WriteByte(EscapeChar);
                    ms.WriteByte(EndValue);
                }
                else
                {
                    ms.WriteByte(b);
                }
            }
            ms.WriteByte(EndChar);
            return ms.ToArray();
        }

        /// <summary>
        /// 数据流反转义
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] UnEscape(byte[] input)
        {
            int l = input.Length;
            MemoryStream ms = new MemoryStream(l);
            int i = 0;
            while (i < l)
            {
                byte b = input[i];
                if (b == EscapeChar)
                {
                    i++;
                    if (i < l)
                    {
                        byte v = input[i];
                        if (v == StartValue)
                        {
                            ms.WriteByte(StartChar);
                        }
                        else if (v == EscapeValue)
                        {
                            ms.WriteByte(EscapeChar);
                        }
                        else if (v == EndValue)
                        {
                            ms.WriteByte(EndChar);
                        }
                    }

                }
                else if ((b != StartChar) && (b != EndChar))
                {
                    ms.WriteByte(b);
                }

                i++;

            }

            return ms.ToArray();
        }

    }



}
