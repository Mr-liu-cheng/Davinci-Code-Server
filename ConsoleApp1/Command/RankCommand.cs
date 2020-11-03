using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class RankCommand : Command
    {
        readonly int type = (int)FirstCommands.RankCommand;
        //排行榜分 好友排行榜 + 世界排行榜

        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()//世界排行榜    还有一个没做
        {
            List<PersonalInfo> rank = SqlConn.GetRank();//直接向数据库查询 获取排行榜 
            conn.SendBts(Incode.IncodeFirstCommand(type, DataDo.Object2Json(rank)));
        }
    }
}
