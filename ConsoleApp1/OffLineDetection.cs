using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;

namespace ConsoleApp1
{
    class OffLineDetection
    {
        DateTime currTime;
        DateTime lastUpdateTime;
        TimeSpan deltime;
        int times;
        const int OUTLINETIME = 7;//离线时间
        //public static List<Conn> clients = new List<Conn>();
        public static List<string> clients = new List<string>();
        
        /// <summary>
        /// 更新心跳包 离线判定
        /// </summary>
        public void Update()
        {
            if (Server.connHeartTime.Count < 1 || Server.connMap.Count < 1) return;

            currTime = System.DateTime.Now;
            if ((currTime - lastUpdateTime).TotalSeconds < 0.1) return;
            lastUpdateTime = currTime;

            foreach (KeyValuePair<string, ConnTime> connHTime in Server.connHeartTime)//改为for循环
            {
                
                deltime = currTime - connHTime.Value.dateTime;
                //times = (int)deltime.TotalMinutes;
                times = (int)deltime.TotalSeconds;
                if (times > OUTLINETIME)////////超过离线规定时间则掉线
                {
                    //存贮超时的客户端，移除操作在这个foreach 外面进行
                    clients.Add(connHTime.Key);
                }
            }

            Oppration();
        }

       /// <summary>
       /// 离线客户端处理
       /// </summary>
        void Oppration()
        {
            if (clients.Count > 0)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    ConnTime connInfo;
                    if (!Server.connHeartTime.TryGetValue(clients[i], out connInfo)) continue;
                    connInfo.conn.Close(connInfo.conn);//移除客户端，关闭连接
                    //Console.WriteLine("connMap.Count:" + Server.connMap.Count + "connHeartTime.Count:" + Server.connHeartTime.Count);
                }
                clients.Clear();
            }
        }
    }
}
