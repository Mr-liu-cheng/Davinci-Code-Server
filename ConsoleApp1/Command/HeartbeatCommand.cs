using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class HeartbeatCommand : Command
    {
        const int _HeartBeat = 0;//心跳命令
        const int _ReLink = 1;//断线重连
        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()
        {
            
            int command = BitConverter.ToInt32(bytes, 8);
            conn.ID = BitConverter.ToInt32(bytes, 12).ToString();

            switch (command)
            {
                case _HeartBeat:
                    // Console.WriteLine("收到"+ conn.ID + "的心跳命令---" +DateTime.Now);
                    ConnTime connTime = new ConnTime(conn, DateTime.Now);
                    Server.AddHeartTime(conn.ID, connTime);
                    break;
                case _ReLink:
                    PersonalInfo.ChangeStatusInfo(int.Parse(conn.ID), "", (int)PersonStatus.OnLine);
                    Console.WriteLine("玩家" + conn.ID + "重连" + DateTime.Now);
                    Conn receiver_conn;
                    if (Server.connMap.TryGetValue(conn.ID, out receiver_conn))
                    {
                        Console.WriteLine("修改成功Conn" + conn.ID + "重连" + DateTime.Now);

                        Server.connMap[conn.ID] = conn;
                    }
                    else
                    {
                        Console.WriteLine("connMap不存在改Id" + conn.ID + "重连" + DateTime.Now);
                        Server.connMap.Add(conn.ID, conn);
                    }
                    break;
            }
           
        }
    }
}
