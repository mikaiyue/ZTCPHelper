using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
 
namespace ZTCPHelper
{
 
    /// <summary>
    /// Tcp服务
    /// </summary>
    public class ZTcpServer
    {
        private class ZTcpListener : TcpListener
        {
            public bool Active
            {
                get
                {
                    return base.Active;
                }
            }

            public ZTcpListener(int port)
                : base(port)
            {
            }

            public ZTcpListener(IPEndPoint localEP)
                : base(localEP)
            {
            }

            public ZTcpListener(IPAddress localaddr, int port)
                : base(localaddr, port)
            {
            }

        }

        private ZTcpListener tcpListener = null;

        public TcpListener Listener { get { return tcpListener; } }

        
        /// <summary>
        /// 是否对接收和发送的数据进行转义
        /// </summary>
        public bool IsDataEscape;
  

        /// <summary>
        /// 是否发出远程指令事件
        /// </summary>
        public bool IsCmdInvoke;

        /// <summary>
        /// 转义字符定义
        /// </summary>
        public ByteEscape EscapeCharDefine = new ByteEscape();

		/// <summary>
        /// KeyVal转义字符定义
        /// </summary>
        public KeyValStringEscape StringEscape = new KeyValStringEscape();

        public void Start()
        {
            lock (this)
            {
                tcpListener = new ZTcpListener(IPAddress.Any, port);
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(Acceptor), tcpListener);
            }
        }

        /// <summary>
        /// 远程链接接入事件
        /// </summary>
        public EventHandler<ZTcpServerEventArgs> OnRemoteAccepted;
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public EventHandler<ZTcpServerEventArgs> OnReciveData;
        /// <summary>
        /// 收到远程指令事件
        /// </summary>
        public EventHandler<ZTcpServerCmdEventArgs> OnCmdInvoke;
        /// <summary>
        /// 远程链接断开事件
        /// </summary>
        public EventHandler<ZTcpServerEventArgs> OnDisConnect;

        /// <summary>
        /// 客户端连接初始化
        /// </summary>
        /// <param name="o"></param>
        private   void Acceptor(IAsyncResult ar)
        {
            
            TcpListener server = ar.AsyncState as TcpListener;
            try
            {
                TcpClient remoteTcp = server.EndAcceptTcpClient(ar);
                //初始化连接的客户端 
                ZRemoteClient newClient = new ZRemoteClient(this, remoteTcp);
                AddClient(newClient);
                if (OnRemoteAccepted != null)
                {
                    OnRemoteAccepted(this, new ZTcpServerEventArgs(this, newClient, newClient.NetWork));
                }
                server.BeginAcceptTcpClient(new AsyncCallback(Acceptor), server);//继续监听客户端连接
                NetworkStream ns = newClient.NetWork.GetStream();
                ns.BeginRead(newClient.buffer, 0, newClient.buffer.Length, new AsyncCallback(ReciveCallBack), newClient);
            }
            catch (ObjectDisposedException ex)
            { //监听被关闭
            }
            catch (Exception ex)
            {
                
            }
        }

        private   void ReciveCallBack(IAsyncResult ar)
        {
            ZRemoteClient client = (ZRemoteClient)ar.AsyncState;
            
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
                        RemoveClient(client);
                    }

                }
                catch (Exception ex)
                {
                    RemoveClient(client);
                }
            }
        }

        /// <summary>
        /// 最大数据接收长度,1M
        /// </summary>
        public int MaxReciveLength = 1024 * 1024;

        private void ReciveData(ZRemoteClient client, int len)
        {
            byte[] ds = client.buffer;
            //如果接收数据大于最大数据接收长度，丢弃
            if (client.BufferStream.Length + len > MaxReciveLength)
                client.BufferStream.SetLength(0);
             
            if (IsDataEscape)
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
                        //发出数据接收事件
                        if (OnReciveData != null)
                            OnReciveData(this, new ZTcpServerEventArgs(this, client, client.NetWork));
                        //发出指令收到事件
                        if (IsCmdInvoke && (OnCmdInvoke != null))
                        {
                            string str = Encoding.GetString(d);
                            NameValueCollection cmd = StringEscape.StrToCollection(str);
                            OnCmdInvoke(this, new ZTcpServerCmdEventArgs(this, client, client.NetWork, cmd));
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
                    OnReciveData(this, new ZTcpServerEventArgs(this, client, client.NetWork));
                }
            }
        }

         

        private Encoding _encoding= System.Text.Encoding.Default;

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

        public void Stop()
        {
            lock (this)
            {
                if (tcpListener != null)
                {
                    tcpListener.Stop();
                }
                ZRemoteClient[] clients;
                lock (dicClients)
                {
                    clients =  dicClients.Values.ToArray ();
                    foreach (ZRemoteClient client in clients)
                    {
                        dicClients.Remove(client.IDKey);
                        client.Close();
                    }
                }
                if (OnDisConnect != null)
                {
                    foreach (ZRemoteClient client in clients)
                    {
                        OnDisConnect(this, new ZTcpServerEventArgs(this, client, client.NetWork));
                    }
                }

                tcpListener = null;
            }
        }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Active
        {
            get
            {
                return tcpListener != null ? tcpListener.Active : false;
            }
        }

        private int port;

        /// <summary>
        /// 监听端口
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                if (Active)
                    throw new Exception("Server is Running,Can not set Port");
                port = value;
            }
        }



        private Dictionary<string, ZRemoteClient> dicClients = new Dictionary<string, ZRemoteClient>(1024);


        public ZRemoteClient GetRemoteClient(string IDKey)
        {
            lock (dicClients)
            {
                ZRemoteClient r = null;
                dicClients.TryGetValue(IDKey, out r);
                return r;
            }
        }

        public ZRemoteClient[] AllRemoteClient
        {
            get
            {
                lock (dicClients)
                {
                    return dicClients.Values.ToArray();
                }
            }
        }


        internal void AddClient(ZRemoteClient client)
        {
            lock (dicClients)
            {
                dicClients.Add(client.IDKey, client);
                
            }
        }


        internal void RemoveClient(ZRemoteClient client)
        {
            lock (dicClients)
            {
                dicClients.Remove(client.IDKey);
                client.Close();
            }
            if (OnDisConnect != null)
                OnDisConnect(this, new ZTcpServerEventArgs(this, client, client.NetWork));

        }


    }

}
