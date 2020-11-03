using System.Threading;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        /// <summary>
        /// 刷新一帧所需时间
        /// </summary>
        const int FrameTimeMs = (int)(1 / 30.0f * 1000);
        static OffLineDetection g_heartTime = new OffLineDetection();

        static void Main(string[] args)
        {
            SqlConn.ConnectDatabase();
            TypeDo.AddType();
            Server srv = new Server();
            srv.Start("192.168.1.5", 62345); //127.0.0.1
            SqlConn.InitializePersenInfo();
            while (true)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                FrameUpdate();

                stopwatch.Stop();

                long sleepTime = FrameTimeMs - stopwatch.ElapsedMilliseconds;
               // System.Console.WriteLine(stopwatch.ElapsedMilliseconds);
                if (sleepTime > 0)//执行时间测试
                    Thread.Sleep((int)sleepTime);
            }
        }

        /// <summary>
        /// 执行方法
        /// 执行命令处理（所有（处理消息）命令放到主线程处理，接收消息放到多线程处理）
        /// </summary>
        /// <param name="heartTime"></param>
        static void FrameUpdate()
        {
            g_heartTime.Update();
            TypeDo.ProcessAllCommand();
        }
    }
}
