using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections.Specialized;


namespace ZTCPHelper
{
    /// <summary>
    ///  ZTcpClient
    /// </summary>
    public class ZTcpClient
    {

        private TcpClient _NetWork = new TcpClient();


        public ZTcpClient()
        {


        }

        public string _key = Guid.NewGuid().ToString();

        /// <summary>
        /// 每个ZTcpClient都生成一个唯一ID
        /// </summary>
        public string IDKey
        {
            get
            {
                return _key;
            }
        }

        private object _Tag;
        public object Tag
        {
            get
            {
                return _Tag;
            }
            set
            {
                _Tag = value;
            }
        }

        public void Connect(string hostname, int port)
        {
            lock (_NetWork)
            {
                if (_NetWork.Connected)
                    throw new Exception("TcpClient is Connected,Can not Connect");

                _NetWork.Connect(hostname, port);
                _NetWork.GetStream().BeginRead(buffer, 0, buffer.Length, new AsyncCallback(ReciveCallBack), this);
            }
        }


        public TcpClient NetWork
        {
            get
            {
                return _NetWork;
            }
        }

        public EventHandler<ZTcpClientEventArgs> OnReciveData;

        /// <summary>
        /// 收到指令回复事件
        /// </summary>
        public EventHandler<ZTcpClientCmdEventArgs> OnCmdInvoke;

        public EventHandler<ZTcpClientEventArgs> OnDisConnect;

        private bool isDataEscape;
        /// <summary>
        /// 是否对接收和发送的数据进行转义
        /// </summary>
        public bool IsDataEscape
        {
            get
            {
                return isDataEscape;
            }
            set
            {
                if (_NetWork.Connected)
                    throw new Exception("TcpClient is Connected,Can not set IsDataEscape");
                else
                    isDataEscape = value;
            }
        }
        /// <summary>
        /// 转义字符定义
        /// </summary>
        public ByteEscape EscapeCharDefine = new ByteEscape();

        /// <summary>
        /// 是否发出指令调用事件
        /// </summary>
        public bool IsCmdInvoke;


        public KeyValStringEscape StringEscape = new KeyValStringEscape();

        private DateTime _lastReciveTime = DateTime.MinValue;
        /// <summary>
        /// 最后接收数据的时间
        /// </summary>
        public DateTime LastReciveTime
        {
            get
            {
                return _lastReciveTime;
            }
        }

        internal void RefreshLastReciveTime()
        {
            _lastReciveTime = DateTime.Now;
        }

        /// <summary>
        /// 数据接收缓存区
        /// </summary>
        internal byte[] buffer = new byte[1024];

        public MemoryStream BufferStream = new MemoryStream(1024 * 16);

        private Encoding _encoding = System.Text.Encoding.Default;

        public System.Text.Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
            }
        }

        private void ReciveCallBack(IAsyncResult ar)
        {
            ZTcpClient client = (ZTcpClient)ar.AsyncState;

            if (client.NetWork.Connected)
            {
                try
                {
                    NetworkStream ns = client.NetWork.GetStream();
                    int len = ns.EndRead(ar);
                    if (len > 0)
                    {
                        ReciveData(client, len);
                        ns.BeginRead(client.buffer, 0, client.buffer.Length, new AsyncCallback(ReciveCallBack), client);
                    }
                    else
                    {
                        DisConnect();
                    }

                }
                catch (Exception ex)
                {
                    DisConnect();
                }
            }
        }

        /// <summary>
        /// 最大数据接收长度,1M
        /// </summary>
        public int MaxReciveLength = 1024 * 1024;

        private void ReciveData(ZTcpClient client, int len)
        {
            byte[] ds = client.buffer;
            //如果接收数据大于最大数据接收长度，丢弃
            if (client.BufferStream.Length + len > MaxReciveLength)
                client.BufferStream.SetLength(0);
            client.RefreshLastReciveTime();
            if (isDataEscape)
            {
                for (int i = 0; i < len; i++)
                {
                    if (ds[i] == EscapeCharDefine.StartChar)
                    {
                        client.BufferStream.SetLength(0);
                        client.BufferStream.WriteByte(ds[i]);
                    }
                    else if (ds[i] == EscapeCharDefine.EndChar)
                    {
                        client.BufferStream.WriteByte(ds[i]);
                        byte[] d = EscapeCharDefine.UnEscape(client.BufferStream.ToArray());
                        client.BufferStream.SetLength(0);
                        client.BufferStream.Write(d, 0, d.Length);
                        if (OnReciveData != null)
                            OnReciveData(this, new ZTcpClientEventArgs(client, client.NetWork));

                        //发出指令收到事件
                        if (IsCmdInvoke && (OnCmdInvoke != null))
                        {
                            string str = Encoding.GetString(d);
                            NameValueCollection cmd = StringEscape.StrToCollection(str);
                            OnCmdInvoke(this, new ZTcpClientCmdEventArgs(this, client.NetWork, cmd));
                        }

                        client.BufferStream.SetLength(0);
                    }
                    else
                        client.BufferStream.WriteByte(ds[i]);

                }
            }
            else
            {
                if (OnReciveData != null)
                {
                    client.BufferStream.SetLength(0);
                    client.BufferStream.Write(ds, 0, len);
                    OnReciveData(this, new ZTcpClientEventArgs(client, client.NetWork));
                }
            }
        }


        public void SendData(byte[] data)
        {
            byte[] d = data;
            if (IsDataEscape)
            {
                d = EscapeCharDefine.Escape(data);
            }

            lock (NetWork)
            {
                NetworkStream ns = _NetWork.GetStream();
                ns.Write(d, 0, d.Length);
            }
        }

        public void SendData(NameValueCollection data)
        {
            string str = StringEscape.CollectionToStr(data);
            byte[] d = Encoding.GetBytes(str);
            SendData(d);
        }


        /// <summary>
        /// 断开客户端连接
        /// </summary>
        public void DisConnect()
        {
            if (_NetWork.Connected)
            {
                NetworkStream ns = _NetWork.GetStream();
                ns.Close();
                _NetWork.Close();

                if (OnDisConnect != null)
                    OnDisConnect(this, new ZTcpClientEventArgs(this, this.NetWork));
            }
        }
    }

}
