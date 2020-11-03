using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace ConsoleApp1
{
    public struct ConnTime
    {
        public Conn conn;
        public DateTime dateTime;

        public ConnTime(Conn conn, DateTime dateTime)
        {
            this.conn = conn;
            this.dateTime = dateTime;
        }
    }

    public class Server
    {
        private Socket listenfd;
        public static Dictionary<string, Conn> connMap;
        public static Dictionary<string, ConnTime> connHeartTime;

        /// <summary>
        /// ip绑定开启监听
        /// </summary>
        /// <param name="ips">用域名就不用这个</param>
        /// <param name="port"></param>
        public void Start(string ips, int port)
        {
            connMap = new Dictionary<string, Conn>();
            connHeartTime = new Dictionary<string, ConnTime>();
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ips), port);
            listenfd.Bind(ipEndPoint);//监听指定的终端ip
            listenfd.Listen(0);
            listenfd.BeginAccept(AcceptM, null);
        }

        /// <summary>
        /// 异步接受回调
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptM(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                Conn cnn = new Conn(socket);
                cnn.Receive();
                listenfd.BeginAccept(AcceptM, null);

            }
            catch (Exception e) { }
        }

        /// <summary>
        /// 添加在线连接的客户端
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cnn"></param>
        public static void AddUser(string userId , Conn cnn)
        {
            if (connMap != null)
            {
                //Conn info;
                //if (connMap.TryGetValue(userId, out info)) return;//判断是否已存在账号
                if (connMap.ContainsKey(userId))
                {
                    ConsoleHelper.WriteColorLine("AddUser----已存在账号", ConsoleColor.Yellow);
                }
                else
                {
                    connMap.Add(userId, cnn);
                    ConsoleHelper.WriteColorLine("玩家："+userId + "加入，     connMap.Count：" + connMap.Count, ConsoleColor.Green);
                }
            }
        }

        /// <summary>
        /// 添加在线的客户端
        /// </summary>
        /// <param name="id"></param>
        /// <param name="heartInfo"></param>
        public static void AddHeartTime(string id, ConnTime heartInfo)
        {
            if (connHeartTime != null)
            {
                if (connHeartTime.ContainsKey(id))//判断是否已存在时间
                {
                    connHeartTime[id] = heartInfo;//覆盖时间
                }
                else
                {
                    connHeartTime.Add(id, heartInfo);//添加项
                }
            }
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="bts"></param>
        public static void Send2All(byte[] bts)
        {
            if (connMap == null) return;
            //遍历所有在线玩家并发送消息
            foreach (Conn v in connMap.Values)
            {
                if (v != null)
                {
                    v.SendBts(bts);
                }
            }
        }
    }
}
