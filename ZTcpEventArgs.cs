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


    public class ZTcpServerEventArgs : EventArgs
    {
        private TcpClient _Client;

        public TcpClient Client
        {
            get { return _Client; }
            private set { _Client = value; }
        }

        private ZTcpServer _TcpServer;

        public ZTcpServer ZServer
        {
            get { return _TcpServer; }
            private set { _TcpServer = value; }
        }

        private ZRemoteClient _ZClient;

        public ZRemoteClient ZClient
        {
            get { return _ZClient; }
            private set { _ZClient = value; }
        }

        public ZTcpServerEventArgs(ZTcpServer zserver, ZRemoteClient zclient, TcpClient client)
        {
            _TcpServer = zserver;
            _ZClient = zclient;
            _Client = client;
        }
        
    }

    public class ZTcpServerCmdEventArgs : ZTcpServerEventArgs
    {
        private NameValueCollection _Data;

        public NameValueCollection Data
        {
            get { return _Data; }
            private set { _Data = value; }
        }

        public ZTcpServerCmdEventArgs(ZTcpServer zserver, ZRemoteClient zclient, TcpClient client, NameValueCollection data)
            : base(zserver, zclient, client)
        {
            _Data = data;
        }
    }

    public class ZTcpClientEventArgs : EventArgs
    {
        private TcpClient _Client;

        public TcpClient Client
        {
            get { return _Client; }
            private set { _Client = value; }
        }

        private ZTcpClient _ZTcpClient;

        public ZTcpClient ZClient
        {
            get { return _ZTcpClient; }
            private set { _ZTcpClient = value; }
        }


        public ZTcpClientEventArgs(ZTcpClient zclient, TcpClient client)
        {
            _ZTcpClient = zclient;
            _Client = client;
        }

    }


    public class ZTcpClientCmdEventArgs : ZTcpClientEventArgs
    {
        private NameValueCollection _Data;

        public NameValueCollection Data
        {
            get { return _Data; }
            private set { _Data = value; }
        }

        public ZTcpClientCmdEventArgs(ZTcpClient zclient, TcpClient client, NameValueCollection data)
            : base(zclient, client)
        {
            _Data = data;
        }

    }

  


}
