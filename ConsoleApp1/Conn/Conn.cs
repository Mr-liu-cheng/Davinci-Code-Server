using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace ConsoleApp1
{
    public class Conn
    {
        //static Mess msg = new Mess();//好像没用

        public const int BUF_SIZE = 1024 * 1024 * 10;
        Socket socket;
        /// <summary>
        /// 判断对象是否使用（用于断线连接的判断“形成对象池”）
        /// </summary>
        //private bool isUse; 
        byte[] readBuff = new byte[BUF_SIZE];
        /// <summary>
        /// 上一次残留消息的长度
        /// </summary>
        int buffCount=0;
        //public DateTime heartTime;//心跳最后一次时间
        public string ID;//账号

        //只有一个数据，所以必须是静态的
        public static Int32 msgLength = 0;//数据长度
        public static byte[] lenBytes = new byte[sizeof(Int32)];//数据长度的数组
        public static byte[] whole;//完整消息
        int msgTotalLength;//消息总长（消息的头长度+数据长度）
        const int HEADLENGTH = 8;//消息的头长度（命令+数据长度）

        /// <summary>
        /// 为监听到的数据包绑定socket
        /// </summary>
        /// <param name="socket"></param>
        public Conn(Socket socket)
        {
            this.socket = socket;
            //isUse = true;
        }

        /// <summary>
        /// 计算剩余的空间
        /// </summary>
        /// <returns></returns>
        public int BuffReamin()
        {
            return BUF_SIZE - buffCount;
        }

        /// <summary>
        /// 被动离线关闭连接（unity关闭运行按钮执行）
        /// </summary>
        public void Close(Conn conn)//关闭连接应该还要移除字典里的数据
        {
            //if (isUse) return;
            RemoveData(conn);
            socket.Close();

            //Console.WriteLine("Close(Conn conn)------被动离线（断网）");
            //isUse = false;//开关用于让程序关闭连接（只执行一次）
        }

        /// <summary>
        /// 擦出所有数据
        /// </summary>
        /// <param name="conn"></param>
        void RemoveData(Conn conn)
        {
            //修改个人状态
            PersonalInfo.ChangeStatusInfo(int.Parse(conn.ID), "", (int)PersonStatus.OffLine);
            //房间移除成员
            string strContent = "SELECT * FROM counter where id=" + conn.ID + " ;";
            PersonalInfo user = SqlConn.Select(strContent);
            ConsoleHelper.WriteColorLine("用户：" + user.id + "已超时断开连接   所在房间：" + user.roomNum, ConsoleColor.Red);

            //玩家退出不修改其所在房间的id值，为以后做离线重连继续战斗做准备
            RoomInfo roomInfo = GameCommand.GetRoom(user.roomNum);
            if (roomInfo != null)
            {
                roomInfo.RemoveMember(user.id);
            }

            Server.connMap.Remove(conn.ID);//清除连接
            Server.connHeartTime.Remove(conn.ID);//清除心跳包
            OffLineDetection.clients.Remove(conn.ID);//清除离线客户
        }

        /// <summary>
        /// 接收消息（多线程）
        /// </summary>
        public void Receive()
        {
            socket.BeginReceive(readBuff, buffCount, BuffReamin(), SocketFlags.None, ReceiveM, this);
        }

        /// <summary>
        /// 消息接收的异步回调
        /// </summary>
        /// <param name="ar"></param>
        public void ReceiveM(IAsyncResult ar)
        {
            try
            {
                int count = socket.EndReceive(ar);
                if (count <= 0)
                {
                    this.Close(this);//主动退出
                    return;
                }
                buffCount += count;
                //Console.WriteLine("命令：" + BitConverter.ToInt32(readBuff, 0));
                ProcessData();
                //继续接收	
                socket.BeginReceive(readBuff, buffCount, BuffReamin(), SocketFlags.None, ReceiveM, this);
            }
            catch (Exception e) { }//这里的抓错不能删因为当客户端掉线时会导致访问不到socket所以要try，不执行里面的代码
        }

        /// <summary>
        /// 粘包处理
        /// </summary>
        private void ProcessData()
        {
            //小于长度字节
            if (buffCount < HEADLENGTH)//命令+长度=2 int
            {
                Console.WriteLine("长度：" + buffCount);
                Console.WriteLine("buffCount < 8");
                return;
            }

            //消息长度
            // Array.Copy(readBuff, 4, lenBytes, 0, sizeof(Int32));
            msgLength = BitConverter.ToInt32(readBuff, 4);//根据从数组第4位开始获取int（4字节长度）的对应int值获取长度值
            msgTotalLength = msgLength + HEADLENGTH;

            if (buffCount < msgTotalLength)
            {
                Console.WriteLine("长度：" + buffCount);
                Console.WriteLine("buffCount < msgLength + 8");
                return;
            }

            //处理消息
            whole = new byte[msgTotalLength];//完整的消息长度
            Array.Copy(readBuff, whole, msgTotalLength);
            //命令处理
            TypeDo.DoType(whole, this);

            //清除已处理的消息
            int count = buffCount - msgTotalLength;
            Array.Copy(readBuff, msgTotalLength, readBuff, 0, count);
            buffCount = count;
            if (buffCount > 0)
            {
                ProcessData();//如果消息过大还可以继续处理看能不能再找出完整的消息
            }
        }

        /// <summary>
        /// 发送消息（字符串）
        /// </summary>
        /// <param name="str"></param>
        public void Send(string str)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            socket.Send(bytes);
        }

        /// <summary>
        /// 发送byte流数据
        /// </summary>
        /// <param name="bts"></param>
        public void SendBts(byte[] bts)
        {
            //byte[] readBuffer = new byte[1024];
            //int bytesRec = socket.Receive(readBuffer);

            //if (bytesRec == 0)
            //{
            //    Console.WriteLine("掉线");
            //}
            //if (socket==null)
            //{
            //    Console.WriteLine("发送失败,玩家已离线，套接字不存在");
            //    return;
            //}
            socket.Send(bts);
        }
    }
}
