using System;
using System.Collections.Generic;


namespace ConsoleApp1
{
    class TypeDo
    {
        public static Dictionary<int,string> Types = new Dictionary<int, string>();
        /// <summary>
        /// 任务队列（用于主线程处理任务）
        /// </summary>
        private static Queue<Command> CommandQueue = new Queue<Command>();

        public static void AddType()
        {
            Types.Add(1, "ConsoleApp1.LoginCommand");
            Types.Add(2, "ConsoleApp1.ChatCommad");
            Types.Add(3, "ConsoleApp1.SelectFriendsCommand");
            Types.Add(4, "ConsoleApp1.HeartbeatCommand");
            Types.Add(5, "ConsoleApp1.RegisteCommand");
            Types.Add(6, "ConsoleApp1.RankCommand");
            Types.Add(7, "ConsoleApp1.RoomCommand");
            Types.Add(8, "ConsoleApp1.GameCommand");
        }

        public static void DoType(Byte[] bts, Conn cnn )
        {
            string strClass;
            Types.TryGetValue(BitConverter.ToInt32(bts,0), out strClass);
            //if (BitConverter.ToInt32(Decode.DecodHeadBtyes(bts), 0)!=4)
            //Console.WriteLine(BitConverter.ToInt32(bts,0)+"命令:"+ strClass);
            Type t = Type.GetType(strClass);//根据名字获取类型（命令）
            Command command = Activator.CreateInstance(t, true) as Command;//实例化指定命令类型的对象
            command.Init(bts, cnn);//传值
            //command.DoCommand();//执行

            lock (CommandQueue)
            {
                CommandQueue.Enqueue(command);//入队
            }
        }

        /// <summary>
        /// 处理所有命令
        /// </summary>
        public static void ProcessAllCommand()
        {
            while (true)
            {
                Command command = null;
                lock (CommandQueue)
                {
                    if (CommandQueue.Count > 0)
                        command = CommandQueue.Dequeue();//出队
                }
                if (command != null) command.DoCommand();
                else break;
            }
        }
    }
}
