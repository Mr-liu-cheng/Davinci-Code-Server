using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public struct RoomMsg
    {
        public string roomNum;
        public PersonalInfo my;
        public int otherId;
        public int myId;
    }

    public class RoomCommand : Command
    {
        readonly static int type = (int)FirstCommands.RoomCommand;//查找所有房间命令
        public static Dictionary<string, RoomInfo> allRooms = new Dictionary<string, RoomInfo>();//增删，状态筛选  
        public static Dictionary<string,RoomInfo> freeRooms = new Dictionary<string, RoomInfo>();//没开始游戏且空余的房间   增删，状态筛选 noStartRooms 独立增删，allRooms 增删影响到 noStartRooms

        /// <summary>
        /// 命令类
        /// </summary>
        public enum SecondCommands
        {
            SELECTROOM = 1, CREATEROOM, TurnBackROOM, JOINROOM, EXITROOM, HOSTRANSFER, DISMISSROOM,
            INVITEROOM, REMOVEFROMROOM, ADDFRIEND, GAINFRIENDLIST, AMENDROOMNAME, BEGINGAME
        };

        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()
        {
            int command = BitConverter.ToInt32(bytes, 8);
            switch (command)
            {
                case 1:
                    Console.WriteLine("查找房间");
                    SelectRoom();
                    break;
                case 2:
                    Console.WriteLine("开房");
                    CreatRoom();
                    break;
                case 3:
                    Console.WriteLine("回到当前房间【游戏结束】");
                    ReturnRoom();
                    break;
                case 4:
                    Console.WriteLine("加入房间");
                    JoinRoom();
                    break;
                case 5:
                    Console.WriteLine("退出房间");
                    ExitRoom();
                    break;
                case 6:
                    Console.WriteLine("房主转让");
                    HostTransfer();
                    break;
                case 8:
                    Console.WriteLine("邀请加入");//如果玩家同意才发送加入命令
                    InviteToRoom();
                    break;
                case 9:
                    Console.WriteLine("踢出房间");
                    RemoveFromRoom();
                    break;
                case 10:
                    Console.WriteLine("添加好友");
                    AddFriendFromRoom();
                    break;
                case 11:
                    Console.WriteLine("获取可以邀请好友列表");
                    GainFriendList();
                    break;
                case 12:
                    Console.WriteLine("修改房间名");
                    AmendRoomName();
                    break;
            }
        }

        /// <summary>
        /// 发送房间命令给自己
        /// </summary>
        /// <param name="byt">内容</param>
        /// <param name="secondCommand">二级命令</param>
        void SendRoom(byte[] byt,int secondCommand)
        {
            byte[] data = Incode.IncodeSecondaryCommand(type, secondCommand, byt);
            conn.SendBts(data);
        }

        /// <summary>
        /// 遍历房间成员发送房间命令
        /// </summary>
        /// <param name="byt">内容</param>
        /// <param name="secondCommand">二级命令</param>
        public static void ForeachSendRoom(int secondCommand,RoomInfo roomInfo)
        {
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                //Console.WriteLine();
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                conn.SendBts(Incode.IncodeSecondaryCommand(type, secondCommand, DataDo.Object2Json(roomInfo)));
            }
        }

        void ReturnRoom()
        {
            Info info = DataDo.Json2Object<Info>(Decode.DecodeSecondContendBtyes(bytes));
            RoomInfo roomInfo = GameCommand.GetRoom(info.roomId);
            if (roomInfo != null)
            {
                PersonalInfo personal=  roomInfo.member.Find(it=> {
                    if (it.id==info.myId)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
                if (personal!=null)
                {
                    personal.IsInWaitRoom = true;
                }
                //在房间的都知道
       
                conn.SendBts(Incode.IncodeSecondaryCommand(type, (int)SecondCommands.TurnBackROOM, DataDo.Object2Json(roomInfo)));
                Conn _conn;
                for (int i = 0; i < roomInfo.member.Count; i++)
                {
                    if (roomInfo.member[i].id!=info.myId)
                    {
                        Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out _conn);
                        _conn.SendBts(Incode.IncodeSecondaryCommand(type, (int)SecondCommands.JOINROOM, DataDo.Object2Json(roomInfo)));
                    }
                }
            }
        }

        /// <summary>
        /// 查找所有闲置的房间 （间隔刷新）
        /// </summary>
        public void SelectRoom()
        {
            SendRoom(DataDo.Object2Json(freeRooms), (int)SecondCommands.SELECTROOM);
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        public void CreatRoom()
        {
            RoomInfo roomInfo = DataDo.Json2Object<RoomInfo>(Decode.DecodeSecondContendBtyes(bytes));
            roomInfo.CreateExitList();
            allRooms.Add(roomInfo.roomID,roomInfo);

            //房主信息修改
            PersonalInfo.ChangeStatusInfo(roomInfo.member[0].id, roomInfo.roomID,(int)PersonStatus.Combine);

            Console.WriteLine("  房间ID： " + roomInfo.roomID);
            Console.WriteLine("allRooms:" + allRooms.Count + "   noStartRooms:" + freeRooms.Count);
        }

        /// <summary>
        /// 加入房间（及时响应）
        /// </summary>
        void JoinRoom()
        {
            RoomMsg roomMsg = DataDo.Json2Object<RoomMsg>(Decode.DecodeSecondContendBtyes(bytes));
            RoomInfo roomInfo = GameCommand.GetRoom(roomMsg.roomNum);
            if (roomInfo != null)
            {
                PersonalInfo.ChangeStatusInfo(roomMsg.my.id, roomInfo.roomID, (int)PersonStatus.Combine);
                roomMsg.my.IsInWaitRoom = true;
                roomInfo.AddMember(roomMsg.my);
                if (roomInfo.member.Count == RoomInfo.MaxSIZE)//房间人满
                {
                    freeRooms.Remove(roomInfo.roomID);
                }
                ForeachSendRoom((int)SecondCommands.JOINROOM, roomInfo);
                Console.WriteLine("加入成功");
            }
        }

        /// <summary>
        /// 退出房间
        /// </summary>
        void ExitRoom()
        {
            RoomMsg roomMsg = DataDo.Json2Object<RoomMsg>(Decode.DecodeSecondContendBtyes(bytes));
            RoomInfo roomInfo = GameCommand.GetRoom(roomMsg.roomNum);
            if (roomInfo != null)
            {
                PersonalInfo.ChangeStatusInfo(roomMsg.otherId, "", (int)PersonStatus.OnLine);

                PersonalInfo people = roomInfo.member.Find(it =>
                {
                    if (it.id == roomMsg.otherId)//找到要删的人
                    { return true; }
                    else return false;
                });
                roomInfo.RemoveMember(people.id);
                //if (roomMsg.otherId == roomInfo.host_Id && roomInfo.member.Count>0)//退出者的ID为房主
                //{
                //    roomInfo.host_Id = roomInfo.member[0].id;//房主转移
                //}
                Console.WriteLine("剩余人数："+ roomInfo.member.Count);
            }
        }



        /// <summary>
        /// 房主转让
        /// </summary>
        public static void HostTransfer()
        {

        }

        /// <summary>
        /// 解散房间(当房间没人就删掉，服务器执行)
        /// </summary>
        public static void DismissRoom()
        {

        }

        /// <summary>
        /// 邀请加入
        /// </summary>
        public static void InviteToRoom()
        {

        }

        /// <summary>
        /// 踢出房间
        /// </summary>
        public static void RemoveFromRoom()
        {

        }

        /// <summary>
        /// 添加好友
        /// </summary>
        public static void AddFriendFromRoom()
        {

        }

        /// <summary>
        /// 获取可以邀请好友列表
        /// </summary>
        public static void GainFriendList()
        {

        }

        /// <summary>
        /// 修改房间名
        /// </summary>
        public static void AmendRoomName()
        {

        }
    }
}
