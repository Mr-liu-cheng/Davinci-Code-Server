using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;

namespace ConsoleApp1
{
    class RegisteCommand : Command
    {
        readonly int type = (int)FirstCommands.RegisteCommand;

        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()
        {
            LoginInfo myInfo = DataDo.Json2Object<LoginInfo>(Decode.DecodeFirstContendBtyes(bytes)); 
            string id = SqlConn.Insert(myInfo.userName, myInfo.passWord); // 添加到数据库 通过访问数据库获取自动分配的id   
            Server.AddUser(id, conn); //添加到服务器连接字典里去
            conn.ID = id;        //客户端的id作为属性存起来了便于访问  
            //Console.WriteLine("获取id：" + id);
            string strContent = "SELECT * FROM counter where id=" + id + " ;";
            PersonalInfo user = SqlConn.Select(strContent);
            if (user != null)
            {
                //Console.WriteLine("用户登陆成功");
                user.status = (int)PersonStatus.OnLine;//在线(发送数据修改)
                PersonalInfo.ChangeStatusInfo(int.Parse(conn.ID), "", (int)PersonStatus.OnLine);
                conn.SendBts(Incode.IncodeFirstCommand(type, DataDo.Object2Json(user)));
            }

        }
    }
}
