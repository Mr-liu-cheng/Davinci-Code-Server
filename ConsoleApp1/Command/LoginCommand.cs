using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ConsoleApp1
{
    class LoginCommand : Command
    {
        readonly int type = (int)FirstCommands.LoginCommand;

        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()
        {
            byte[] content = Decode.DecodeFirstContendBtyes(bytes);
            LoginInfo myInfo = DataDo.Json2Object<LoginInfo>(content);
            string strContent = "SELECT * FROM counter where id=" + myInfo.id + " ;";
            PersonalInfo user= SqlConn.Select(strContent);
            if (user != null)
            {
                if(user.password == myInfo.passWord)
                {
                    Console.WriteLine("用户登陆成功");
                    Server.AddUser(myInfo.id, conn);
                    conn.ID = myInfo.id;              //客户端的id作为属性存起来了便于访问

                    user.status = (int)PersonStatus.OnLine;//在线
                    PersonalInfo.ChangeStatusInfo(int.Parse(conn.ID),"",(int)PersonStatus.OnLine);
                    conn.SendBts(Incode.IncodeFirstCommand(type, DataDo.Object2Json(user)));
                }
                else
                {
                    Console.WriteLine("密码错误");
                    //回客户端消息
                }
            }
            else
            {
                Console.WriteLine("账号不存在");
                //回客户端消息
            }
        }
    }
}
