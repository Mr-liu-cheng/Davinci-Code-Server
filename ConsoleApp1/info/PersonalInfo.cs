using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public enum PersonStatus
    {
        OffLine,
        OnLine,
        Combine,
        Fighting
    }

    public class PersonalInfo
    //id  PassWord  name markName mileage  icon sex age winRate serialWin  gameNum degree status roomNum  13个
    {
        public int id;
        public string password;
        public string name;
        public string markName;
        public string icon;
        public int sex;//0女1男，null未知
        public int age;
        public float winRate;
        public int serialWin;//最高连胜
        public int gameNum;//对局场次
        public int degree;//等级
        public int status;
        public string roomNum;
        public int coin;
        public bool IsInWaitRoom;

        /// <summary>
        /// 改变状态信息【退出房间都不会修改玩家房间号，下次进入房间会自动更替】
        /// </summary>
        public static void ChangeStatusInfo(int id, string roomId, int status)
        {
            string where = "id = '" + id + "'";
            string roomCondition = "";
            if (status == (int)PersonStatus.Combine || status == (int)PersonStatus.Fighting)
            {
                roomCondition = "roomNum = '" + roomId + "',";
            }
            SqlConn.AlterInfo("counter", roomCondition + "status='" + status + "'", where);
        }
    }
}
