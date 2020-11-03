using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class SelectFriendsCommand : Command
    {
        readonly int type = (int)FirstCommands.SelectFriendsCommand;

        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()
        {
            int id = BitConverter.ToInt32(bytes,8);
            string strContent = "SELECT * from  friends right JOIN counter ON friends.f_id=counter.id where friends.user_id = " + id + " ORDER BY status DESC;";
            List<PersonalInfo> friends = SqlConn.FindFriends(strContent);
            Console.WriteLine("用户:" + id + "的好友数:" + friends.Count);
            conn.SendBts(Incode.IncodeFirstCommand(type, DataDo.Object2Json(friends)));
        }
    }
}
