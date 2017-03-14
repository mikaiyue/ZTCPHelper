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

   public  class ZRemoteClient
    {
        /// <summary>
        /// TCP客户端
        /// </summary>
        private TcpClient _NetWork = null;

        private ZTcpServer _TcpServer = null;

        public ZRemoteClient(ZTcpServer TcpServer, TcpClient client)
          {
              _TcpServer = TcpServer;
              _NetWork = client;
              _NetWork.Client.NoDelay = true;
              _RemoteEndPoint = _NetWork.Client.RemoteEndPoint;
              
          }

        private EndPoint _RemoteEndPoint;
        public EndPoint RemoteEndPoint
        {
            get
            {
                return _RemoteEndPoint;
            }
        }

        public string _key = Guid.NewGuid().ToString();

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

        public TcpClient NetWork
        {
            get
            {
                return _NetWork;
            }
        }



        /// <summary>
        /// 数据接收缓存区
        /// </summary>
        internal byte[] buffer = new byte[1024];

        public MemoryStream BufferStream = new MemoryStream(1024*16);

        internal void Close()
        {
            lock (NetWork)
            {
                if (_NetWork.Connected)
                {
                    NetworkStream ns = _NetWork.GetStream();
                    ns.Close();
                    _NetWork.Close();
                }
            }
            
        }

        public void SendData(byte[] data)
        {
            byte[] d = data;
            if (_TcpServer.IsDataEscape)
            {
                d = _TcpServer.EscapeCharDefine.Escape(data);
            }

            lock (NetWork)
            {
                NetworkStream ns = _NetWork.GetStream();
                ns.Write(d, 0, d.Length);
            }
 
        }

        public void SendData(NameValueCollection data)
        {
            string str = _TcpServer.StringEscape.CollectionToStr(data);
            byte[] d = _TcpServer.Encoding.GetBytes(str);
            SendData(d);
        }
        /// <summary>
        /// 断开客户端连接
        /// </summary>
        public void DisConnect()
        {
            _TcpServer.RemoveClient(this);
        }

 

    }

}
