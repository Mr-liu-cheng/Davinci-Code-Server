using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class ChatCommad : Command
    {
        //readonly int type = (int)FirstCommands.ChatCommad;这里不需要 只是转发已封装好的 消息
        const int PrivateChat = 1;//表示私聊
        const int RoomChat = 2;//表示群聊

        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()
        {
            int type= BitConverter.ToInt32(bytes,8);//消息类型：私聊/群聊
            MessageInfo info = DataDo.Json2Object<MessageInfo>(Decode.DecodeSecondContendBtyes(bytes));
            Console.WriteLine("ChatCommad=>msg：" + " MYid:" + info.sendId + " Fridend:" + info.toIds + " Msg:" + info.content);
            if (Server.connMap != null)
            {
                //Console.WriteLine("当前连接数：" + Server.connMap.Count);
                if(type == PrivateChat)
                {
                    Conn receiver_conn;
                    Server.connMap.TryGetValue(info.toIds.ToString(), out receiver_conn);

                    if (receiver_conn != null)        //做一个判断receiver_conn是否为空，就不需要在做遍历了
                    {
                        receiver_conn.SendBts(bytes);             //转发给接收方
                        Console.WriteLine("已发送");
                    }
                    else { Console.WriteLine("发送失败，好友不在线"); }
                }
                else
                {
                    Console.WriteLine("群发");
                }
            }
        }
    }
}
